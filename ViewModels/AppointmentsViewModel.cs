using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Vet_System.Components.Dialogs;
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

        public AppointmentsViewModel(IAppointmentService? appointmentService = null)
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private void OnSearchDebounce(object? sender, object? e)
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
                (FilterStatus == "all" || a.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var appointment in filtered)
            {
                FilteredAppointments.Add(appointment);
            }
        }

        private async Task CreateNewAppointment()
        {
            try
            {
                var xamlRoot = App.MainWindow.Content is FrameworkElement mainElement ? mainElement.XamlRoot : null;
                var viewModel = new AppointmentFormViewModel(
                    null,
                    _appointmentService,
                    new PetService(DatabaseService.DefaultConnectionString),
                    xamlRoot 
                );

                var dialog = new ContentDialog
                {
                    XamlRoot = xamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Content = new AppointmentFormDialog(viewModel),
                    Title = "Schedule Appointment",
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    IsPrimaryButtonEnabled = true
                };

                dialog.PrimaryButtonClick += async (s, e) =>
                {
                    e.Cancel = true;
                    try
                    {
                        await viewModel.Save();

                        if (!viewModel.HasValidationErrors)
                        {
                            dialog.Hide();

                            var createdAppointment = await viewModel.GetResultAsync();
                            if (createdAppointment != null)
                            {
                                await LoadAppointmentsAsync();

                                var successDialog = new ContentDialog
                                {
                                    XamlRoot = App.MainWindow.Content is FrameworkElement successElement ? successElement.XamlRoot : null,
                                    Title = "Appointment Scheduled",
                                    Content = "The appointment has been successfully scheduled.",
                                    CloseButtonText = "OK"
                                };
                                await successDialog.ShowAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        dialog.Hide();
                        var errorDialog = new ContentDialog
                        {
                            XamlRoot = App.MainWindow.Content is FrameworkElement errorElement ? errorElement.XamlRoot : null,
                            Title = "Error Saving Appointment",
                            Content = $"Could not save appointment: {ex.Message}",
                            CloseButtonText = "OK"
                        };
                        await errorDialog.ShowAsync();
                    }
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    XamlRoot = App.MainWindow.Content is FrameworkElement errorElement ? errorElement.XamlRoot : null,
                    Title = "Error",
                    Content = $"Error creating appointment: {ex.Message}",
                    CloseButtonText = "OK"
                };
                await errorDialog.ShowAsync();
            }
        }

        private void ViewCalendar()
        {
            Debug.WriteLine("Viewing calendar");
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