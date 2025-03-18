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
                    }
                    if (args.Phase == 1)
                    {
                        var compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread();
                        var animation = compositor.CreateVector3KeyFrameAnimation();
                        animation.InsertKeyFrame(1, new Vector3(0, 0, 0));
                        animation.Duration = TimeSpan.FromMilliseconds(200);
                        animation.DelayTime = TimeSpan.FromMilliseconds(args.ItemIndex * 50);

                        args.ItemContainer.StartAnimation(animation);
                        args.ItemContainer.Opacity = 1;
                    }
                };
            }
        }
    }
}