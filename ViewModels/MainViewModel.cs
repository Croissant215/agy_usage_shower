using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using AgyUsageShower.Models;
using AgyUsageShower.Services;

namespace AgyUsageShower.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AntigravityUsageService _usageService;
        private readonly DispatcherTimer _timer;
        private UsageData _currentUsage;
        private bool _isDarkTheme = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _usageService = new AntigravityUsageService();
            _currentUsage = new UsageData();

            // Real-time FileSystemWatcher trigger (instant update on prompt/log change)
            _usageService.OnRealtimeUsageChanged += () =>
            {
                System.Windows.Application.Current?.Dispatcher.InvokeAsync(async () => await RefreshUsageAsync());
            };

            // Periodic 2-second polling timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += async (s, e) => await RefreshUsageAsync();
            _timer.Start();

            _ = RefreshUsageAsync();
        }

        public UsageData CurrentUsage
        {
            get => _currentUsage;
            set
            {
                _currentUsage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Quota5hFormatted));
                OnPropertyChanged(nameof(WeeklyFormatted));
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                _isDarkTheme = value;
                OnPropertyChanged();
                NotifyThemePropertiesChanged();
            }
        }

        public string Quota5hFormatted => $"{CurrentUsage.Gemini5hPercent:F2}%";
        public string WeeklyFormatted => $"{CurrentUsage.GeminiWeeklyPercent:F2}%";

        // Semi-transparent ARGB Hex Colors (80% / 90% opacity for glass effect)
        public string WidgetBgHex => IsDarkTheme ? "#C812111A" : "#D8FFFFFF";
        public string CardBgHex => IsDarkTheme ? "#E612111A" : "#F2FFFFFF";
        public string WidgetBorderHex => IsDarkTheme ? "#663B3754" : "#66EAE6FA";
        public string MainTextHex => IsDarkTheme ? "#FFFFFF" : "#1A1828";
        public string SubTextHex => IsDarkTheme ? "#A09CBA" : "#6E6A8A";
        public string DividerHex => IsDarkTheme ? "#443B3754" : "#44EAE6FA";

        public string BrandPinkHex => "#FF2E93";
        public string Progress5hHex => "#FF2E93";  // Vibrant Pink
        public string ProgressWkHex => "#8C52FF";  // Electric Violet
        public string ProgressBgHex => IsDarkTheme ? "#55231F35" : "#55F0EDF9";
        public string TimerTextHex => IsDarkTheme ? "#FFAE00" : "#D97706";
        public string ThemeIcon => IsDarkTheme ? "🌙" : "☀️";

        public void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        private void NotifyThemePropertiesChanged()
        {
            OnPropertyChanged(nameof(WidgetBgHex));
            OnPropertyChanged(nameof(CardBgHex));
            OnPropertyChanged(nameof(WidgetBorderHex));
            OnPropertyChanged(nameof(MainTextHex));
            OnPropertyChanged(nameof(SubTextHex));
            OnPropertyChanged(nameof(DividerHex));
            OnPropertyChanged(nameof(ProgressBgHex));
            OnPropertyChanged(nameof(TimerTextHex));
            OnPropertyChanged(nameof(ThemeIcon));
        }

        public async Task RefreshUsageAsync()
        {
            CurrentUsage = await _usageService.FetchUsageAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
