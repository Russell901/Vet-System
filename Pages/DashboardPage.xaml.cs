using Microsoft.UI.Xaml.Controls;
using Vet_System.ViewModels;

namespace Vet_System.Pages
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; }
        public DashboardPage()
        {
            this.InitializeComponent();
            ViewModel = new DashboardViewModel();
        }
    }
}
