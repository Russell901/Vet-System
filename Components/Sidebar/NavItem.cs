using System;

namespace Vet_System.Components
{
    public class NavItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public Type PageType { get; set; } = typeof(Pages.DashboardPage);

        public NavItem()
        {
        }

        public NavItem(Type pageType)
        {
            PageType = pageType;
        }
    }
}