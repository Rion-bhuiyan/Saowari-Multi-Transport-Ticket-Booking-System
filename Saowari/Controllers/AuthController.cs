using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Auth;
using Saowari.Models.DTOs.User;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Security.Claims;
using Saowari.Services;
using System.Text.Json;
using System.Linq;
using System.Net.Http;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SaowariDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        private readonly IEmailService _emailService;

        public AuthController(SaowariDbContext context, IJwtService jwtService, IMapper mapper, INotificationService notificationService, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _mapper = mapper;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Email already exists."));
            }

            var userRole = await _context.UserRoles.FindAsync(dto.RoleID);
            if (userRole == null)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid Role ID."));
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleID = dto.RoleID,
                UserRole = userRole
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var otpCode = new Random().Next(100000, 999999).ToString();
            user.RegistrationOtpCode = otpCode;
            user.RegistrationOtpExpireTime = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: 0 auto; border: 1px solid #eee; border-radius: 8px;'>
                    <h2 style='color: #0369a1;'>Welcome to Saowari, {user.FullName}!</h2>
                    <p>Please verify your email address to complete your registration.</p>
                    <p>Your 6-digit verification code is:</p>
                    <h3 style='background-color: #f3f4f6; padding: 12px; display: inline-block; letter-spacing: 4px; font-size: 24px; color: #1d4ed8; border-radius: 4px;'>{otpCode}</h3>
                    <p style='color: #6b7280; font-size: 14px;'>This code will expire in 15 minutes.</p>
                </div>";
            
            var textBody = $"Welcome to Saowari! Your verification code is: {otpCode}";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Verify your Saowari account", emailBody, textBody);
                await _notificationService.NotifyNewUserRegisteredAsync(user);
            }
            catch (System.Exception) { /* Fail-safe */ }

            return Ok(ApiResponse<AuthResponseDto>.Fail("OTP_REQUIRED"));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);


            if (user == null)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid credentials."));
            }

            if (!user.IsActive)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("User account is inactive."));
            }

            // Check if account is locked out
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                if (user.OtpExpireTime == null || user.OtpExpireTime < DateTime.UtcNow)
                {
                    user.OtpCode = new Random().Next(100000, 999999).ToString();
                    user.OtpExpireTime = DateTime.UtcNow.AddMinutes(15);
                    await _context.SaveChangesAsync();

                    var alertBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2>Account Locked</h2>
                            <p>Hello {user.FullName},</p>
                            <p>Your account remains locked due to multiple failed login attempts.</p>
                            <p>To unlock your account, please use the following 6-digit OTP code:</p>
                            <h3 style='background-color: #f3f4f6; padding: 10px; display: inline-block; letter-spacing: 2px;'>{user.OtpCode}</h3>
                        </div>";

                    var plainAlert = $"Your account is locked. Your unlock OTP is {user.OtpCode}.";

                    try
                    {
                        await _emailService.SendEmailAsync(user.Email, "Security Alert: Account Locked", alertBody, plainAlert);
                    }
                    catch { /* Fail-safe */ }
                }
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Account locked. Please check your email for the unlock code."));
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(30); // Lock for 30 mins or until OTP
                    user.OtpCode = new Random().Next(100000, 999999).ToString();
                    user.OtpExpireTime = DateTime.UtcNow.AddMinutes(15);
                    
                    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                    var emailIp = ip == "::1" ? "Localhost (127.0.0.1)" : ip;
                    var emailDevice = ParseUserAgent(HttpContext.Request.Headers["User-Agent"].ToString());
                    var locationInfo = await GetLocationFromIpAsync(ip);

                    var alertBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2>Suspicious Login Attempts</h2>
                            <p>Hello {user.FullName},</p>
                            <p>Your account has been locked due to 5 consecutive failed login attempts from:</p>
                            <ul>
                                <li><b>IP Address:</b> {emailIp}</li>
                                <li><b>Location:</b> {locationInfo}</li>
                                <li><b>Device:</b> {emailDevice}</li>
                                <li><b>Time:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                            </ul>
                            <p>To unlock your account immediately, please use the following 6-digit OTP code:</p>
                            <h3 style='background-color: #f3f4f6; padding: 10px; display: inline-block; letter-spacing: 2px;'>{user.OtpCode}</h3>
                            <p>If this was not you, someone is trying to access your account. Your password remains safe.</p>
                        </div>";

                    var plainAlert = $"Your account has been locked due to 5 failed logins from IP {emailIp}, Device: {emailDevice}. Your unlock OTP is {user.OtpCode}.";

                    try
                    {
                        await _emailService.SendEmailAsync(user.Email, "Security Alert: Account Locked", alertBody, plainAlert);
                    }
                    catch { /* Fail-safe */ }
                }

                await _context.SaveChangesAsync();
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(user.FailedLoginAttempts >= 5 ? "Account locked due to 5 failed attempts. Please check your email for the unlock code." : "Invalid credentials."));
            }

            // Successful login -> Reset failed attempts
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.OtpCode = null;
            user.OtpExpireTime = null;

            if (!user.IsEmailVerified)
            {
                // Generate a new registration OTP if needed
                var otpCode = new Random().Next(100000, 999999).ToString();
                user.RegistrationOtpCode = otpCode;
                user.RegistrationOtpExpireTime = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: 0 auto; border: 1px solid #eee; border-radius: 8px;'>
                        <h2 style='color: #0369a1;'>Verify Your Email</h2>
                        <p>You need to verify your email address before logging in.</p>
                        <p>Your 6-digit verification code is:</p>
                        <h3 style='background-color: #f3f4f6; padding: 12px; display: inline-block; letter-spacing: 4px; font-size: 24px; color: #1d4ed8; border-radius: 4px;'>{otpCode}</h3>
                        <p style='color: #6b7280; font-size: 14px;'>This code will expire in 15 minutes.</p>
                    </div>";
                var textBody = $"Your verification code is: {otpCode}";

                try
                {
                    await _emailService.SendEmailAsync(user.Email, "Verify your Saowari account", emailBody, textBody);
                }
                catch (System.Exception) { /* Fail-safe */ }

                return Ok(ApiResponse<AuthResponseDto>.Fail("UNVERIFIED_EMAIL_OTP_SENT"));
            }

            // Track New Device Login
            var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
            var currentDevice = HttpContext.Request.Headers["User-Agent"].ToString();
            var deviceId = HttpContext.Request.Headers["X-Device-Id"].ToString();
            
            if (!string.IsNullOrEmpty(deviceId))
            {
                currentDevice = $"{currentDevice} [ID:{deviceId}]";
            }

            var previousLogin = await _context.UserLoginHistories
                .Where(h => h.UserId == user.UserID && h.IpAddress == currentIp && h.DeviceName == currentDevice)
                .FirstOrDefaultAsync();

            if (previousLogin == null)
            {
                var emailCurrentIp = currentIp == "::1" ? "Localhost (127.0.0.1)" : currentIp;
                var emailCurrentDevice = ParseUserAgent(currentDevice);
                var locationInfo = await GetLocationFromIpAsync(currentIp);

                // New device/IP — generate a login OTP and block login until verified
                var loginOtp = new Random().Next(100000, 999999).ToString();
                user.LoginOtpCode = loginOtp;
                user.LoginOtpExpireTime = DateTime.UtcNow.AddMinutes(10);
                await _context.SaveChangesAsync();

                var otpHtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: 0 auto; background: #fff; border: 1px solid #e5e7eb; border-radius: 8px;'>
                        <h2 style='color: #dc2626;'>🔐 New Login Attempt Detected</h2>
                        <p>Hello {user.FullName},</p>
                        <p>Someone (possibly you) is trying to log into your Saowari account from a <strong>new device or browser</strong>:</p>
                        <ul>
                            <li><b>IP Address:</b> {emailCurrentIp}</li>
                            <li><b>Location:</b> {locationInfo}</li>
                            <li><b>Device:</b> {emailCurrentDevice}</li>
                            <li><b>Time:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                        </ul>
                        <p>To complete login, enter the following verification code on the login page:</p>
                        <div style='text-align:center; margin: 20px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; background: #f3f4f6; padding: 12px 24px; border-radius: 8px; color: #1d4ed8;'>{loginOtp}</span>
                        </div>
                        <p style='color: #6b7280; font-size: 13px;'>This code expires in <strong>10 minutes</strong>. If this wasn't you, please change your password immediately.</p>
                    </div>";

                var otpPlainBody = $"New login attempt on your Saowari account from {emailCurrentDevice} ({emailCurrentIp}). Your verification code is: {loginOtp}. Expires in 10 minutes.";

                Console.WriteLine($"\n=======================================================");
                Console.WriteLine($"SECURITY ALERT - NEW DEVICE LOGIN OTP GENERATED");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"OTP Code: {loginOtp}");
                Console.WriteLine($"=======================================================\n");

                try { await _emailService.SendEmailAsync(user.Email, "Security Alert: Verify Your Login - Saowari", otpHtmlBody, otpPlainBody); }
                catch (Exception ex) { 
                    Console.WriteLine($"WARNING: Failed to send Login OTP email: {ex.Message}");
                }

                // Return special response telling the frontend to ask for OTP
                return Ok(ApiResponse<AuthResponseDto>.Fail("NEW_DEVICE_OTP_REQUIRED"));
            }

            // Record this login with full tracking info
            var geoInfo = await GetGeoInfoAsync(currentIp);
            var referrer = dto.Referrer ?? HttpContext.Request.Headers["Referer"].ToString();
            _context.UserLoginHistories.Add(new UserLoginHistory
            {
                UserId = user.UserID,
                IpAddress = currentIp,
                DeviceName = currentDevice,
                LoginTime = DateTime.UtcNow,
                Country = geoInfo.Country,
                CountryCode = geoInfo.CountryCode,
                City = geoInfo.City,
                IspName = geoInfo.Isp,
                Referrer = referrer,
                TrafficChannel = ParseTrafficChannel(referrer),
                Browser = ParseBrowserName(HttpContext.Request.Headers["User-Agent"].ToString())
            });

            return Ok(ApiResponse<AuthResponseDto>.Ok(BuildAuthResponse(user), "Login successful"));
        }

        [HttpPost("verify-login-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyLoginOtp([FromBody] VerifyLoginOtpDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid request."));

            if (user.LoginOtpCode != dto.OtpCode || user.LoginOtpExpireTime < DateTime.UtcNow)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid or expired OTP code."));

            // OTP verified — clear it and record this device
            user.LoginOtpCode = null;
            user.LoginOtpExpireTime = null;

            // Record as trusted device
            var currentDevice = HttpContext.Request.Headers["User-Agent"].ToString();
            var deviceId = HttpContext.Request.Headers["X-Device-Id"].ToString();
            if (!string.IsNullOrEmpty(deviceId))
            {
                currentDevice = $"{currentDevice} [ID:{deviceId}]";
            }

            var verifyIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var verifyGeoInfo = await GetGeoInfoAsync(verifyIp);
            var verifyReferrer = HttpContext.Request.Headers["Referer"].ToString();
            _context.UserLoginHistories.Add(new UserLoginHistory
            {
                UserId = user.UserID,
                IpAddress = verifyIp,
                DeviceName = currentDevice,
                LoginTime = DateTime.UtcNow,
                Country = verifyGeoInfo.Country,
                CountryCode = verifyGeoInfo.CountryCode,
                City = verifyGeoInfo.City,
                IspName = verifyGeoInfo.Isp,
                Referrer = verifyReferrer,
                TrafficChannel = ParseTrafficChannel(verifyReferrer),
                Browser = ParseBrowserName(HttpContext.Request.Headers["User-Agent"].ToString())
            });

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserResponseDto>(user)
            };

            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful"));
        }

        [HttpPost("verify-registration-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyRegistrationOtp([FromBody] VerifyRegistrationDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid request."));

            if (user.RegistrationOtpCode != dto.OtpCode || user.RegistrationOtpExpireTime < DateTime.UtcNow)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid or expired OTP code."));

            // OTP verified — clear it and set as verified
            user.RegistrationOtpCode = null;
            user.RegistrationOtpExpireTime = null;
            user.IsEmailVerified = true;

            // Also record device as trusted for future
            var currentDevice = HttpContext.Request.Headers["User-Agent"].ToString();
            var deviceId = HttpContext.Request.Headers["X-Device-Id"].ToString();
            if (!string.IsNullOrEmpty(deviceId))
            {
                currentDevice = $"{currentDevice} [ID:{deviceId}]";
            }

            _context.UserLoginHistories.Add(new UserLoginHistory
            {
                UserId = user.UserID,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                DeviceName = currentDevice,
                LoginTime = DateTime.UtcNow
            });

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserResponseDto>(user)
            };

            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Registration verified successfully"));
        }

        private AuthResponseDto BuildAuthResponse(User user)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpireTime <= DateTime.UtcNow)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token."));
            }

            var currentDevice = HttpContext.Request.Headers["User-Agent"].ToString();
            var deviceId = HttpContext.Request.Headers["X-Device-Id"].ToString();
            if (!string.IsNullOrEmpty(deviceId))
            {
                currentDevice = $"{currentDevice} [ID:{deviceId}]";
            }

            var activeSession = await _context.UserLoginHistories
                .Where(h => h.UserId == user.UserID && h.DeviceName == currentDevice)
                .OrderByDescending(h => h.LoginTime)
                .FirstOrDefaultAsync();

            if (activeSession != null && !activeSession.IsActive)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpireTime = null;
                await _context.SaveChangesAsync();
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Session has been remotely logged out."));
            }

            if (activeSession != null)
            {
                activeSession.LastActiveTime = DateTime.UtcNow;
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserResponseDto>(user)
            };

            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Token refreshed successfully"));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.Fail("Invalid user token."));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpireTime = null;

                var currentDevice = HttpContext.Request.Headers["User-Agent"].ToString();
                var deviceId = HttpContext.Request.Headers["X-Device-Id"].ToString();
                if (!string.IsNullOrEmpty(deviceId))
                {
                    currentDevice = $"{currentDevice} [ID:{deviceId}]";
                }

                var activeSession = await _context.UserLoginHistories
                    .Where(h => h.UserId == user.UserID && h.DeviceName == currentDevice)
                    .OrderByDescending(h => h.LoginTime)
                    .FirstOrDefaultAsync();

                if (activeSession != null)
                {
                    activeSession.IsActive = false;
                }

                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<bool>.Ok(true, "Logout successful"));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.Fail("Invalid user token."));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.Fail("User not found."));
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                return BadRequest(ApiResponse<bool>.Fail("Incorrect old password."));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            try 
            {
                await _notificationService.NotifySystemEventAsync("User Password Changed", $"User {user.FullName} ({user.Email}) has changed their password from inside the app.");
            } catch { /* fail safe */ }

            return Ok(ApiResponse<bool>.Ok(true, "Password changed successfully"));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                // Always return success to prevent email enumeration
                return Ok(ApiResponse<bool>.Ok(true, "If your email is registered, you will receive a password reset link shortly."));
            }

            // Using localhost instead of 127.0.0.1 because Angular ng serve binds to localhost
            var resetToken = Guid.NewGuid().ToString();
            var resetLink = $"http://localhost:4200/auth/reset-password?email={dto.Email}&token={resetToken}";

            var htmlBody = $@"
                <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 30px; background-color: #ffffff; border: 1px solid #e1e4e8; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05);'>
                    <div style='text-align: center; margin-bottom: 25px;'>
                        <h2 style='color: #0369a1; margin: 0; font-size: 24px;'>Saowari Account Recovery</h2>
                    </div>
                    <p style='color: #334155; font-size: 16px; line-height: 1.5; margin-bottom: 20px;'>Hello {user.FullName},</p>
                    <p style='color: #334155; font-size: 16px; line-height: 1.5; margin-bottom: 20px;'>We received a request to reset the password for your Saowari account associated with this email address. If you made this request, please click the secure button below to set a new password.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='display: inline-block; padding: 14px 28px; background-color: #0284c7; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; letter-spacing: 0.5px;'>Reset Password</a>
                    </div>
                    <p style='color: #475569; font-size: 14px; line-height: 1.5; margin-bottom: 10px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
                    <p style='color: #0284c7; font-size: 13px; word-break: break-all; margin-bottom: 30px;'>{resetLink}</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin-bottom: 20px;' />
                    <p style='color: #64748b; font-size: 12px; line-height: 1.5; text-align: center;'>If you did not request a password reset, you can safely ignore this email. Your password will remain unchanged.</p>
                    <p style='color: #64748b; font-size: 11px; line-height: 1.5; text-align: center; margin-top: 10px;'>Request ID: {Guid.NewGuid().ToString().Substring(0, 8)} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                </div>";

            var textBody = $@"Saowari Account Recovery
            
