using Windows.UI;
using Microsoft.UI.Xaml.Data;

namespace NetworkParser.UI.Views.Converters;

internal class ProtocolColorConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, string language) {
        return (value?.ToString() ?? "") switch {
            "Tcp" => new SolidColorBrush(Color.FromArgb(255, 25, 55, 100)), 
            "Udp" => new SolidColorBrush(Color.FromArgb(255, 20, 70, 30)), 
            "Dns" => new SolidColorBrush(Color.FromArgb(255, 70, 25, 100)),  
            "Arp" => new SolidColorBrush(Color.FromArgb(255, 90, 75, 10)), 
            "Icmp" => new SolidColorBrush(Color.FromArgb(255, 100, 45, 10)),
            "Tls" => new SolidColorBrush(Color.FromArgb(255, 0, 80, 80)),
            _ => new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),   
        };
    }
    public object ConvertBack (object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
