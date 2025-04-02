using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Vet_System.Models;
using Vet_System.Services;

namespace Vet_System.ViewModels
{
    public class AppointmentFormViewModel : ObservableObject
    {
        private readonly IAppointmentService? _appointmentService;
        private readonly IPetService? _petService;
        private readonly TaskCompletionSource<AppointmentItem?> _taskCompletionSource;
        private readonly XamlRoot? _xamlRoot;
        private readonly AppointmentItem? _originalAppointment;

        private PetItem? _selectedPet;
        private string _ownerName = string.Empty;
        private DateTimeOffset _appointmentDate;
        private TimeSpan _appointmentTime;
        private string _selectedAppointmentType = string.Empty;
        private string _reason = string.Empty;
        private string _status = "scheduled";
        private bool _hasValidationErrors;
        private ObservableCollection<string> _validationErrors = new();
        public ObservableCollection<PetItem> Pets { get; } = new();
        public bool IsEditMode => _originalAppointment != null;
        private bool _isLoadingPets = false;

        public DateTimeOffset MinDate => DateTimeOffset.Now;
        public List<string> AppointmentTypes { get; } = new()
        {
            "Wellness Check",
            "Vaccination",
            "Illness",
            "Injury",
            "Surgery",
            "Follow-up",
            "Dental Cleaning",
            "Other"
        };

        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set => SetProperty(ref _validationErrors, value);
        }

        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            set => SetProperty(ref _hasValidationErrors, value);
        }

        public bool IsLoadingPets
        {
            get => _isLoadingPets;
            set => SetProperty(ref _isLoadingPets, value);
        }

        public PetItem? SelectedPet
        {
            get => _selectedPet;
            set
            {
                if (SetProperty(ref _selectedPet, value) && value != null)
                {
                    OwnerName = value.Owner ?? string.Empty;
                    UpdateCanSaveCommand();
                }
            }
        }


        public string OwnerName
        {
            get => _ownerName;
            set => SetProperty(ref _ownerName, value);
        }

        public DateTimeOffset AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                if (SetProperty(ref _appointmentDate, value))
                {
                    UpdateCanSaveCommand();
                }
            }
        }

        public TimeSpan AppointmentTime
        {
            get => _appointmentTime;
            set
            {
                if (SetProperty(ref _appointmentTime, value))
                {
                    UpdateCanSaveCommand();
                }
            }
        }

        public string SelectedAppointmentType
        {
            get => _selectedAppointmentType;
            set
            {
                if (SetProperty(ref _selectedAppointmentType, value))
                {
                    UpdateCanSaveCommand();
                }
            }
        }

        public string Reason
        {
            get => _reason;
            set
            {
                if (SetProperty(ref _reason, value))
                {
                    UpdateCanSaveCommand();
                }
            }
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public List<StatusFilter> StatusOptions { get; } = new()
        {
            new StatusFilter("Scheduled", "scheduled"),
            new StatusFilter("Completed", "completed"),
            new StatusFilter("Cancelled", "cancelled")
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public AppointmentFormViewModel(
            AppointmentItem? appointment,
            IAppointmentService? appointmentService,
            IPetService? petService,
            XamlRoot? xamlRoot)
        {
            _originalAppointment = appointment;
            _appointmentService = appointmentService ??
                throw new ArgumentException(nameof(appointmentService));
            _petService = petService ??
                throw new ArgumentException(nameof(petService));
            _taskCompletionSource = new TaskCompletionSource<AppointmentItem?>();
            _xamlRoot = xamlRoot;

            // Load initial data
            _ = LoadPetsAsync();

            // If editing an existing appointment, populate fields
            if (appointment != null)
            {
                _appointmentDate = appointment.DateTime.Date;
                _appointmentTime = appointment.DateTime.TimeOfDay;
                _reason = appointment.Reason;
                _status = appointment.Status;
                // Pet will be set after pets are loaded
            }

            AppointmentDate = DateTime.Now.AddDays(1).Date.Add(new TimeSpan(9, 0, 0));
            AppointmentTime = _originalAppointment?.DateTime.TimeOfDay ?? TimeSpan.Zero;
            Reason = _originalAppointment?.Reason ?? string.Empty;
            Status = _originalAppointment?.Status ?? "scheduled";

            if (!string.IsNullOrEmpty(_originalAppointment?.Reason))
            {
                SelectedAppointmentType = AppointmentTypes.FirstOrDefault(t =>
                    _originalAppointment.Reason.Contains(t, StringComparison.OrdinalIgnoreCase)) ?? AppointmentTypes[0];
            }
            else
            {
                SelectedAppointmentType = AppointmentTypes[0];
            }

            SaveCommand = new AsyncRelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void UpdateCanSaveCommand()
        {
            (SaveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task LoadPetsAsync()
        {
            try
            {
                var petsList = await _petService.GetPetsAsync();
                Pets.Clear();

                foreach (var pet in petsList)
                {
                    Pets.Add(pet);
                }

                if (_originalAppointment != null)
                {
                    SelectedPet = Pets.FirstOrDefault(p => p.Name == _originalAppointment.PetName);
                    OwnerName = _originalAppointment.OwnerName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading pets: {ex.Message}");
                ValidationErrors.Add($"Could not load pets: {ex.Message}");
                HasValidationErrors = true;
            }
            finally
            {
                IsLoadingPets = false;
            }
        }

        private bool CanSave()
        {
            Validate();
            return !HasValidationErrors;
        }

        private void Validate()
        {
            ValidationErrors.Clear();

            if (SelectedPet == null)
                ValidationErrors.Add("Please select a pet");

            if (string.IsNullOrWhiteSpace(SelectedAppointmentType))
                ValidationErrors.Add("Please select an appointment type");

            if (string.IsNullOrWhiteSpace(Reason))
                ValidationErrors.Add("Please provide a reason for the visit");

            if (AppointmentDate.Date < DateTime.Now.Date)
                ValidationErrors.Add("Appointment date cannot be in the past");

            if (AppointmentDate.Date == DateTime.Now.Date &&
                AppointmentTime < DateTime.Now.TimeOfDay)
                ValidationErrors.Add("Appointment time cannot be in the past");

            if (AppointmentTime.Hours < 8 || AppointmentTime.Hours >= 18)
                ValidationErrors.Add("Appointments must be between 8:00 AM and 6:00 PM");

            HasValidationErrors = ValidationErrors.Count > 0;
        }

        public async Task Save()
        {
            Validate();
            if (HasValidationErrors || SelectedPet == null || _appointmentService == null)
                return;

            try
            {
                var appointmentDateTime = AppointmentDate.Date.Add(AppointmentTime);

                AppointmentItem appointment;
                if (IsEditMode)
                {
                    // Update existing appointment
                    appointment = new AppointmentItem(
                        _originalAppointment.Id,
                        SelectedPet.Name,
                        OwnerName,
                        appointmentDateTime,
                        Reason,
                        Status
                    );
                    await _appointmentService.UpdateAppointmentAsync(appointment);
                }
                else
                {
                    appointment = new AppointmentItem(
                       null,
                       SelectedPet.Name,
                       OwnerName,
                       appointmentDateTime,
                       Reason,
                       "scheduled"
                   );

                    await _appointmentService.CreateAppointmentAsync(appointment);
                }
                _result = appointment;
            }
            catch (Exception ex)
            {
                ValidationErrors.Add($"Error saving appointment: {ex.Message}");
                HasValidationErrors = true;
                throw;
            }
        }

        private AppointmentItem _result;

        public Task<AppointmentItem> GetResultAsync()
        {
            return Task.FromResult(_result);
        }

        private void Cancel()
        {
            _taskCompletionSource.SetResult(null);
        }


        private async Task ShowErrorDialog(string title, string message)
        {
            if (_xamlRoot == null)
                return;

            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }
    }
}
