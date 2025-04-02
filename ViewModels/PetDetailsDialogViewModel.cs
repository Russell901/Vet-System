using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Vet_System.Models;
using Vet_System.Services;

namespace Vet_System.ViewModels
{
    public partial class PetDetailsDialogViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly IAppointmentService _appointmentService;
        private readonly IPetService _petService;

        [ObservableProperty]
        private PetItem pet;

        [ObservableProperty]
        private OwnerInfo owner;

        [ObservableProperty]
        private string notes;

        [ObservableProperty]
        private bool hasNotes;

        [ObservableProperty]
        private string age;

        [ObservableProperty]
        private ObservableCollection<AppointmentItem> upcomingAppointments = new();

        [ObservableProperty]
        private ObservableCollection<AppointmentItem> appointmentHistory = new();

        [ObservableProperty]
        private bool hasUpcomingAppointments;

        [ObservableProperty]
        private bool hasAppointmentHistory;

        [ObservableProperty]
        private bool isLoading = true;

        public PetDetailsDialogViewModel(string petId, XamlRoot xamlRoot)
        {
            _databaseService = new DatabaseService(xamlRoot);
            _petService = new PetService(DatabaseService.DefaultConnectionString);

            // Initialize the pet
            LoadPetDetailsAsync(petId).ConfigureAwait(false);
        }

        private async Task LoadPetDetailsAsync(string petId)
        {
            try
            {
                IsLoading = true;

                // Load pet details
                Pet = await _petService.GetPetByIdAsync(petId);

                if (Pet != null)
                {
                    // Calculate age
                    Age = CalculateAge(Pet.DateOfBirth);

                    //// Load owner details
                    //Owner = await _databaseService.GetOwnerByIdAsync(Pet.OwnerId);
                    //if (Owner == null)
                    //{
                    //    Owner = new OwnerInfo
                    //    {
                    //        Name = Pet.Owner,
                    //        Phone = "Not available",
                    //        Email = "Not available",
                    //        Address = "Not available"
                    //    };
                    //}

                    // Load appointments
                    await LoadAppointmentsAsync(petId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading pet details: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAppointmentsAsync(string petId)
        {
            try
            {
                // Get all appointments for this pet
                var allAppointments = await GetAppointmentsByPetIdAsync(petId);

                var now = DateTime.Now;

                // Split into upcoming and past appointments
                var upcoming = allAppointments
                    .Where(a => a.DateTime > now)
                    .OrderBy(a => a.DateTime)
                    .ToList();

                var past = allAppointments
                    .Where(a => a.DateTime <= now)
                    .OrderByDescending(a => a.DateTime)
                    .ToList();

                // Update collections
                UpcomingAppointments.Clear();
                foreach (var appointment in upcoming)
                {
                    UpcomingAppointments.Add(appointment);
                }
                HasUpcomingAppointments = UpcomingAppointments.Count > 0;

                AppointmentHistory.Clear();
                foreach (var appointment in past)
                {
                    AppointmentHistory.Add(appointment);
                }
                HasAppointmentHistory = AppointmentHistory.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
        }

        private async Task<IEnumerable<AppointmentItem>> GetAppointmentsByPetIdAsync(string petId)
        {
            if (string.IsNullOrEmpty(petId))
                return Array.Empty<AppointmentItem>();

            try
            {
                var allAppointments = await _appointmentService.GetAppointmentsAsync();
                return allAppointments.Where(a => a.PetName == Pet?.Name).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAppointmentsByPetIdAsync: {ex.Message}");
                return Array.Empty<AppointmentItem>();
            }
        }


        private string CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            // Adjust age if birthday hasn't occurred yet this year
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age == 1 ? "1 year" : $"{age} years";
        }
    }
}
