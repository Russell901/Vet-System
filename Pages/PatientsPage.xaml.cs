using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vet_System.ViewModels;

namespace Vet_System.Pages
{
    public sealed partial class PatientsPage : Page
    {
        public PatientsViewModel ViewModel { get; }
        public PatientsPage()
        {
            this.InitializeComponent();
            ViewModel = new PatientsViewModel(App.MainWindow);
            this.Loaded += PatientsPage_Loaded;
        }

        private void PatientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && this.XamlRoot != null)
            {
                ViewModel.XamlRoot = this.XamlRoot;
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set XamlRoot for dialogs
            if (ViewModel != null)
            {
                ViewModel.XamlRoot = this.XamlRoot;
            }
        }
    }
}
