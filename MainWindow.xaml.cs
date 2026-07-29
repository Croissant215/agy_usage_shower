using System;
using System.Windows;
using System.Windows.Input;
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
        private readonly DispatcherTimer _repositionTimer;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _repositionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _repositionTimer.Tick += (s, e) => EmbedInsideTaskbar();
            _repositionTimer.Start();

            Closing += (s, e) => { e.Cancel = true; };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmbedInsideTaskbar();
        }

        private void EmbedInsideTaskbar()
        {
            Win32TaskbarService.EmbedInsideTaskbar(this, Width, Height);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

                _detailWindow.Left = screenWidth - _detailWindow.Width - 20;
                _detailWindow.Top = screenHeight - _detailWindow.Height - 45;
                _detailWindow.Show();
                _detailWindow.Activate();
            }
        }
    }
}
