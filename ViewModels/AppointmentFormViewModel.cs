using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        private AppointmentItem _appointment;
        private PetItem? _selectedPet;
        private string _ownerName = string.Empty;
        private DateTimeOffset _appointmentDate;
        private TimeSpan _appointmentTime;
        private string _selectedAppointmentType = string.Empty;
        private string _reason = string.Empty;
        private string _status = "scheduled";
        private bool _hasValidationErrors;
        private ObservableCollection<string> _validationErrors = new();
        private bool _isNewAppointment;
        private bool _isLoadingPets = false;

        public string Title => IsNewAppointment ? "Schedule New Appointment" : "Edit Appointment";
        public bool IsNewAppointment => _isNewAppointment;
        public DateTimeOffset MinDate => DateTimeOffset.Now;
        public ObservableCollection<PetItem> Pets { get; } = new();
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
        public List<string> StatusOptions { get; } = new()
        {
            "scheduled",
            "completed",
            "cancelled"
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AppointmentFormViewModel() : this(null)
        {
        }
        private XamlRoot _xamlRoot;
        public AppointmentFormViewModel(
            AppointmentItem? appointment = null,
            IAppointmentService? appointmentService = null,
            IPetService? petService = null,
            XamlRoot? xamlRoot = null)
        {
            _appointmentService = appointmentService ?? new AppointmentService(DatabaseService.DefaultConnectionString);
            _petService = petService ?? new PetService(DatabaseService.DefaultConnectionString);
            _taskCompletionSource = new TaskCompletionSource<AppointmentItem?>();
            _isNewAppointment = appointment == null;
            _xamlRoot = xamlRoot;

            _appointment = appointment ?? new AppointmentItem(
                string.Empty,
                string.Empty,
                string.Empty,
                DateTime.Now.AddDays(1).Date.Add(new TimeSpan(9, 0, 0)),
                string.Empty,
                "scheduled");

            AppointmentDate = _appointment.DateTime.Date;
            AppointmentTime = _appointment.DateTime.TimeOfDay;
            Reason = _appointment.Reason;
            Status = _appointment.Status;

            if (!string.IsNullOrEmpty(_appointment.Reason))
            {
                SelectedAppointmentType = AppointmentTypes.FirstOrDefault(t =>
                    _appointment.Reason.Contains(t, StringComparison.OrdinalIgnoreCase)) ?? AppointmentTypes[0];
            }
            else
            {
                SelectedAppointmentType = AppointmentTypes[0];
            }

            SaveCommand = new AsyncRelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            _ = LoadPetsAsync();
        }

        private void UpdateCanSaveCommand()
        {
            (SaveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task LoadPetsAsync()
        {
            try
            {
                if (_petService == null)
                    return;

                IsLoadingPets = true;

                var pets = await _petService.GetPetsAsync();
                Pets.Clear();

                foreach (var pet in pets)
                {
                    Pets.Add(pet);
                }

                if (!string.IsNullOrEmpty(_appointment.PetName) && !_isNewAppointment)
                {
                    SelectedPet = Pets.FirstOrDefault(p => p.Name == _appointment.PetName);
                    OwnerName = _appointment.OwnerName;
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

                if (IsNewAppointment)
                {
                    var appointmentItem = new AppointmentItem(
                        string.Empty,
                        SelectedPet.Name,
                        OwnerName,
                        appointmentDateTime,
                        $"{SelectedAppointmentType}: {Reason}",
                        Status
                    );

                    System.Diagnostics.Debug.WriteLine($"Creating appointment for pet: {SelectedPet.Name} (ID: {SelectedPet.Id})");

                    var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointmentItem);
                    _taskCompletionSource.SetResult(createdAppointment);
                }
                else
                {
                    _appointment.PetName = SelectedPet.Name;
                    _appointment.OwnerName = OwnerName;
                    _appointment.DateTime = appointmentDateTime;
                    _appointment.Reason = $"{SelectedAppointmentType}: {Reason}";
                    _appointment.Status = Status;

                    await _appointmentService.UpdateAppointmentAsync(_appointment);
                    _taskCompletionSource.SetResult(_appointment);
                }
            }
            catch (Exception ex)
            {
                ValidationErrors.Add($"Error saving appointment: {ex.Message}");
                HasValidationErrors = true;
                throw;
            }
        }

        private void Cancel()
        {
            _taskCompletionSource.SetResult(null);
        }

        public Task<AppointmentItem?> GetResultAsync() => _taskCompletionSource.Task;

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
