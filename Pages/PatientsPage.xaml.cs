using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set XamlRoot for dialogs
            if (ViewModel != null)
            {
                ViewModel.XamlRoot = this.XamlRoot;
                Task.Run(() => ViewModel.LoadPetsAsync());
            }
            else
            {

                this.Loaded += Page_Loaded;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.XamlRoot != null)
            {
                ViewModel.XamlRoot = this.XamlRoot;
                this.Loaded -= Page_Loaded;
            }
        }
    }
}
