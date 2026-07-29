using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using AgyUsageShower.Services;
using AgyUsageShower.ViewModels;
using AgyUsageShower.Views;

namespace AgyUsageShower
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private DetailCardWindow? _detailWindow;
        private readonly NotifyIcon _notifyIcon;
        private readonly DispatcherTimer _animTimer;
        private int _frameIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "AGY RunCat Quota Monitor (Gemini 5h: 29.17% | Weekly: 35.09%)";

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("🐱 Open Quota Details", null, (s, e) => ToggleDetailCard());
            contextMenu.Items.Add("🔄 Sync Quotas Now", null, async (s, e) => await _viewModel.RefreshUsageAsync());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("🚪 Exit AGY Usage Shower", null, (s, e) => ExitApplication());
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ToggleDetailCard();
                }
            };

            _animTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();

            Closing += (s, e) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCatAnimation();
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            _frameIndex = (_frameIndex + 1) % 5;
            UpdateCatAnimation();
        }

        private void UpdateCatAnimation()
        {
            double remaining5h = _viewModel.CurrentUsage.Gemini5hPercent;
            double doubleWeekly = _viewModel.CurrentUsage.GeminiWeeklyPercent;

            double consumption = Math.Max(0.0, Math.Min(100.0, 100.0 - remaining5h));
            int intervalMs = (int)Math.Max(35.0, 250.0 - (consumption * 2.1));
            _animTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);

            _notifyIcon.Text = $"AGY RunCat: Gemini 5h {remaining5h:F1}% | Wk {doubleWeekly:F1}% ({_viewModel.CurrentUsage.GeminiResetTime})";

            Icon catIcon = CatIconGenerator.GetCatFrameIcon(_frameIndex, remaining5h);
            _notifyIcon.Icon = catIcon;
        }

        private void ToggleDetailCard()
        {
            if (_detailWindow == null)
            {
                _detailWindow = new DetailCardWindow(_viewModel);
            }

            if (_detailWindow.IsVisible)
            {
                _detailWindow.Hide();
            }
            else
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                _detailWindow.Left = screenWidth - _detailWindow.Width - 15;
                _detailWindow.Top = screenHeight - _detailWindow.Height - 45;
                _detailWindow.Show();
                _detailWindow.Activate();
            }
        }

        private void ExitApplication()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
