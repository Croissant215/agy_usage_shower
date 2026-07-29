namespace AgyUsageShower.Models
{
    public class UsageData
    {
        public string ModelName { get; set; } = "Gemini 3.5 Pro";
        public double QuotaPercent { get; set; } = 85.0;
        public string ResetCountdown { get; set; } = "⏳ 2h 15m";
        public bool IsOffline { get; set; } = false;
        public long TokensUsedToday { get; set; } = 124500;
        public double FlashQuotaPercent { get; set; } = 92.0;
        public string CreditBalance { get; set; } = "$100.00";
    }
}
