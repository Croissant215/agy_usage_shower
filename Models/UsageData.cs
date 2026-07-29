namespace AgyUsageShower.Models
{
    public class UsageData
    {
        public double Quota5hPercent { get; set; } = 85.0; // 5-hour refresh quota %
        public string Reset5hCountdown { get; set; } = "⏳ 2h 15m";
        public double WeeklyQuotaPercent { get; set; } = 64.0; // Weekly baseline quota %
        public string WeeklyResetCountdown { get; set; } = "⏳ 4d 12h";
        public bool IsOffline { get; set; } = false;
        public long TokensUsedToday { get; set; } = 124500;
        public string TierName { get; set; } = "Google AI Pro";
    }
}
