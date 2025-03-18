using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Vet_System.Models;
using Vet_System.Services;

namespace Vet_System.ViewModels
{
    public class AppointmentsViewModel : ObservableObject
    {
        private readonly IAppointmentService _appointmentService;
        private string _searchTerm = string.Empty;
        private string _filterStatus = "all";
        private DispatcherTimer _searchDebounceTimer;
        private bool _isLoading = false;
        private bool _isRefreshing = false;

        public ObservableCollection<AppointmentItemViewModel> Appointments { get; } = new();
        public ObservableCollection<AppointmentItemViewModel> FilteredAppointments { get; } = new();

        public ObservableCollection<StatusFilter> StatusFilters { get; } = new()
        {
            new("All Status", "all"),
            new("Scheduled", "scheduled"),
            new("Completed", "completed"),
            new("Cancelled", "cancelled")
        };

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set => SetProperty(ref _isRefreshing, value);
        }

        public ICommand NewAppointmentCommand { get; }
        public ICommand ViewCalendarCommand { get; }
        public ICommand RefreshCommand { get; }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetProperty(ref _searchTerm, value);
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            }
        }

        public string FilterStatus
        {
            get => _filterStatus;
            set
            {
                SetProperty(ref _filterStatus, value);
                FilterAppointments();
            }
        }

        // Constructor for the main AppointmentsViewModel
        public AppointmentsViewModel(IAppointmentService appointmentService = null)
        {
            _appointmentService = appointmentService ?? new AppointmentService(DatabaseService.DefaultConnectionString);
            _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += OnSearchDebounce;

            NewAppointmentCommand = new AsyncRelayCommand(CreateNewAppointment);
            ViewCalendarCommand = new RelayCommand(ViewCalendar);
            RefreshCommand = new AsyncRelayCommand(LoadAppointmentsAsync);
        }

        public async Task LoadAppointmentsAsync()
        {
            if (IsLoading)
                return;

            IsLoading = true;
            IsRefreshing = true;

            try
            {
                var appointments = await _appointmentService.GetAppointmentsAsync();
                Appointments.Clear();

                foreach (var appointment in appointments)
                {
                    Appointments.Add(new AppointmentItemViewModel(
                        appointment,
                        _appointmentService,
                        async () => await LoadAppointmentsAsync()
                    ));
                }

                FilterAppointments();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private void OnSearchDebounce(object sender, object e)
        {
            _searchDebounceTimer.Stop();
            FilterAppointments();
        }

        public void FilterAppointments()
        {
            FilteredAppointments.Clear();

            var filtered = Appointments.Where(a =>
                (string.IsNullOrEmpty(SearchTerm) ||
                a.PetName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.OwnerName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
                (FilterStatus == "all" || a.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase)));

            foreach (var appointment in filtered)
            {
                FilteredAppointments.Add(appointment);
            }
        }

        private async Task CreateNewAppointment()
        {
            try
            {
                // ToDo: Create a ComboBox that collects and displays Pet's names
                var newAppointment = new AppointmentItem(
                    null,
                    "New Pet", 
                    "New Owner",
                    DateTime.Now.AddDays(3).Date.AddHours(9),
                    "Wellness check",
                    "scheduled"
                );

                await _appointmentService.CreateAppointmentAsync(newAppointment);

                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating appointment: {ex.Message}");
            }
        }

        private void ViewCalendar()
        {
            //ToDo create calendar view
            System.Diagnostics.Debug.WriteLine("Viewing calendar");
        }
    }

    public class StatusFilter
    {
        public string DisplayName { get; }
        public string Value { get; }

        public StatusFilter(string displayName, string value)
        {
            DisplayName = displayName;
            Value = value;
        }
    }
}