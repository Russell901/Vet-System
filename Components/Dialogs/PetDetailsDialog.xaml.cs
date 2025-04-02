using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vet_System.ViewModels;

namespace Vet_System.Components.Dialogs
{
    public sealed partial class PetDetailsDialog : UserControl
    {
        public PetDetailsDialogViewModel ViewModel { get; }
        public PetDetailsDialog(string petId, XamlRoot xamlRoot) // Add XamlRoot parameter
        {
            this.InitializeComponent();
            ViewModel = new PetDetailsDialogViewModel(petId, xamlRoot); // Pass XamlRoot to ViewModel
            DataContext = ViewModel;
        }
    }
}
