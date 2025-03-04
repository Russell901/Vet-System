using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vet_System.Models;

namespace Vet_System.ViewModels
{
    public partial class AddPatientDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string petName = string.Empty;

        [ObservableProperty]
        private string selectedSpecies = string.Empty;

        [ObservableProperty]
        private string breed = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? dateOfBirth = DateTimeOffset.Now;

        [ObservableProperty]
        private string ownerName = string.Empty;

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string address = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private bool hasValidationErrors;

        [ObservableProperty]
        private string validationMessage = string.Empty;

        public List<string> SpeciesList { get; } = new List<string>
        {
            "Dog",
            "Cat",
            "Bird",
            "Rabbit",
            "Hamster",
            "Other"
        };

        public async Task<bool> ValidateAndSaveAsync()
        {
            // Reset validation state
            HasValidationErrors = false;
            ValidationMessage = string.Empty;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(PetName))
            {
                HasValidationErrors = true;
                ValidationMessage = "Pet name is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedSpecies))
            {
                HasValidationErrors = true;
                ValidationMessage = "Species is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(OwnerName))
            {
                HasValidationErrors = true;
                ValidationMessage = "Owner name is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                HasValidationErrors = true;
                ValidationMessage = "Phone number is required";
                return false;
            }

            // Create new pet object
            var newPet = new PetItem
            {
                Name = PetName,
                Species = SelectedSpecies.ToLowerInvariant(),
                SpeciesBreed = Breed,
                Age = CalculateAge(DateOfBirth),
                Owner = new OwnerInfo
                {
                    Name = OwnerName,
                    Phone = PhoneNumber
                },
                ImageUrl = new Uri("ms-appx:///Assets/Pets/default.jpg"),
                NextAppointmentDate = "Not scheduled"
            };

            // TODO: Add to database or service
            // For now, we'll just add it to the collection in PatientsViewModel
            await Task.Delay(500); // Simulate network delay

            return true;
        }

        private string CalculateAge(DateTimeOffset? birthDate)
        {
            if (!birthDate.HasValue)
                return "Unknown";

            var today = DateTimeOffset.Now;
            var age = today.Year - birthDate.Value.Year;

            if (today.Month < birthDate.Value.Month ||
                (today.Month == birthDate.Value.Month && today.Day < birthDate.Value.Day))
            {
                age--;
            }

            return age == 1 ? "1 year" : $"{age} years";
        }
    }
}