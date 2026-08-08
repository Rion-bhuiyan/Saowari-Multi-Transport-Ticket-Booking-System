using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Responses;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOrManager")]
    public class TrafficAnalyticsController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public TrafficAnalyticsController(SaowariDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics()
        {
            // ── 1. OVERVIEW ────────────────────────────────────────────────
            var totalVisits = await _context.UserLoginHistories.CountAsync();
            if (totalVisits == 0) totalVisits = 1; // prevent div-by-zero

            var totalBookings = await _context.Bookings.CountAsync();
            var conversionRate = Math.Round(((double)totalBookings / totalVisits) * 100, 2);

            // Bounce Rate: sessions with no LastActiveTime OR less than 10 sec on site
            var bounceCount = await _context.UserLoginHistories
                .CountAsync(h => h.LastActiveTime == null
                    || EF.Functions.DateDiffSecond(h.LoginTime, h.LastActiveTime) < 10);
            var bounceRate = Math.Round(((double)bounceCount / totalVisits) * 100, 2);

            // Average session duration (in-memory for TimeSpan math)
            var sessionsWithDuration = await _context.UserLoginHistories
                .Where(h => h.LastActiveTime.HasValue)
                .Select(h => new { h.LoginTime, h.LastActiveTime })
                .ToListAsync();

            double avgSeconds = 0;
            if (sessionsWithDuration.Any())
                avgSeconds = sessionsWithDuration.Average(h => (h.LastActiveTime!.Value - h.LoginTime).TotalSeconds);

            var ts = TimeSpan.FromSeconds(avgSeconds);
            var visitDuration = $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";

            // ── 2. DEVICE DISTRIBUTION (real from DeviceName / Browser) ────
            var allLogins = await _context.UserLoginHistories
                .Select(h => new { h.DeviceName, h.Browser })
                .ToListAsync();

            var mobileCount = allLogins.Count(h =>
                (h.DeviceName != null && (h.DeviceName.Contains("Mobi") || h.DeviceName.Contains("Android") || h.DeviceName.Contains("iPhone")))
                || (h.Browser != null && (h.Browser == "Mobile Safari" || h.Browser.Contains("Mobile"))));

            var desktopCount = totalVisits - mobileCount;
            var mobileShare = Math.Round(((double)mobileCount / totalVisits) * 100, 2);
            var desktopShare = Math.Round(((double)desktopCount / totalVisits) * 100, 2);

            // ── 3. TOP COUNTRIES (real) ────────────────────────────────────
            var countryData = await _context.UserLoginHistories
                .Where(h => h.Country != null && h.CountryCode != null)
                .GroupBy(h => new { h.Country, h.CountryCode })
                .Select(g => new { g.Key.Country, g.Key.CountryCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topCountries = countryData.Select(c => new
            {
                country = c.Country!,
                flag = c.CountryCode!.ToLower(),
                trafficShare = Math.Round(((double)c.Count / totalVisits) * 100, 2),
                // Calculate monthly trend vs previous month
                change = 0.0 // will calculate below
            }).ToList<object>();

            // Calculate change vs previous 30 days
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var sixtyDaysAgo = now.AddDays(-60);

            var recentCountryData = await _context.UserLoginHistories
                .Where(h => h.Country != null && h.LoginTime >= thirtyDaysAgo)
                .GroupBy(h => h.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToListAsync();

            var prevCountryData = await _context.UserLoginHistories
                .Where(h => h.Country != null && h.LoginTime >= sixtyDaysAgo && h.LoginTime < thirtyDaysAgo)
                .GroupBy(h => h.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentTotal = recentCountryData.Sum(x => x.Count);
            var prevTotal = prevCountryData.Sum(x => x.Count);

            topCountries = countryData.Select(c =>
            {
                var recentCount = recentCountryData.FirstOrDefault(r => r.Country == c.Country)?.Count ?? 0;
                var prevCount = prevCountryData.FirstOrDefault(r => r.Country == c.Country)?.Count ?? 0;
                double recentShare = recentTotal > 0 ? (double)recentCount / recentTotal * 100 : 0;
                double prevShare = prevTotal > 0 ? (double)prevCount / prevTotal * 100 : 0;
                double change = Math.Round(recentShare - prevShare, 2);
                return (object)new
                {
                    country = c.Country!,
                    flag = c.CountryCode!.ToLower(),
                    trafficShare = Math.Round(((double)c.Count / totalVisits) * 100, 2),
                    change = change
                };
            }).ToList();

            // ── 4. MARKETING CHANNELS (real from TrafficChannel column) ───
            var channelData = await _context.UserLoginHistories
                .GroupBy(h => h.TrafficChannel ?? "Direct")
                .Select(g => new { Channel = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var marketingChannels = channelData.Select(c => new
            {
                channel = c.Channel,
                percentage = Math.Round(((double)c.Count / totalVisits) * 100, 2)
            }).ToList<object>();

            if (!marketingChannels.Any())
            {
                marketingChannels = new List<object>
                {
                    new { channel = "Direct", percentage = 100.0 }
                };
            }

            // ── 5. SOCIAL TRAFFIC (real, extracted from Social channel visits) ─
            var socialNetworkMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "youtube", "YouTube" },
                { "facebook", "Facebook" },
                { "instagram", "Instagram" },
                { "twitter", "X (Twitter)" },
                { "t.co", "X (Twitter)" },
                { "linkedin", "LinkedIn" },
                { "pinterest", "Pinterest" },
                { "telegram", "Telegram" },
                { "tiktok", "TikTok" }
            };

            var socialReferrers = await _context.UserLoginHistories
                .Where(h => h.TrafficChannel == "Social" && h.Referrer != null)
                .Select(h => h.Referrer!)
                .ToListAsync();

            var socialCounts = new Dictionary<string, int>();
            foreach (var referrer in socialReferrers)
            {
                var lower = referrer.ToLower();
                foreach (var kv in socialNetworkMap)
                {
                    if (lower.Contains(kv.Key))
                    {
                        if (!socialCounts.ContainsKey(kv.Value)) socialCounts[kv.Value] = 0;
                        socialCounts[kv.Value]++;
                        break;
                    }
                }
            }

            var totalSocial = socialCounts.Values.Sum();
            List<object> socialTraffic;

            if (totalSocial > 0)
            {
                socialTraffic = socialCounts
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => (object)new
                    {
                        network = kv.Key,
                        percentage = Math.Round(((double)kv.Value / totalSocial) * 100, 2)
                    }).ToList();
            }
            else
            {
                // No social data yet — show empty state placeholder
                socialTraffic = new List<object>
                {
                    new { network = "No social traffic yet", percentage = 100.0 }
                };
            }

            // ── 6. BROWSER DISTRIBUTION ───────────────────────────────────
            var browserData = await _context.UserLoginHistories
                .Where(h => h.Browser != null)
                .GroupBy(h => h.Browser)
                .Select(g => new { Browser = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var browserStats = browserData.Select(b => new
            {
                browser = b.Browser!,
                percentage = Math.Round(((double)b.Count / totalVisits) * 100, 2)
            }).ToList<object>();

            // ── 7. ACTIVE USERS (logged in within last 30 minutes) ────────
            var activeThreshold = DateTime.UtcNow.AddMinutes(-30);
            var activeUsers = await _context.UserLoginHistories
                .CountAsync(h => h.IsActive && h.LastActiveTime.HasValue && h.LastActiveTime > activeThreshold);

            // ── 8. TOP CITIES ─────────────────────────────────────────────
            var topCities = await _context.UserLoginHistories
                .Where(h => h.City != null && h.City != "Unknown" && h.City != "Localhost")
                .GroupBy(h => new { h.City, h.Country })
                .Select(g => new { g.Key.City, g.Key.Country, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // ── COMPOSE RESPONSE ──────────────────────────────────────────
            return Ok(ApiResponse<object>.Ok(new
            {
                overview = new
                {
                    totalVisits,
                    conversionRate,
                    bounceRate,
                    visitDuration,
                    activeUsers
                },
                deviceDistribution = new
                {
                    desktop = desktopShare,
                    mobile = mobileShare
                },
                marketingChannels,
                socialTraffic,
                topCountries,
                browserStats,
                topCities = topCities.Select(c => new { city = c.City, country = c.Country, count = c.Count })
            }));
        }
    }
}
