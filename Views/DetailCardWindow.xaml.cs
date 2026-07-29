using System;
using System.Windows;
using AgyUsageShower.Services;
using AgyUsageShower.ViewModels;

namespace AgyUsageShower.Views
{
    public partial class DetailCardWindow : Window
    {
        public DetailCardWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (s, e) => Win32TaskbarService.HideFromAltTab(this);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ToggleTheme();
            }
        }

        private async void SwitchAccountButton_Click(object sender, RoutedEventArgs e)
        {
            AntigravityUsageService.TriggerGoogleLogin();
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshUsageAsync();
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AntigravityUsageService.TriggerLogout();
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshUsageAsync();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshUsageAsync();
            }
        }
    }
}
