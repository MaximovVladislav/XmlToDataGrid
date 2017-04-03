using System;
using System.Globalization;
using System.Windows.Data;

namespace XmlToDataGrid.Converters
{
    public class DefaultDateToNothingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                DateTime dValue = (DateTime) value;
                if (dValue == default(DateTime))
                {
                    return Binding.DoNothing;
                }
                else
                {
                    return value;
                }
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}