Hello {user.FullName},

We received a request to reset your password. Please copy and paste the link below into your browser to choose a new password:
{resetLink}

If you did not request this, please ignore this email. Your password will remain unchanged.";

            try
            {
                var dynamicSubject = $"Password Reset Request - Saowari - {DateTime.Now:HH:mm:ss}";
                await _emailService.SendEmailAsync(dto.Email, dynamicSubject, htmlBody, textBody);
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return StatusCode(500, ApiResponse<bool>.Fail("Failed to send email. " + ex.Message));
            }

            return Ok(ApiResponse<bool>.Ok(true, "If your email is registered, you will receive a password reset link shortly."));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid request."));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            
            try 
            {
                await _notificationService.NotifySystemEventAsync("User Password Reset", $"User {user.FullName} ({user.Email}) has reset their password.");
            } catch { /* fail safe */ }

            return Ok(ApiResponse<bool>.Ok(true, "Password has been reset successfully."));
        }

        [HttpPost("unlock-account")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> UnlockAccount([FromBody] UnlockAccountDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid request."));
            }

            if (user.OtpCode != dto.OtpCode || user.OtpExpireTime < DateTime.UtcNow)
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid or expired OTP code."));
            }

            // Unlock the account
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.OtpCode = null;
            user.OtpExpireTime = null;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Account unlocked successfully. You can now log in."));
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ResendOtp([FromBody] ResendOtpDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid request."));
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expireTime = DateTime.UtcNow.AddMinutes(10);
            
            if (dto.Type == "unlock")
            {
                user.OtpCode = otpCode;
                user.OtpExpireTime = expireTime;
                await _context.SaveChangesAsync();
                
                var alertBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2>Account Locked</h2>
                        <p>Hello {user.FullName},</p>
                        <p>To unlock your account, please use the following 6-digit OTP code:</p>
                        <h3 style='background-color: #f3f4f6; padding: 10px; display: inline-block; letter-spacing: 2px;'>{user.OtpCode}</h3>
                    </div>";
                var plainAlert = $"Your unlock OTP is {user.OtpCode}.";
                
                try { await _emailService.SendEmailAsync(user.Email, "Security Alert: Account Locked", alertBody, plainAlert); }
                catch { /* Fail-safe */ }
            }
            else if (dto.Type == "login")
            {
                user.LoginOtpCode = otpCode;
                user.LoginOtpExpireTime = expireTime;
                await _context.SaveChangesAsync();
                
                var otpHtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2>Login Verification</h2>
                        <p>Hello {user.FullName},</p>
                        <p>We detected a new login. Please use this 6-digit code:</p>
                        <h3 style='background-color: #f3f4f6; padding: 10px; display: inline-block; letter-spacing: 2px;'>{user.LoginOtpCode}</h3>
                    </div>";
                var otpPlainBody = $"Your login verification code is {user.LoginOtpCode}.";
                
                try { await _emailService.SendEmailAsync(user.Email, "Security Alert: Verify Your Login - Saowari", otpHtmlBody, otpPlainBody); }
                catch { /* Fail-safe */ }
            }
            else if (dto.Type == "registration")
            {
                user.RegistrationOtpCode = otpCode;
                user.RegistrationOtpExpireTime = expireTime;
                await _context.SaveChangesAsync();
                
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: 0 auto; border: 1px solid #eee; border-radius: 8px;'>
                        <h2 style='color: #0369a1;'>Welcome to Saowari, {user.FullName}!</h2>
                        <p>Please verify your email address to complete your registration.</p>
                        <p>Your 6-digit verification code is:</p>
                        <h3 style='background-color: #f3f4f6; padding: 12px; display: inline-block; letter-spacing: 4px; font-size: 24px; color: #1d4ed8; border-radius: 4px;'>{otpCode}</h3>
                        <p style='color: #6b7280; font-size: 14px;'>This code will expire in 15 minutes.</p>
                    </div>";
                
                var textBody = $"Welcome to Saowari! Your verification code is: {otpCode}";
                
                try { await _emailService.SendEmailAsync(user.Email, "Verify your Saowari account", emailBody, textBody); }
                catch { /* Fail-safe */ }
            }
            else
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid OTP type."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "OTP has been resent successfully."));
        }


        [HttpGet("sessions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetActiveSessions()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<IEnumerable<object>>.Fail("Invalid user token."));

            var currentDevice = HttpContext.Request.Headers["User-Agent"].ToString();
            var deviceId = HttpContext.Request.Headers["X-Device-Id"].ToString();
            if (!string.IsNullOrEmpty(deviceId))
            {
                currentDevice = $"{currentDevice} [ID:{deviceId}]";
            }

            // We only want unique devices, so let's group by DeviceName and get the latest
            var allSessions = await _context.UserLoginHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.LoginTime)
                .Take(50)
                .ToListAsync();

            var uniqueSessionsList = allSessions
                .GroupBy(h => h.DeviceName)
                .Select(g => g.First())
                .OrderByDescending(s => s.LoginTime)
                .ToList();

            var sessionTasks = uniqueSessionsList.Select(async h => new {
                h.Id,
                IpAddress = (h.IpAddress == "::1" || h.IpAddress == "127.0.0.1") ? "127.0.0.1 (Localhost)" : h.IpAddress,
                Location = await GetLocationFromIpAsync(h.IpAddress),
                DeviceName = ParseUserAgent(h.DeviceName),
                h.LoginTime,
                h.LastActiveTime,
                h.IsActive,
                IsCurrentDevice = h.DeviceName == currentDevice
            });

            var uniqueSessions = await Task.WhenAll(sessionTasks);

            return Ok(ApiResponse<IEnumerable<object>>.Ok(uniqueSessions, "Sessions fetched successfully"));
        }

        [HttpPost("revoke-session/{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeSession(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<bool>.Fail("Invalid user token."));

            var session = await _context.UserLoginHistories.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
            if (session == null)
            {
                return NotFound(ApiResponse<bool>.Fail("Session not found."));
            }

            session.IsActive = false;
            
            // Optionally set other sessions with same device name to inactive
            var otherSessions = await _context.UserLoginHistories
                .Where(h => h.UserId == userId && h.DeviceName == session.DeviceName && h.IsActive)
                .ToListAsync();
                
            foreach (var s in otherSessions) {
                s.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Session logged out successfully"));
        }

        private string ParseUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown Device";
            
            string browser = "Unknown Browser";
            if (userAgent.Contains("Edg/")) browser = "Microsoft Edge";
            else if (userAgent.Contains("Chrome/")) browser = "Google Chrome";
            else if (userAgent.Contains("Firefox/")) browser = "Mozilla Firefox";
            else if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/")) browser = "Apple Safari";
            else if (userAgent.Contains("OPR/") || userAgent.Contains("Opera/")) browser = "Opera";
            
            string os = "Unknown OS";
            if (userAgent.Contains("Windows NT 10.0")) os = "Windows 10/11";
            else if (userAgent.Contains("Windows NT 6.")) os = "Windows";
            else if (userAgent.Contains("Mac OS X")) os = "macOS";
            else if (userAgent.Contains("Android")) os = "Android";
            else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) os = "iOS";
            else if (userAgent.Contains("Linux")) os = "Linux";

            return $"{browser} on {os}";
        }

        private record GeoInfo(string Country, string CountryCode, string City, string Isp);

        private async Task<GeoInfo> GetGeoInfoAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(3);
                    var ipifyResponse = await client.GetStringAsync("https://api64.ipify.org?format=json");
                    using var ipDoc = JsonDocument.Parse(ipifyResponse);
                    ipAddress = ipDoc.RootElement.GetProperty("ip").GetString() ?? "";
                }
                catch { return new GeoInfo("Local", "LO", "Localhost", "Local Network"); }
            }
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(4);
                var json = await client.GetStringAsync($"https://get.geojs.io/v1/ip/geo/{ipAddress}.json");
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var country = root.TryGetProperty("country", out var c) ? c.GetString() ?? "" : "";
                var countryCode = root.TryGetProperty("country_code", out var cc) ? cc.GetString() ?? "" : "";
                var city = root.TryGetProperty("city", out var ci) ? ci.GetString() ?? "" : "";
                var isp = root.TryGetProperty("organization_name", out var org) ? org.GetString() ?? ""
                        : root.TryGetProperty("organization", out var org2) ? org2.GetString() ?? "" : "";
                return new GeoInfo(country, countryCode.ToUpper(), city, isp);
            }
            catch { return new GeoInfo("Unknown", "XX", "Unknown", "Unknown"); }
        }

        private string ParseTrafficChannel(string? referrer)
        {
            if (string.IsNullOrWhiteSpace(referrer)) return "Direct";
            var r = referrer.ToLower();
            if (r.Contains("google.") || r.Contains("bing.") || r.Contains("yahoo.") || r.Contains("duckduckgo.")) return "Organic Search";
            if (r.Contains("facebook.") || r.Contains("instagram.") || r.Contains("twitter.") || r.Contains("t.co") || r.Contains("youtube.") || r.Contains("linkedin.") || r.Contains("pinterest.") || r.Contains("telegram.") || r.Contains("tiktok.")) return "Social";
            if (r.Contains("email") || r.Contains("mail.") || r.Contains("newsletter") || r.Contains("substack.")) return "Email";
            return "Referral";
        }

        private string ParseBrowserName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            if (userAgent.Contains("Edg/")) return "Edge";
            if (userAgent.Contains("OPR/") || userAgent.Contains("Opera")) return "Opera";
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) return "Internet Explorer";
            return "Other";
        }

        private async Task<string> GetLocationFromIpAsync(string ipAddress)
        {
            var geo = await GetGeoInfoAsync(ipAddress);
            var location = string.Join(", ", new[] { geo.City, geo.Country }.Where(s => !string.IsNullOrEmpty(s) && s != "Unknown"));
            if (string.IsNullOrEmpty(location)) location = "Unknown Location";
            if (!string.IsNullOrEmpty(geo.Isp) && geo.Isp != "Unknown")
                location += $" (ISP: {geo.Isp})";
            return location;
        }
    }
}
