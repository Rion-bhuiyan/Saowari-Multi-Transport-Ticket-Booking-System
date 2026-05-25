using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.User;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SaowariDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public UsersController(SaowariDbContext context, AutoMapper.IMapper mapper, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        /// <summary>Get all users (Admin only)</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAll()
        {
            var users = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<UserResponseDto>>.Ok(_mapper.Map<IEnumerable<UserResponseDto>>(users)));
        }

        /// <summary>Create a user (Admin only)</summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] UserAdminCreateDto dto)
        {
            var userRole = await _context.UserRoles.FindAsync(dto.RoleID);
            if (userRole == null) return BadRequest(ApiResponse<UserResponseDto>.Fail("Invalid Role ID."));

            var user = _mapper.Map<User>(dto);
            
            var passwordToUse = string.IsNullOrEmpty(dto.Password) ? "Saowari@123" : dto.Password;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordToUse);

            if (userRole.UserRoleName == "Driver")
            {
                if (string.IsNullOrEmpty(dto.LicenceNumber) || !dto.LicenceExpDate.HasValue)
                    return BadRequest(ApiResponse<UserResponseDto>.Fail("Licence Number and Expiry Date are required for Drivers."));

                var driverInfo = new DriverInformtion
                {
                    LicenceNumber = dto.LicenceNumber,
                    licenceExpDate = dto.LicenceExpDate.Value
                };
                _context.DriverInformtions.Add(driverInfo);
                await _context.SaveChangesAsync();
                user.DriverInformtionId = driverInfo.DriverInformtionId;
            }
            else if (userRole.UserRoleName == "Supervisor")
            {
                var supervisor = new Supervisor();
                _context.Supervisors.Add(supervisor);
                await _context.SaveChangesAsync();
                user.SupervisorId = supervisor.SupervisorId;
            }

            if (userRole.UserRoleName != "Customer")
            {
                user.CompanyId = dto.CompanyId;
            }
            else
            {
                user.CompanyId = null;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user = await _context.Users.Include(u => u.UserRole).Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == user.UserID);
            return CreatedAtAction(nameof(GetById), new { id = user!.UserID }, ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user), "User created"));
        }

        /// <summary>Update user (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(int id, [FromBody] UserAdminUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(ApiResponse<UserResponseDto>.Fail("User not found"));

            var userRole = await _context.UserRoles.FindAsync(dto.RoleID);
            if (userRole == null) return BadRequest(ApiResponse<UserResponseDto>.Fail("Invalid Role ID."));

            _mapper.Map(dto, user);

            if (userRole.UserRoleName == "Driver")
            {
                if (string.IsNullOrEmpty(dto.LicenceNumber) || !dto.LicenceExpDate.HasValue)
                    return BadRequest(ApiResponse<UserResponseDto>.Fail("Licence Number and Expiry Date are required for Drivers."));

                if (user.DriverInformtionId.HasValue)
                {
                    var driverInfo = await _context.DriverInformtions.FindAsync(user.DriverInformtionId.Value);
                    if (driverInfo != null)
                    {
                        driverInfo.LicenceNumber = dto.LicenceNumber;
                        driverInfo.licenceExpDate = dto.LicenceExpDate.Value;
                    }
                }
                else
                {
                    var driverInfo = new DriverInformtion
                    {
                        LicenceNumber = dto.LicenceNumber,
                        licenceExpDate = dto.LicenceExpDate.Value
                    };
                    _context.DriverInformtions.Add(driverInfo);
                    await _context.SaveChangesAsync();
                    user.DriverInformtionId = driverInfo.DriverInformtionId;
                }
                user.SupervisorId = null;
            }
            else if (userRole.UserRoleName == "Supervisor")
            {
                if (!user.SupervisorId.HasValue)
                {
                    var supervisor = new Supervisor();
                    _context.Supervisors.Add(supervisor);
                    await _context.SaveChangesAsync();
                    user.SupervisorId = supervisor.SupervisorId;
                }
                user.DriverInformtionId = null;
            }
            else
            {
                user.DriverInformtionId = null;
                user.SupervisorId = null;
            }

            if (userRole.UserRoleName != "Customer")
            {
                user.CompanyId = dto.CompanyId;
            }
            else
            {
                user.CompanyId = null;
            }

            await _context.SaveChangesAsync();

            user = await _context.Users.Include(u => u.UserRole).Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == user.UserID);
            return Ok(ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user), "User updated"));
        }

        /// <summary>Get user by Email (Admin only)</summary>
        [HttpGet("by-email/{email}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetByEmail(string email)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound(ApiResponse<UserResponseDto>.Fail("User not found"));
            return Ok(ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user)));
        }

        /// <summary>Get user by ID (Admin only)</summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(int id)
        {
            var user = await _context.Users.Include(u => u.UserRole).Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return NotFound(ApiResponse<UserResponseDto>.Fail("User not found"));
            return Ok(ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user)));
        }

        /// <summary>Get current logged-in user</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetMe()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var user = await _context.Users.Include(u => u.UserRole).Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound(ApiResponse<UserResponseDto>.Fail("User not found"));
            return Ok(ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user)));
        }

        /// <summary>Update own profile</summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateProfile([FromForm] UserUpdateDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(ApiResponse<UserResponseDto>.Fail("User not found"));

            _mapper.Map(dto, user);

            if (dto.PictureFile != null)
            {
                var webRoot = _env.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    webRoot = System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");
                }
                var uploadsFolder = System.IO.Path.Combine(webRoot, "uploads", "profiles");
                
                if (!System.IO.Directory.Exists(uploadsFolder))
                {
                    System.IO.Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = System.Guid.NewGuid().ToString() + "_" + dto.PictureFile.FileName;
                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await dto.PictureFile.CopyToAsync(fileStream);
                }

                // Assuming the app runs at root, e.g. /uploads/profiles/filename.jpg
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                user.Picture = $"{baseUrl}/uploads/profiles/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user), "Profile updated"));
        }

        /// <summary>Toggle user active status (Admin only)</summary>
        [HttpPatch("{id}/active")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> PatchActive(int id, [FromBody] bool isActive)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(ApiResponse<bool>.Fail("User not found"));
            user.IsActive = isActive;
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Status updated"));
        }

        /// <summary>Assign a role to a user (Admin only)</summary>
        [HttpPatch("{id}/role")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignRole(int id, [FromBody] int roleId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(ApiResponse<bool>.Fail("User not found"));
            user.RoleID = roleId;
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Role assigned"));
        }

        /// <summary>Soft delete user (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(ApiResponse<bool>.Fail("User not found"));
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "User deactivated"));
        }
    }
}