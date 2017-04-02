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
    public class ColumnBehavior
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns",
            typeof(ObservableCollection<Column>), typeof(ColumnBehavior),
            new UIPropertyMetadata(null, ColumnsPropertyChanged));

        private static DataGrid _dataGrid;
        private static ColumnToDataGridColumnConverter _converter = new ColumnToDataGridColumnConverter();

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

            foreach (var column in columns)
            {
                AddColumnsInAcyncContext(_dataGrid, (IList) e.NewValue/*ToGridColumn(column)*/);
            }

            columns.CollectionChanged += Columns_CollectionChanged;
        }

        private static void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {

                foreach (Column column in e.NewItems)
                {
                    AddColumnsInAcyncContext(_dataGrid, e.NewItems);
                }
            }
            //else if (e.Action == NotifyCollectionChangedAction.Move)
            //{
            //    _dataGrid.Columns.Move(e.OldStartingIndex, e.NewStartingIndex);
            //}
            //else if (e.Action == NotifyCollectionChangedAction.Remove)
            //{
            //    foreach (DataGridColumn column in e.NewItems)
            //    {
            //        _dataGrid.Columns.Remove(column);
            //    }
            //}
            //else if (e.Action == NotifyCollectionChangedAction.Replace)
            //{
            //    _dataGrid.Columns[e.NewStartingIndex] = e.NewItems[0] as DataGridColumn;
            //}
            //else if (e.Action == NotifyCollectionChangedAction.Reset)
            //{
            //    _dataGrid.Columns.Clear();
            //    foreach (DataGridColumn column in e.NewItems)
            //    {
            //        _dataGrid.Columns.Add(column);
            //    }
            //}
        }

        private static DataGridColumn ToGridColumn(Column column)
        {
            if (column == null) throw new ArgumentNullException();

            if (column.ValueType == typeof(DateTime))
            {
                DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
                Binding binding = new Binding(column.Name);
                binding.StringFormat = ConfigurationManager.AppSettings["terminalDateFormat"];
                newDataGridColumn.Binding = binding;
                newDataGridColumn.Header = column.Name;
                return newDataGridColumn;
            }
            else
            {
                DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
                newDataGridColumn.Binding = new Binding(column.Name);
                newDataGridColumn.Header = column.Name;
                return newDataGridColumn;
            }
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

                //dataGrid.Dispatcher.Invoke(DispatcherPriority.Normal,
                //    new Action<object>(AddColumnInner), new object[] {column});
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