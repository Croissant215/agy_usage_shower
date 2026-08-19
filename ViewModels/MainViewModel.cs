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
        private readonly string _settingsFilePath;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isRefreshing = false;

        public MainViewModel()
        {
            _usageService = new AntigravityUsageService();
            _currentUsage = new UsageData();

            var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AgyUsageShower");
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            _settingsFilePath = System.IO.Path.Combine(folder, "settings.json");
            LoadSettings();

            // Background polling every 60 seconds to save CPU/RAM when using CLI
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            _timer.Tick += async (s, e) => 
            {
                if (_isRefreshing) return;
                _isRefreshing = true;
                try
                {
                    await RefreshUsageAsync(forceRefresh: false);
                }
                finally
                {
                    _isRefreshing = false;
                }
            };
            _timer.Start();

            _ = RefreshUsageAsync(forceRefresh: true);
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
            SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (System.IO.File.Exists(_settingsFilePath))
                {
                    string json = System.IO.File.ReadAllText(_settingsFilePath);
                    if (json.Contains("\"IsDarkTheme\":false"))
                    {
                        IsDarkTheme = false; // Using property to trigger notify
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                string json = $"{{\"IsDarkTheme\":{_isDarkTheme.ToString().ToLower()}}}";
                System.IO.File.WriteAllText(_settingsFilePath, json);
            }
            catch { }
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

        public async Task RefreshUsageAsync(bool forceRefresh = false)
        {
            CurrentUsage = await _usageService.FetchUsageAsync(forceRefresh);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
