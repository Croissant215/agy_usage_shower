using System;

namespace AgyUsageShower.Models
{
    public class UsageData
    {
        public string AccountEmail { get; set; } = "cloudcandy2772@gmail.com";
        public double GeminiWeeklyPercent { get; set; } = 97.36;
        public double Gemini5hPercent { get; set; } = 72.52;
        public string GeminiResetTime { get; set; } = "4h 32m";

        public double ClaudeWeeklyPercent { get; set; } = 100.00;
        public double Claude5hPercent { get; set; } = 100.00;
        public string ClaudeResetTime { get; set; } = "Quota available";

        public bool IsRealData { get; set; } = true;
        public bool IsOffline { get; set; } = false;
        public bool IsLoggedIn { get; set; } = true;

        public string StatusBadgeText => IsLoggedIn ? $"🟢 Connected: {AccountEmail}" : "🔴 Logged Out";
    }
}
