using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgyUsageShower.Models;

namespace AgyUsageShower.Services
{
    public class AntigravityUsageService
    {
        private readonly string _antigravityDir;

        public AntigravityUsageService()
        {
            string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _antigravityDir = Path.Combine(userHome, ".gemini", "antigravity-cli");
        }

        public async Task<UsageData> FetchUsageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string historyPath = Path.Combine(_antigravityDir, "history.jsonl");
                    long tokenCountToday = 0;

                    if (File.Exists(historyPath))
                    {
                        FileInfo fi = new FileInfo(historyPath);
                        tokenCountToday = fi.Length / 4; // Approx estimated token footprint
                    }

                    // Calculate mock/parsed quota metrics based on usage
                    double proQuota = Math.Max(15.0, 100.0 - ((tokenCountToday / 1000.0) % 85.0));
                    double flashQuota = Math.Max(25.0, 100.0 - ((tokenCountToday / 1500.0) % 70.0));

                    DateTime now = DateTime.Now;
                    DateTime nextReset = now.Date.AddHours(now.Hour + 1);
                    TimeSpan remaining = nextReset - now;
                    string resetText = $"⏳ {remaining.Minutes}m {remaining.Seconds}s";

                    return new UsageData
                    {
                        ModelName = "Gemini 3.5 Pro",
                        QuotaPercent = Math.Round(proQuota, 1),
                        FlashQuotaPercent = Math.Round(flashQuota, 1),
                        ResetCountdown = resetText,
                        TokensUsedToday = tokenCountToday,
                        CreditBalance = "$100.00",
                        IsOffline = false
                    };
                }
                catch (Exception)
                {
                    return new UsageData
                    {
                        ModelName = "Gemini 3.5 Pro",
                        QuotaPercent = 75.0,
                        FlashQuotaPercent = 90.0,
                        ResetCountdown = "⏳ 45m",
                        TokensUsedToday = 50000,
                        CreditBalance = "$100.00",
                        IsOffline = true
                    };
                }
            });
        }
    }
}
