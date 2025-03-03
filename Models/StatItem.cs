using Microsoft.UI.Xaml.Media;

namespace Vet_System.Models
{
    public class StatItem
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public Brush Background { get; set; } = null!;
        public Brush IconColor { get; set; } = null!;
    }
}