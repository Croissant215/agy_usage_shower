namespace AgyUsageShower.Models
{
    public class UsageData
    {
        public string AccountEmail { get; set; } = "cloudcandy2772@gmail.com";

        // Gemini Group
        public double GeminiWeeklyPercent { get; set; } = 35.09;
        public double Gemini5hPercent { get; set; } = 29.17;
        public string GeminiResetTime { get; set; } = "7m";

        // Claude / GPT Group
        public double ClaudeWeeklyPercent { get; set; } = 100.00;
        public double Claude5hPercent { get; set; } = 100.00;
        public string ClaudeResetTime { get; set; } = "Quota available";

        // Aggregate for display
        public double Quota5hPercent => Gemini5hPercent;
        public double WeeklyQuotaPercent => GeminiWeeklyPercent;

        public bool IsOffline { get; set; } = false;
        public bool IsRealData { get; set; } = true;
    }
}
