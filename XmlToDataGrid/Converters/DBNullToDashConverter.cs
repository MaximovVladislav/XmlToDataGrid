using System;
using System.Globalization;
using System.Windows.Data;

namespace XmlToDataGrid.Converters
{
    /// <summary>
    /// Конвертер, заменяющий <see cref="DBNull"/> на прочерк
    /// </summary>
    public class DBNullToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DBNull)
            {
                return "-";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sValue = value as string;

            if (sValue != null && sValue == "-")
            {
                return DBNull.Value;
            }

            return value;
        }
    }
}