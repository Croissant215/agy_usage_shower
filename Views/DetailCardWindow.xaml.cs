using System;
using System.Windows;
using AgyUsageShower.ViewModels;

namespace AgyUsageShower.Views
{
    public partial class DetailCardWindow : Window
    {
        public DetailCardWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshUsageAsync();
            }
        }
    }
}
