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

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _usageService = new AntigravityUsageService();
            _currentUsage = new UsageData();

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
                OnPropertyChanged(nameof(Color5hHex));
                OnPropertyChanged(nameof(ColorWeeklyHex));
            }
        }

        public string Quota5hFormatted => $"{CurrentUsage.Quota5hPercent}%";
        public string WeeklyFormatted => $"{CurrentUsage.WeeklyQuotaPercent}%";

        public string Color5hHex => GetColorForPercent(CurrentUsage.Quota5hPercent);
        public string ColorWeeklyHex => GetColorForPercent(CurrentUsage.WeeklyQuotaPercent);

        private static string GetColorForPercent(double percent)
        {
            if (percent >= 70) return "#40C057"; // Green
            if (percent >= 30) return "#FAB005"; // Yellow
            return "#FA5252"; // Red
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
