using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Vet_System.Components.Dialogs;
using Vet_System.Models;
using Vet_System.Services;

namespace Vet_System.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly DialogService _dialogService;
        private readonly Window _mainWindow;
        private ContentDialog addPatientDialog;

        public XamlRoot XamlRoot
        {
            get => _xamlRoot;
            set
            {
                if (value != null)
                {
                    _xamlRoot = value;
                    _dialogService?.UpdateXamlRoot(value);
                }
            }
        }
        private XamlRoot _xamlRoot;

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
            _mainWindow = window;

            // Initialize services
            if (window.Content is FrameworkElement element)
            {
                _xamlRoot = element.XamlRoot;
                _dialogService = new DialogService(_xamlRoot);
                _databaseService = new DatabaseService(_xamlRoot);
            }
            else
            {
                Debug.WriteLine("Warning: Window.Content is not FrameworkElement");
            }


            InitializeDatabaseAsync().ConfigureAwait(false);
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                await _databaseService.InitializeAsync();
                await LoadPetsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                await _dialogService.ShowErrorAsync("Database Error",
                    "Failed to initialize database. Please ensure MySQL is running.");
            }
        }

        [RelayCommand]
        private async Task AddPatientAsync()
        {
            try
            {
                if (_xamlRoot == null)
                {
                    if (_mainWindow?.Content is FrameworkElement element)
                    {
                        _xamlRoot = element.XamlRoot;
                        _dialogService?.UpdateXamlRoot(_xamlRoot);
                    }

                    if (_xamlRoot == null)
                    {
                        Debug.WriteLine("Error: Unable to get XamlRoot from window or page");
                        await _dialogService.ShowErrorAsync("Error", "Cannot show dialog - XamlRoot not set");
                        return;
                    }
                }

                var addPatientContent = new AddPatientDialog();

                var dialog = new ContentDialog
                {
                    XamlRoot = _xamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Add New Patient",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = addPatientContent
                };

                var result = await dialog.ShowAsync();


                if (result == ContentDialogResult.Primary)
                {
                    var viewModel = addPatientContent?.ViewModel;

                    if (!viewModel.Validate())
                    {
                        return;
                    }

                    var newPet = new PetItem
                    {
                        Name = viewModel.PetName,
                        Species = viewModel.SelectedSpecies?.ToLowerInvariant() ?? "unknown",
                        SpeciesBreed = viewModel.Breed,
                        Age = CalculateAge(viewModel.DateOfBirth),
                        Owner = new OwnerInfo
                        {
                            Name = viewModel.OwnerName,
                            Phone = viewModel.PhoneNumber,
                            Email = viewModel.Email,
                            Address = viewModel.Address
                        },
                        NextAppointmentDate = "Not scheduled",
                        ImageUrl = new Uri("ms-appx:///Assets/Pets/default.jpg")
                    };

                    await SaveNewPetAsync(newPet);
                }
            
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddPatientAsync: {ex.Message}");
                await _dialogService.ShowErrorAsync("Error",
                    "Unable to show the Add Patient dialog. Please try again.");
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

private async Task SaveNewPetAsync(PetItem newPet)
{
    try
    {
        await _databaseService.AddPetAsync(newPet);
        allPets.Add(newPet);
        OnPropertyChanged(nameof(FilteredPets));
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error saving new pet: {ex.Message}");
        await _dialogService.ShowErrorAsync("Error",
            "Unable to save new pet to database.");
    }
}

private async Task LoadPetsAsync()
{
    try
    {
        var pets = await _databaseService.GetAllPetsAsync();
        allPets.Clear();
        foreach (var pet in pets)
        {
            allPets.Add(pet);
        }
        OnPropertyChanged(nameof(FilteredPets));
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error loading pets: {ex.Message}");
        await _dialogService.ShowErrorAsync("Error",
            "Unable to load pets from database.");
    }
}

public void OnAddPatientSaved(PetItem newPet)
{
    allPets.Add(newPet);
    OnPropertyChanged(nameof(FilteredPets));
    addPatientDialog?.Hide();
}
    }
}