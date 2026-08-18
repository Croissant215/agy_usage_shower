using System;

namespace AgyUsageShower.Models
{
    public class UsageData
    {
        public string AccountEmail { get; set; } = "Connected Account";
        public double GeminiWeeklyPercent { get; set; } = 0.0;
        public double Gemini5hPercent { get; set; } = 0.0;
        public string GeminiResetTime { get; set; } = "-";

        public double ClaudeWeeklyPercent { get; set; } = 0.0;
        public double Claude5hPercent { get; set; } = 0.0;
        public string ClaudeResetTime { get; set; } = "-";

        public bool IsRealData { get; set; } = false;
        public bool IsOffline { get; set; } = false;
        public bool IsLoggedIn { get; set; } = false;
        public bool IsRefreshingToken { get; set; } = false;

        public string StatusBadgeText
        {
            get
            {
                if (IsRefreshingToken) return "⏳ 토큰 자동 갱신 중...";
                if (!IsLoggedIn) return "🔴 계정 연결 필요 (로그인 클릭)";
                if (!IsRealData) return "🟡 네트워크 또는 파싱 오류";
                return $"🟢 Connected: {AccountEmail}";
            }
        }
    }
}
