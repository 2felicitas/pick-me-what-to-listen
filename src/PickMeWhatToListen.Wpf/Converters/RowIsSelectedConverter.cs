using System.Globalization;
using System.Windows.Data;

namespace PickMeWhatToListen.Wpf.Converters;

/// <summary>
/// Compares an artist row's <see cref="Guid"/> against the currently selected
/// artist's id, so the row highlight can be driven by <c>MainViewModel.SelectedArtist</c>
/// instead of the ListBox's own (deliberately disabled) selection state — see
/// the "ArtistRowContainerStyle" comment in MainWindow.xaml.
/// </summary>
public sealed class RowIsSelectedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values is [Guid rowId, Guid selectedId] && rowId == selectedId;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
