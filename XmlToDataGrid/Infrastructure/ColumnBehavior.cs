using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using XmlToDataGrid.Converters;

namespace XmlToDataGrid.Infrastructure
{
    /// <summary>
    /// Класс, содержащий прикрепляющуюся коллекцию столбцов <see cref="DataColumn"/> и определяющий ее поведение 
    /// </summary>
    public class ColumnBehavior
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns",
            typeof(DataColumnCollection), typeof(ColumnBehavior),
            new UIPropertyMetadata(null, ColumnsPropertyChanged));

        private static DataGrid _dataGrid;
        private static readonly DBNullToDashConverter _dbNullToDashConverter = new DBNullToDashConverter();

        public static DataColumnCollection GetColumns(DependencyObject element)
        {
            return (DataColumnCollection)element.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject element, DataColumnCollection value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        private static void ColumnsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            _dataGrid = sender as DataGrid;
            if (_dataGrid == null) return;
            _dataGrid.Columns.Clear();

            //ObservableCollection<Column> columns = e.NewValue as ObservableCollection<Column>;

            DataColumnCollection columns = e.NewValue as DataColumnCollection;

            if (columns == null) return;

            ICollection newColumns = (ICollection) e.NewValue;

            if (newColumns.Count > 0)
            {
                AddColumnsInAcyncContext(_dataGrid, newColumns);
            }

            //columns.CollectionChanged += Columns_CollectionChanged;
            columns.CollectionChanged += Columns_CollectionChanged;
        }

        private static void Columns_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Add)
            {
                AddColumnInAcyncContext(_dataGrid, e.Element);
            }
        }

        //private static void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add)
        //    {
        //        AddColumnsInAcyncContext(_dataGrid, e.NewItems);
        //    }
        //}

        private static DataGridColumn ToGridColumn(DataColumn column)
        {
            if (column == null) throw new ArgumentNullException();
            
            DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
            Binding binding = new Binding(column.ColumnName);

            if (column.DataType == typeof(DateTime))
            {
                binding.StringFormat = ConfigurationManager.AppSettings["terminalDateFormat"];
            }

            binding.Converter = _dbNullToDashConverter;
            newDataGridColumn.Binding = binding;
            newDataGridColumn.Header = column.ColumnName;
            return newDataGridColumn;
        }

        private static void AddColumnInAcyncContext(DataGrid dataGrid, object column)
        {
            if (dataGrid.Dispatcher.CheckAccess())
            {
                dataGrid.Columns.Add(ToGridColumn((DataColumn)column));
            }
            else
            {
                dataGrid.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        dataGrid.Columns.Add(ToGridColumn((DataColumn)column));
                    }));
            }
        }

        private static void AddColumnsInAcyncContext(DataGrid dataGrid, ICollection newItems)
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

        private static void AddColumnsInner(DataGrid dataGrid, ICollection newItems)
        {
            foreach (DataColumn column in newItems)
            {
                dataGrid.Columns.Add(ToGridColumn(column));
            }
        }
    }
}