using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Vet_System.Models;

namespace Vet_System.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string currentDateTime = "2025-03-03 15:20:39";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredPets))]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredPets))]
        private string filterSpecies = "all";

        private ObservableCollection<PetItem> allPets = new();

        public ObservableCollection<PetItem> FilteredPets
        {
            get
            {
                var query = allPets.AsEnumerable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(pet =>
                        pet.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        pet.SpeciesBreed.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (filterSpecies != "all")
                {
                    query = query.Where(pet => pet.Species == filterSpecies);
                }

                return new ObservableCollection<PetItem>(query);
            }
        }

        public PatientsViewModel()
        {
            LoadSampleData();
        }

        [RelayCommand]
        private void AddPatient()
        {
            // Implement add patient logic
        }

        private void LoadSampleData()
        {
            allPets.Add(new PetItem
            {
                Name = "Max",
                Age = "3 years",
                Species = "dog",
                SpeciesBreed = "Golden Retriever",
                ImageUrl = new Uri("ms-appx:///Assets/Pets/max.jpg"),
                Owner = new OwnerInfo
                {
                    Name = "John Smith",
                    Phone = "(555) 123-4567"
                },
                NextAppointmentDate = "Mar 15"
            });

            // Add more sample data
            allPets.Add(new PetItem
            {
                Name = "Luna",
                Age = "2 years",
                Species = "cat",
                SpeciesBreed = "Siamese",
                ImageUrl = new Uri("ms-appx:///Assets/Pets/luna.jpg"),
                Owner = new OwnerInfo
                {
                    Name = "Sarah Johnson",
                    Phone = "(555) 234-5678"
                },
                NextAppointmentDate = "Mar 10"
            });
        }
    }
}