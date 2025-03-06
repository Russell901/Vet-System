using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Vet_System.Models;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

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
        private string nextAppointmentDate = string.Empty;

        [ObservableProperty]
        private Uri selectedImageUri = new Uri("ms-appx:///Assets/Pets/default.jpg");


        [ObservableProperty]
        private string selectedImageName = "No image selected";

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private bool hasValidationErrors;

        [ObservableProperty]
        private string validationMessage = string.Empty;

        private StorageFile selectedImageFile;

        public List<string> SpeciesList { get; } = new List<string>
        {
            "Dog",
            "Cat",
            "Bird",
            "Rabbit",
            "Hamster",
            "Other"
        };

        public async Task SelectImageAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };

                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    // Validate file size (2MB limit)
                    var properties = await file.GetBasicPropertiesAsync();
                    if (properties.Size > 2 * 1024 * 1024)
                    {
                        selectedImageName = "Error: File size must be less than 2MB";
                        return;
                    }

                    selectedImageFile = file;
                    selectedImageName = file.Name;

                    // Create preview
                    await CreateImagePreviewAsync(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting image: {ex.Message}");
                selectedImageName = "Error selecting image";
            }
        }

        private async Task CreateImagePreviewAsync(StorageFile file)
        {
            try
            {
                using var stream = await file.OpenAsync(FileAccessMode.Read);
                using var inputStream = stream.GetInputStreamAt(0);

                var tempFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(
                    "Pets", CreationCollisionOption.OpenIfExists);

                var tempFile = await tempFolder.CreateFileAsync(
                    "temp_preview.jpg", CreationCollisionOption.ReplaceExisting);

                using var outputStream = await tempFile.OpenAsync(FileAccessMode.ReadWrite);
                await RandomAccessStream.CopyAsync(inputStream, outputStream);

                selectedImageUri = new Uri(tempFile.Path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating preview: {ex.Message}");
                selectedImageUri = new Uri("ms-appx:///Assets/Pets/default.jpg");
            }
        }

        public async Task<string> SaveImageAsync()
        {
            if (selectedImageFile == null)
            {
                return "ms-appx:///Assets/Pets/default.jpg";
            }

            try
            {
                var petsFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation
                    .GetFolderAsync("Assets\\Pets");

                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(selectedImageFile.Name)}";
                var destinationFile = await selectedImageFile.CopyAsync(
                    petsFolder, newFileName, NameCollisionOption.GenerateUniqueName);

                return $"ms-appx:///Assets/Pets/{destinationFile.Name}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
                return "ms-appx:///Assets/Pets/default.jpg";
            }
        }

        public AddPatientDialogViewModel()
        {
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