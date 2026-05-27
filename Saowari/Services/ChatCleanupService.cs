using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saowari.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class ChatCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ChatCleanupService> _logger;

        public ChatCleanupService(
            IServiceProvider serviceProvider,
            IWebHostEnvironment env,
            ILogger<ChatCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Automatic Chat Cleanup Service has started working.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanupAsync();
                    // Run every 1 hour
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during background chat cleanup execution.");
                    // Optional: delay before retry to prevent tight error loop
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task PerformCleanupAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SaowariDbContext>();
                var now = DateTime.UtcNow;

                // ── 1. PASSENGER GROUP CHATS (24-Hour Rule after Schedule Expired) ────────

                // Find all schedules that ended more than 24 hours ago
                var expiredSchedules = await context.Schedules
                    .Where(s => s.ArrivalDateTime <= now.AddHours(-24))
                    .Select(s => s.ScheduleID)
                    .ToListAsync();

                if (expiredSchedules.Any())
                {
                    _logger.LogInformation("Pruning passenger chats for {Count} expired schedules.", expiredSchedules.Count);

                    var expiredMessages = await context.ScheduleChatMessages
                        .Where(m => expiredSchedules.Contains(m.ScheduleId))
                        .ToListAsync();

                    // Delete physical files
                    foreach (var msg in expiredMessages.Where(m => !string.IsNullOrEmpty(m.FileUrl)))
                    {
                        DeletePhysicalFile(msg.FileUrl!);
                    }

                    // Remove from database
                    if (expiredMessages.Any())
                    {
                        context.ScheduleChatMessages.RemoveRange(expiredMessages);
                    }
                }

                // ── 2. SUPPORT MESSAGES (3-Month / 90-Day Rule) ──────────────────────────

                var cutoffDate = now.AddDays(-90);
                _logger.LogInformation("Pruning support messages older than: {Cutoff}", cutoffDate);

                var oldSupportMessages = await context.SupportMessages
                    .Where(m => m.CreatedAt <= cutoffDate)
                    .ToListAsync();

                // Delete physical files
                foreach (var msg in oldSupportMessages.Where(m => !string.IsNullOrEmpty(m.FileUrl)))
                {
                    DeletePhysicalFile(msg.FileUrl!);
                }

                // Remove from database
                if (oldSupportMessages.Any())
                {
                    context.SupportMessages.RemoveRange(oldSupportMessages);
                }

                await context.SaveChangesAsync();
            }
        }

        private void DeletePhysicalFile(string relativeUrl)
        {
            try
            {
                // Convert relative URL e.g. "/uploads/chat/xxx.png" to physical path
                var relativePath = relativeUrl.TrimStart('/');
                var physicalPath = Path.Combine(_env.WebRootPath, relativePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("Deleted expired chat attachment: {Path}", physicalPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete physical file: {Url}", relativeUrl);
            }
        }
    }
}
