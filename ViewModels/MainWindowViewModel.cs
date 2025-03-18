using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Vet_System.Components;
using Vet_System.Lib;
using Vet_System.Models;

namespace Vet_System.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private string currentPageTitle = string.Empty;

        [ObservableProperty]
        private UserInfo currentUser = null!;

        [ObservableProperty]
        private string currentDateTime = string.Empty;

        [ObservableProperty]
        private NavItem selectedNavItem = null!;

        // Initialize the collection in the property declaration
        public ObservableCollection<NavItem> NavItems { get; } = new();

        private readonly PeriodicTimer _timer;
        private bool _isDisposed;
        public event EventHandler<CustomNavigationEventArgs> NavigationRequested;

        public MainPageViewModel()
        {
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            InitializeNavItems();
            InitializeUserInfo();
            StartTimeUpdate();

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedNavItem) && SelectedNavItem != null)
                {
                    CurrentPageTitle = SelectedNavItem.Label;
                    NavigateToPage(SelectedNavItem);
                }
            };
        }

        private void InitializeNavItems()
        {
            // Clear existing items if any
            NavItems.Clear();

            // Add items using the constructor
            NavItems.Add(new NavItem(typeof(Pages.DashboardPage))
            {
                Icon = "\uE80F",
                Label = "Dashboard"
            });

            NavItems.Add(new NavItem(typeof(Pages.PatientsPage))
            {
                Icon = "\uE77B",
                Label = "Patients"
            });

            NavItems.Add(new NavItem(typeof(Pages.AppointmentsPage))
            {
                Icon = "\uE77C",
                Label = "Appointments"
            });

            NavItems.Add(new NavItem(typeof(Pages.StaffPage))
            {
                Icon = "\uE716",
                Label = "Staff"
            });

            // Set initial selection
            SelectedNavItem = NavItems[0];
            CurrentPageTitle = SelectedNavItem.Label;

        }

        private void InitializeUserInfo()
        {
            CurrentUser = new UserInfo("Russell901", "Russell");
            UpdateDateTime();
        }

        private async void StartTimeUpdate()
        {
            try
            {
                while (!_isDisposed && await _timer.WaitForNextTickAsync())
                {
                    UpdateDateTime();
                }
            }
            catch (OperationCanceledException)
            {
                // Timer was disposed
            }
        }

        private void UpdateDateTime()
        {
            try
            {
                CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            }
            catch (Exception)
            {
                // Log or handle the exception
            }
        }


        private void NavigateToPage(NavItem navItem)
        {
            if (navItem?.PageType != null)
            {
                NavigationRequested?.Invoke(this,
                    new CustomNavigationEventArgs
                    {
                        PageType = navItem.PageType,
                        Parameter = null
                    });
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Add logout logic here
            // For example:
            // await AuthService.LogoutAsync();
            // Navigate to login page
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _timer?.Dispose();
            }
        }
    }
}