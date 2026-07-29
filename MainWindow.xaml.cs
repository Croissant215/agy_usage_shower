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
                Interval = TimeSpan.FromSeconds(3)
            };
            _repositionTimer.Tick += (s, e) => RepositionOnTaskbar();
            _repositionTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Win32TaskbarService.SetOverlayWindowStyles(this);
            RepositionOnTaskbar();
        }

        private void RepositionOnTaskbar()
        {
            var (left, top) = Win32TaskbarService.CalculateRightDockPosition(Width, Height, this);
            Left = left;
            Top = top;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_detailWindow == null)
            {
                _detailWindow = new DetailCardWindow(_viewModel);
            }

            _detailWindow.Left = Left - 180;
            _detailWindow.Top = Top - _detailWindow.Height - 8;
            _detailWindow.Show();
            _detailWindow.Activate();
        }
    }
}
