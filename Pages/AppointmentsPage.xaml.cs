using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Numerics;
using Vet_System.ViewModels;

namespace Vet_System.Pages
{
    public sealed partial class AppointmentsPage : Page
    {
        public AppointmentsViewModel ViewModel { get; } = new AppointmentsViewModel();

        public AppointmentsPage()
        {
            ViewModel = new AppointmentsViewModel();
            this.InitializeComponent();
            Loaded += async (s, e) => await ViewModel.LoadAppointmentsAsync();
        }

        private void OnSearchTextChanged(AutoSuggestBox sender,
                                   AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.SearchTerm = sender.Text;
            }
        }

        private void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListView listView)
            {
                listView.ContainerContentChanging += static (s, args) =>
                {
                    if (args.Phase == 0)
                    {
                        args.ItemContainer.Opacity = 0;
                        args.ItemContainer.Translation = new Vector3(0, 20, 0);
                        args.RegisterUpdateCallback(OnPhaseCallback);
                    }
                };
            }
        }

        private static void OnPhaseCallback(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            // Just set the final properties directly instead of using animations
            args.ItemContainer.Opacity = 1;
            args.ItemContainer.Translation = new Vector3(0, 0, 0);

            // Add a slight delay based on item index
            var delay = args.ItemIndex * 30;
            if (delay > 0)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delay) };
                timer.Tick += (s, e) =>
                {
                    args.ItemContainer.Opacity = 1;
                    args.ItemContainer.Translation = new Vector3(0, 0, 0);
                    timer.Stop();
                };
                timer.Start();
            }
        }
    }
}