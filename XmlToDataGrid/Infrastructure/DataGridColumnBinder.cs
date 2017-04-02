using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace XmlToDataGrid.Infrastructure
{
    public class DataGridColumnBinder
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns",
            typeof(ObservableCollection<DataGridColumn>), typeof(DataGridColumnBinder),
            new UIPropertyMetadata(null, ColumnsPropertyChanged));

        private static DataGrid _dataGrid;

        public static ObservableCollection<DataGridColumn> GetColumns(DependencyObject element)
        {
            return (ObservableCollection<DataGridColumn>) element.GetValue(ColumnsProperty);
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

            ObservableCollection<DataGridColumn> columns = e.NewValue as ObservableCollection<DataGridColumn>;

            if (columns == null) return;

            foreach (var column in columns)
            {
                _dataGrid.Columns.Add(column);
            }

            columns.CollectionChanged += Columns_CollectionChanged;
        }

        private static void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (DataGridColumn column in e.NewItems)
                {
                    _dataGrid.Columns.Add(column);
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Move)
            {
                _dataGrid.Columns.Move(e.OldStartingIndex, e.NewStartingIndex);
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (DataGridColumn column in e.NewItems)
                {
                    _dataGrid.Columns.Remove(column);
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Replace)
            {
                _dataGrid.Columns[e.NewStartingIndex] = e.NewItems[0] as DataGridColumn;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _dataGrid.Columns.Clear();
                foreach (DataGridColumn column in e.NewItems)
                {
                    _dataGrid.Columns.Add(column);
                }
            }
        }
    }
}