using System;

namespace Vet_System.Lib
{
    public class CustomNavigationEventArgs : EventArgs
    {
        required public Type PageType { get; set; }
        public object? Parameter { get; set; }
    }
}
