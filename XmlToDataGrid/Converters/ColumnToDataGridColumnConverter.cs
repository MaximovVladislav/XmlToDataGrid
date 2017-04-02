using System;
using System.Configuration;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using XmlToDataGrid.Models;

namespace XmlToDataGrid.Converters
{
    public class ColumnToDataGridColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Column column = value as Column;

            if (column == null) return Binding.DoNothing;

            if (column.ValueType == typeof(string))
            {
                DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
                newDataGridColumn.Binding = new Binding(column.Name);
                newDataGridColumn.Header = column.Name;
                return newDataGridColumn;
            }
            else if (column.ValueType == typeof(DateTime))
            {
                DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
                Binding binding = new Binding(column.Name);
                binding.StringFormat = ConfigurationManager.AppSettings["terminalDateFormat"];
                newDataGridColumn.Binding = binding;
                newDataGridColumn.Header = column.Name;
                return newDataGridColumn;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}