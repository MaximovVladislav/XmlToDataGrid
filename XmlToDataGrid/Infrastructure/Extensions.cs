using System.Configuration;

namespace XmlToDataGrid.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows;

    public static class Extensions
    {
        /// <summary>
        /// Добавляет список объектов <see cref="T"/> в коллекцию в UI потоке без повторений
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="collection">Коллекция</param>
        /// <param name="items">Список добавляемыхобъектов <see cref="T"/></param>
        public static void AddRangeUniqueOnUI<T>(this ObservableCollection<T> collection, IList<T> items)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => collection.AddRangeUnique(items)));
        }

        /// <summary>
        /// Добавляет список объектов <see cref="T"/> в коллекцию без повторений
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="collection">Коллекция</param>
        /// <param name="items">Список добавляемыхобъектов <see cref="T"/></param>
        public static void AddRangeUnique<T>(this ObservableCollection<T> collection, IList<T> items)
        {
            foreach (T item in items)
            {
                if (!collection.Contains(item))
                    collection.Add(item);
            }
        }

        /// <summary>
        /// Добавляет список <see cref="DataRow"/> в коллекцию <see cref="DataRowCollection"/> в UI потоке без повторений
        /// </summary>
        /// <param name="collection">Коллекция строк <see cref="DataRowCollection"/></param>
        /// <param name="items">Список строк <see cref="DataRow"/></param>
        public static void AddRangeUniqueOnUI(this DataRowCollection collection, IList<DataRow> items)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => collection.AddRangeUnique(items)));
        }

        /// <summary>
        /// Добавляет список <see cref="DataRow"/> в коллекцию <see cref="DataRowCollection"/> без повторений
        /// </summary>
        /// <param name="collection">Коллекция строк <see cref="DataRowCollection"/></param>
        /// <param name="items">Список строк <see cref="DataRow"/></param>
        public static void AddRangeUnique(this DataRowCollection collection, IList<DataRow> items)
        {
            foreach (DataRow item in items)
            {
                try
                {
                    if (!collection.Contains(item[ConfigurationManager.AppSettings["keyAttribute"]]))
                        collection.Add(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}