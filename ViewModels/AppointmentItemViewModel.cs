using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Vet_System.Models;
using Vet_System.Services;

namespace Vet_System.ViewModels
{
    public class AppointmentItemViewModel : ObservableObject
    {
        private readonly AppointmentItem _appointment;
        private readonly IAppointmentService _appointmentService;
        private readonly Func<Task> _refreshCallback;
        private readonly Action _onStatusChanged;

        public string Id => _appointment.Id;
        public string PetName => _appointment.PetName;
        public string OwnerName => _appointment.OwnerName;
        public string Date => _appointment.DateTime.Date.ToString("MM/dd/yyyy");
        public string Time => _appointment.DateTime.ToString("hh:mm tt");
        public string Reason => _appointment.Reason;
        public string Status => _appointment.Status;

        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CompleteCommand { get; }

        public AppointmentItemViewModel(
            AppointmentItem appointment,
            IAppointmentService appointmentService,
            Func<Task> refreshCallback = null,
            Action onStatusChanged = null)
        {
            _appointment = appointment ?? throw new ArgumentNullException(nameof(appointment));
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _refreshCallback = refreshCallback;

            EditCommand = new AsyncRelayCommand(EditAppointment);
            CancelCommand = new AsyncRelayCommand(CancelAppointment);
            CompleteCommand = new AsyncRelayCommand(CompleteAppointment);
        }

        private async Task EditAppointment()
        {
            //ToDo: Create a dialog for editing appoinyments
            try
            {
                _appointment.Reason = $"{_appointment.Reason} (Updated)";
                await _appointmentService.UpdateAppointmentAsync(_appointment);

                await RefreshParent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing appointment: {ex.Message}");
            }
        }

        private async Task CancelAppointment()
        {
            try
            {
                bool result = await _appointmentService.CancelAppointmentAsync(_appointment.Id);
                if (result)
                {
                    _appointment.Status = "cancelled";
                    OnPropertyChanged(nameof(Status));

                    await RefreshParent();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling appointment: {ex.Message}");
            }
        }

        private async Task CompleteAppointment()
        {
            try
            {
                _appointment.Status = "completed";
                await _appointmentService.UpdateAppointmentAsync(_appointment);
                OnPropertyChanged(nameof(Status));
                await RefreshParent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completing appointment: {ex.Message}");
            }
        }

        private async Task RefreshParent()
        {
            if (_refreshCallback != null)
            {
                await _refreshCallback();
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
}