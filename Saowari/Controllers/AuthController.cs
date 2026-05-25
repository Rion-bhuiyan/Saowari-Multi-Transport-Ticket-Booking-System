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

        public AuthController(SaowariDbContext context, IJwtService jwtService, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _jwtService = jwtService;
            _mapper = mapper;
            _notificationService = notificationService;
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

            try
            {
                await _notificationService.NotifyNewUserRegisteredAsync(user);
            }
            catch (System.Exception) { /* Fail-safe */ }

            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Registration successful"));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid credentials."));
            }

            if (!user.IsActive)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("User account is inactive."));
            }

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

            return Ok(ApiResponse<bool>.Ok(true, "Password changed successfully"));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<bool>> ForgotPassword()
        {
            // Stub implementation
            return Ok(ApiResponse<bool>.Ok(true, "If your email is registered, you will receive a password reset link shortly."));
        }
    }
}
