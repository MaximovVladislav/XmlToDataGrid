using System;
using System.Globalization;
using System.Windows.Data;

namespace XmlToDataGrid.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                TimeSpan ts = (TimeSpan) value;
                return $"{(int)ts.TotalHours}:{ts.Minutes}:{ts.Seconds}";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}