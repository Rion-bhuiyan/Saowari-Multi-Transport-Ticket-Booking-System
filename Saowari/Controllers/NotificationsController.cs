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

        public NotificationsController(SaowariDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
    }
}
