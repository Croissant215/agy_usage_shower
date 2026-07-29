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

            Closing += (s, e) => { e.Cancel = true; }; // Prevent closing to stay persistent like RunCat
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

            if (_detailWindow.IsVisible)
            {
                _detailWindow.Hide();
            }
            else
            {
                _detailWindow.Left = Left - 140;
                _detailWindow.Top = Top - _detailWindow.Height - 6;
                _detailWindow.Show();
                _detailWindow.Activate();
            }
        }
    }
}
