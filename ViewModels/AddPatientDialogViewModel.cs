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


        public AddPatientDialogViewModel()
        {
            System.Diagnostics.Debug.WriteLine("Current Date and Time (UTC): 2025-03-05 19:55:16");
            System.Diagnostics.Debug.WriteLine("Current User's Login: Russell901");

            // Set default values
            DateOfBirth = DateTimeOffset.Now;
            HasValidationErrors = false;
            ValidationMessage = string.Empty;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(PetName))
            {
                ValidationMessage = "Pet name is required";
                HasValidationErrors = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedSpecies))
            {
                ValidationMessage = "Species is required";
                HasValidationErrors = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(OwnerName))
            {
                ValidationMessage = "Owner name is required";
                HasValidationErrors = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                ValidationMessage = "Phone number is required";
                HasValidationErrors = true;
                return false;
            }

            HasValidationErrors = false;
            ValidationMessage = string.Empty;
            return true;
        }
      
    }
}