using System;

namespace XmlToDataGrid.Models
{
    /// <summary>
    /// Класс, инкапсулирующий столбцы таблицы
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Тип
        /// </summary>
        public Type ValueType { get; set; }
    }
}