using Microsoft.UI.Xaml.Controls;
using Vet_System.ViewModels;

namespace Vet_System.Components.Dialogs
{
    public sealed partial class AppointmentFormDialog : UserControl
    {
        public AppointmentFormViewModel ViewModel { get; }

        public AppointmentFormDialog(AppointmentFormViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
        }
    }
}
