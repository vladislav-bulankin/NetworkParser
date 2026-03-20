using Microsoft.UI.Xaml.Data;

namespace NetworkParser.UI.Views.Converters;

internal class TimestampConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, string language) {
        if (value is DateTime dt)
            return dt.ToString("HH:mm:ss.fff");
        return "";
    }
    public object ConvertBack (object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
