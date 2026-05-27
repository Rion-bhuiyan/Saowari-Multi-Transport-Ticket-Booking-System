using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.DTOs.Notification;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly SaowariDbContext _context;
        private readonly IMapper _mapper;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;

        public NotificationsController(
            SaowariDbContext context, 
            IMapper mapper, 
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, 
            Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
            _scopeFactory = scopeFactory;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User is not authenticated properly.");
        }

        /// <summary>Get all notifications for the logged-in user</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetAll()
        {
            try
            {
                var userId = GetCurrentUserId();

                var notifications = await _context.Notifications
                    .Include(n => n.Company)
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var dtos = _mapper.Map<List<NotificationDto>>(notifications);
                return Ok(ApiResponse<List<NotificationDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<NotificationDto>>.Fail(ex.Message));
            }
        }

        /// <summary>Get unread notification count for the logged-in user</summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
                return Ok(ApiResponse<int>.Ok(count));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<int>.Fail(ex.Message));
            }
        }

        /// <summary>Mark a single notification as read</summary>
        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                if (notification == null) return NotFound(ApiResponse<bool>.Fail("Notification not found or access denied"));

                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.Ok(true, "Marked as read"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        /// <summary>Mark all notifications as read for the logged-in user</summary>
        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var unread = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
                unread.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.Ok(true, "All marked as read"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        /// <summary>Delete a notification</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                if (notification == null) return NotFound(ApiResponse<bool>.Fail("Notification not found or access denied"));

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.Ok(true, "Notification deleted"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        /// <summary>Clear all notifications for the logged-in user</summary>
        [HttpDelete("clear-all")]
        public async Task<ActionResult<ApiResponse<bool>>> ClearAll()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.Ok(true, "All notifications cleared"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        // ─── Admin Preferences Toggle Endpoints ───────────────────────────────

        /// <summary>Get admin notification preferences for all companies</summary>
        [HttpGet("admin-preferences")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<AdminNotificationPreferenceDto>>>> GetAdminPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Get all active companies in the system
                var companies = await _context.Companies.Where(c => c.IsActive).ToListAsync();
                
                // Get existing preferences for this admin
                var existingPrefs = await _context.AdminNotificationPreferences
                    .Where(p => p.AdminUserId == userId)
                    .ToListAsync();

                var result = new List<AdminNotificationPreferenceDto>();
                foreach (var company in companies)
                {
                    var pref = existingPrefs.FirstOrDefault(p => p.CompanyId == company.CompanyID);
                    result.Add(new AdminNotificationPreferenceDto
                    {
                        Id = pref?.Id ?? 0,
                        AdminUserId = userId,
                        CompanyId = company.CompanyID,
                        CompanyName = company.CompanyName,
                        IsEnabled = pref?.IsEnabled ?? true // Defaults to true
                    });
                }

                return Ok(ApiResponse<List<AdminNotificationPreferenceDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<AdminNotificationPreferenceDto>>.Fail(ex.Message));
            }
        }

        /// <summary>Toggle preference for a company</summary>
        [HttpPut("admin-preferences/{companyId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> TogglePreference(int companyId, [FromBody] UpdateNotificationPreferenceDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();

                var companyExists = await _context.Companies.AnyAsync(c => c.CompanyID == companyId);
                if (!companyExists) return NotFound(ApiResponse<bool>.Fail("Company not found"));

                var pref = await _context.AdminNotificationPreferences
                    .FirstOrDefaultAsync(p => p.AdminUserId == userId && p.CompanyId == companyId);

                if (pref == null)
                {
                    pref = new AdminNotificationPreference
                    {
                        AdminUserId = userId,
                        CompanyId = companyId,
                        IsEnabled = dto.IsEnabled
                    };
                    await _context.AdminNotificationPreferences.AddAsync(pref);
                }
                else
                {
                    pref.IsEnabled = dto.IsEnabled;
                }

                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.Ok(true, $"Notification preference updated successfully. Status: {dto.IsEnabled}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<bool>>> BroadcastMessage([FromForm] BroadcastRequestDto dto)
        {
            try
            {
                var adminUserId = GetCurrentUserId();

                string? imageUrl = null;
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    var uploadsFolder = System.IO.Path.Combine(_env.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "broadcasts");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.Image.FileName;
                    var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(fileStream);
                    }

                    // Get base URL
                    var request = HttpContext.Request;
                    var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                    imageUrl = $"{baseUrl}/uploads/broadcasts/{uniqueFileName}";
                }

                // Query users matching roles
                var usersQuery = _context.Users.AsQueryable();
                if (dto.TargetRoleIds != null && dto.TargetRoleIds.Any())
                {
                    usersQuery = usersQuery.Where(u => dto.TargetRoleIds.Contains(u.RoleID));
                }

                var targetUsers = await usersQuery.Select(u => new { u.UserID, u.Email, u.FullName }).ToListAsync();

                if (!targetUsers.Any())
                {
                    return BadRequest(ApiResponse<bool>.Fail("No users matched the selected roles."));
                }

                // Create in-app notifications
                var notifications = new List<Notification>();
                foreach (var u in targetUsers)
                {
                    notifications.Add(new Notification
                    {
                        UserId = u.UserID,
                        Title = dto.Subject,
                        Message = dto.Message,
                        Type = "system",
                        Icon = "fas fa-bullhorn",
                        ColorClass = "bg-purple-100 text-purple-600",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }

                await _context.Notifications.AddRangeAsync(notifications);
                await _context.SaveChangesAsync();

                // Start background task to send emails
                _ = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<Saowari.Services.IEmailService>();

                    var imgTag = !string.IsNullOrEmpty(imageUrl) ? $"<div style='text-align: center; margin-bottom: 20px;'><img src='{imageUrl}' alt='Promotion' style='max-width: 100%; border-radius: 8px;' /></div>" : "";
                    
                    foreach (var u in targetUsers)
                    {
                        if (string.IsNullOrEmpty(u.Email)) continue;

                        var htmlBody = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e5e7eb; border-radius: 8px;'>
                                {imgTag}
                                <h2 style='color: #111827; margin-bottom: 16px;'>{dto.Subject}</h2>
                                <p style='color: #374151; line-height: 1.6;'>Hello {u.FullName},</p>
                                <div style='color: #4b5563; line-height: 1.6; margin-top: 16px;'>
                                    {dto.Message.Replace("\n", "<br/>")}
                                </div>
                                <hr style='border: 0; border-top: 1px solid #e5e7eb; margin: 30px 0;' />
                                <p style='color: #9ca3af; font-size: 12px; text-align: center;'>You received this notice because you are an important member of the Saowari platform.</p>
                            </div>";

                        try
                        {
                            await emailService.SendEmailAsync(u.Email, dto.Subject, htmlBody, dto.Message);
                        }
                        catch { /* Ignore failures to keep the loop going */ }
                    }
                });

                return Ok(ApiResponse<bool>.Ok(true, $"Broadcast queued successfully for {targetUsers.Count} user(s)."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
