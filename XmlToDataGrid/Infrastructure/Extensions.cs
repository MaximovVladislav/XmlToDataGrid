namespace XmlToDataGrid.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows;
    
    public static class Extensions
    {
        public static void AddRangeOnUI<T>(this ObservableCollection<T> collection, IList<T> items)
        {
            Application.Current.Dispatcher.BeginInvoke((Action) (() => collection.AddRange(items)));
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IList<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddRangeOnUI(this DataRowCollection collection, IList<DataRow> items)
        {
            Application.Current.Dispatcher.BeginInvoke((Action) (() => collection.AddRange(items)));
        }

        public static void AddRange(this DataRowCollection collection, IList<DataRow> items)
        {
            foreach (DataRow item in items)
            {
                collection.Add(item);
            }
        }
    }
}
