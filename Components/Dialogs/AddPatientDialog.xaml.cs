using Microsoft.UI.Xaml.Controls;
using Vet_System.ViewModels;

namespace Vet_System.Components.Dialogs
{
    public sealed partial class AddPatientDialog : UserControl
    {
        public AddPatientDialogViewModel ViewModel { get; }
        public AddPatientDialog()
        {
            this.InitializeComponent();
            ViewModel = new AddPatientDialogViewModel();
        }
    }
}
