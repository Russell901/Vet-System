using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vet_System.Components
{
    public sealed partial class Sidebar : UserControl
    {
        public ObservableCollection<NavItem> NavItems
        {
            get => (ObservableCollection<NavItem>)GetValue(NavItemsProperty);
            set => SetValue(NavItemsProperty, value);
        }

        public static readonly DependencyProperty NavItemsProperty =
            DependencyProperty.Register(nameof(NavItems), typeof(ObservableCollection<NavItem>),
                typeof(Sidebar), new PropertyMetadata(null));

        public NavItem SelectedNavItem
        {
            get => (NavItem)GetValue(SelectedNavItemProperty);
            set => SetValue(SelectedNavItemProperty, value);
        }

        public static readonly DependencyProperty SelectedNavItemProperty =
            DependencyProperty.Register(nameof(SelectedNavItem), typeof(NavItem),
                typeof(Sidebar), new PropertyMetadata(null));

        public ICommand LogoutCommand
        {
            get => (ICommand)GetValue(LogoutCommandProperty);
            set => SetValue(LogoutCommandProperty, value);
        }

        public static readonly DependencyProperty LogoutCommandProperty =
            DependencyProperty.Register(nameof(LogoutCommand), typeof(ICommand),
                typeof(Sidebar), new PropertyMetadata(null));

        public Sidebar()
        {
            this.InitializeComponent();
        }
    }
}
