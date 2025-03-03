using Microsoft.UI.Xaml;
using System;
using Vet_System.Lib;
using Vet_System.ViewModels;

namespace Vet_System
{
    public sealed partial class MainWindow : Window, IDisposable
    {
        public MainPageViewModel ViewModel { get; }
        private bool _isDisposed;

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainPageViewModel();
            ViewModel.NavigationRequested += ViewModel_NavigationRequested;

            // Set initial page
            ContentFrame.Navigate(typeof(Pages.DashboardPage));
        }

        private void ViewModel_NavigationRequested(object sender, CustomNavigationEventArgs e)
        {
            ContentFrame.Navigate(e.PageType, e.Parameter);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                ViewModel?.Dispose();
            }
        }
    }
}