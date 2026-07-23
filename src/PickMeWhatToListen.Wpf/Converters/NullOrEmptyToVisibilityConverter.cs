using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PickMeWhatToListen.Wpf.Converters;

/// <summary>Collapses an element when the bound string is null or empty.</summary>
public sealed class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
