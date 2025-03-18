using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Vet_System.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string status)
                return new SolidColorBrush(GetColor("#6B7280")); 

            var (backgroundColor, textColor) = status.ToLower() switch
            {
                "scheduled" => ("#EFF6FF", "#1D4ED8"),    
                "completed" => ("#ECFDF5", "#059669"),    
                "cancelled" => ("#FEF2F2", "#DC2626"),    
                "urgent" => ("#FEF3C7", "#D97706"),   
                "in_progress" => ("#E0E7FF", "#4F46E5"),    
                _ => ("#F3F4F6", "#374151")                 
            };

            return parameter switch
            {
                "Background" => new SolidColorBrush(GetColor(backgroundColor)),
                "Text" => new SolidColorBrush(GetColor(textColor)),
                "Border" => new SolidColorBrush(GetColor(textColor)),
                _ => new SolidColorBrush(GetColor(textColor))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private Windows.UI.Color GetColor(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            var bytes = BitConverter.GetBytes(int.Parse(hex, System.Globalization.NumberStyles.HexNumber));
            return Windows.UI.Color.FromArgb(
                255,
                bytes[2],
                bytes[1],
                bytes[0]);
        }
    }
}
