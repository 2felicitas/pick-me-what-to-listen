using System.Globalization;
using System.Windows.Data;

namespace PickMeWhatToListen.Wpf.Converters;

/// <summary>
/// Scales a design-time pixel measurement linearly with the host window's
/// <see cref="System.Windows.FrameworkElement.ActualWidth"/>. Used for control
/// chrome (heights, widths, primary-button font sizes) — not body typography
/// in the details panel or list rows.
/// </summary>
public sealed class ResponsiveScaleConverter : IValueConverter
{
    /// <summary>Matches <c>MainWindow</c>'s default <c>Width</c> — the size the base measurements were tuned at.</summary>
    private const double ReferenceWidth = 760;

    /// <summary>Matches <c>MainWindow</c>'s <c>MinWidth</c> — scale factor floor.</summary>
    private const double MinWindowWidth = 640;

    /// <summary>Cap so primary controls don't balloon on very wide windows while text stays fixed.</summary>
    private const double MaxScaleFactor = 1.25;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double actualWidth)
        {
            return Fallback(parameter, culture);
        }

        if (!TryParseBase(parameter, culture, out double baseValue))
        {
            return 0.0;
        }

        double scale = Math.Clamp(
            actualWidth / ReferenceWidth,
            MinWindowWidth / ReferenceWidth,
            MaxScaleFactor);

        return baseValue * scale;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static object Fallback(object? parameter, CultureInfo culture) =>
        TryParseBase(parameter, culture, out double baseValue) ? baseValue : 0.0;

    private static bool TryParseBase(object? parameter, CultureInfo culture, out double baseValue) =>
        parameter switch
        {
            double d => (baseValue = d) == d,
            int i => (baseValue = i) == i,
            string s => double.TryParse(s, NumberStyles.Any, culture, out baseValue),
            _ => (baseValue = 0) == 0 && false,
        };
}
