using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Vet_System.Components.Dialogs;
using Vet_System.Models;

namespace Vet_System.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        public XamlRoot XamlRoot { get; set; }
        private readonly Window mainWindow;
        private ContentDialog addPatientDialog;

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

        public PatientsViewModel(Window window)
        {
            mainWindow = window;
            LoadSampleData();
        }
        [RelayCommand]
        private async Task AddPatientAsync()
        {
            try
            {
                if (XamlRoot == null)
                {
                    // Log error or show message
                    System.Diagnostics.Debug.WriteLine("XamlRoot is null");
                    return;
                }

                if (addPatientDialog == null)
                {
                    addPatientDialog = new ContentDialog
                    {
                        XamlRoot = XamlRoot,
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "Add New Patient",
                        PrimaryButtonText = "Save",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary,
                        Content = new AddPatientDialog()
                    };
                }

                var result = await addPatientDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var addPatientContent = addPatientDialog.Content as AddPatientDialog;
                    var viewModel = addPatientContent?.ViewModel;

                    if (viewModel != null)
                    {
                        // Create new pet from dialog data
                        var newPet = new PetItem
                        {
                            Name = viewModel.PetName,
                            Species = viewModel.SelectedSpecies?.ToLowerInvariant() ?? "unknown",
                            SpeciesBreed = viewModel.Breed,
                            Age = CalculateAge(viewModel.DateOfBirth),
                            Owner = new OwnerInfo
                            {
                                Name = viewModel.OwnerName,
                                Phone = viewModel.PhoneNumber
                            },
                            NextAppointmentDate = "Not scheduled",
                            ImageUrl = new Uri("ms-appx:///Assets/Pets/default.jpg")
                        };

                        allPets.Add(newPet);
                        OnPropertyChanged(nameof(FilteredPets));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");

                if (XamlRoot != null)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Unable to show the Add Patient dialog. Please try again.",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot
                    };

                    await errorDialog.ShowAsync();
                }
            }
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

        private async void ShowError(string title, string message)
        {
            if (XamlRoot != null)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        public void OnAddPatientSaved(PetItem newPet)
        {
            allPets.Add(newPet);
            OnPropertyChanged(nameof(FilteredPets));
            addPatientDialog.Hide();
        }

        private void LoadSampleData()
        {
            allPets.Add(new PetItem
            {
                Name = "Max",
                Age = "3 years",
                Species = "dog",
                SpeciesBreed = "Golden Retriever",
                ImageUrl = new Uri("ms-appx:///Assets/Pets/max.png"),
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
                ImageUrl = new Uri("ms-appx:///Assets/Pets/luna.png"),
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