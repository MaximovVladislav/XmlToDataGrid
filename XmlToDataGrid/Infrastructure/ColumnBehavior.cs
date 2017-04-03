using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using XmlToDataGrid.Converters;
using XmlToDataGrid.Models;

namespace XmlToDataGrid.Infrastructure
{
    /// <summary>
    /// Класс, содержащий прикрепляющуюся коллекцию столбцов <see cref="Column"/> и определяющий ее поведение 
    /// </summary>
    public class ColumnBehavior
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns",
            typeof(ObservableCollection<Column>), typeof(ColumnBehavior),
            new UIPropertyMetadata(null, ColumnsPropertyChanged));

        private static DataGrid _dataGrid;
        private static readonly DBNullToDashConverter _dbNullToDashConverter = new DBNullToDashConverter();

        public static ObservableCollection<Column> GetColumns(DependencyObject element)
        {
            return (ObservableCollection<Column>)element.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        private static void ColumnsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            _dataGrid = sender as DataGrid;
            if (_dataGrid == null) return;
            _dataGrid.Columns.Clear();

            ObservableCollection<Column> columns = e.NewValue as ObservableCollection<Column>;

            if (columns == null) return;

            IList newColumns = (IList) e.NewValue;

            if (newColumns.Count > 0)
            {
                AddColumnsInAcyncContext(_dataGrid, newColumns);
            }

            columns.CollectionChanged += Columns_CollectionChanged;
        }

        private static void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddColumnsInAcyncContext(_dataGrid, e.NewItems);
            }
        }

        private static DataGridColumn ToGridColumn(Column column)
        {
            if (column == null) throw new ArgumentNullException();
            
            DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
            Binding binding = new Binding(column.Name);

            if (column.ValueType == typeof(DateTime))
            {
                binding.StringFormat = ConfigurationManager.AppSettings["terminalDateFormat"];
            }

            binding.Converter = _dbNullToDashConverter;
            newDataGridColumn.Binding = binding;
            newDataGridColumn.Header = column.Name;
            return newDataGridColumn;
        }

        private static void AddColumnsInAcyncContext(DataGrid dataGrid, IList newItems)
        {
            if (dataGrid.Dispatcher.CheckAccess())
            {
                AddColumnsInner(dataGrid, newItems);
            }
            else
            {
                dataGrid.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        AddColumnsInner(dataGrid, newItems);
                    }));
            }
        }

        private static void AddColumnsInner(DataGrid dataGrid, IList newItems)
        {
            foreach (Column column in newItems)
            {
                dataGrid.Columns.Add(ToGridColumn(column));
            }
        }
    }
}