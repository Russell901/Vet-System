using Microsoft.UI.Xaml.Controls;
using Vet_System.ViewModels;

namespace Vet_System.Pages
{
    public sealed partial class PatientsPage : Page
    {
        public PatientsViewModel ViewModel { get; }
        public PatientsPage()
        {
            this.InitializeComponent();
            ViewModel = new PatientsViewModel();
        }
    }
}
