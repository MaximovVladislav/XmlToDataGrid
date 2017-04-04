using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Win32;
using XmlToDataGrid.Infrastructure;

namespace XmlToDataGrid.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ICommand _loadCommand;
        private DateTime _beginDate;
        private DateTime _endDate;
        private string _serverState;
        private TimeSpan _upTime;
        
        public MainWindowViewModel()
        {
            Table = new DataTable();
        }

        public string Title => "Терминалы";

        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime BeginDate
        {
            get { return _beginDate; }
            set
            {
                _beginDate = value; 
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value; 
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Состояние сервера
        /// </summary>
        public string ServerState
        {
            get { return _serverState; }
            set
            {
                _serverState = value; 
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Время работы сервера
        /// </summary>
        public TimeSpan UpTime
        {
            get { return _upTime; }
            set
            {
                _upTime = value; 
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Таблица терминалов
        /// </summary>
        public DataTable Table { get; set; }

        public ICommand LoadCommand
        {
            get { return _loadCommand ?? (_loadCommand = new RelayCommand(async action => await LoadAsync())); }
        }
        

        private async Task LoadAsync()
        {
            try
            {
                await Task.Run(() => Load());
            }
            catch (Exception ex)
            {
                // Простейший вариант обработки исключений
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Load()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                // Имитации длительной работы, чтобы увидеть асинхронность
                //Thread.Sleep(5000);
                
                XDocument doc = XDocument.Load(openFileDialog.FileName);

                XElement dataNode = (XElement)doc.FirstNode;
                
                XAttribute tempAttribute = dataNode.Attribute("begindate");

                string dataDateFormat = ConfigurationManager.AppSettings["dataDateFormat"];

                if (tempAttribute != null)
                {
                    BeginDate = DateTime.ParseExact(tempAttribute.Value, dataDateFormat,
                        CultureInfo.InvariantCulture);
                }

                tempAttribute = dataNode.Attribute("enddate");

                if (tempAttribute != null)
                {
                    EndDate = DateTime.ParseExact(tempAttribute.Value, dataDateFormat,
                        CultureInfo.InvariantCulture);
                }

                XElement serverNode = (XElement)dataNode.FirstNode;

                ServerState = serverNode.Attribute("state")?.Value;

                string timeUpTemp = serverNode.Attribute("UpTime")?.Value;

                if (!string.IsNullOrEmpty(timeUpTemp))
                {
                    string[] timeUpStrings = timeUpTemp.Split(':');

                    if (timeUpStrings.Length == 3)
                    {
                        UpTime = new TimeSpan(int.Parse(timeUpStrings[0]), int.Parse(timeUpStrings[1]),
                            int.Parse(timeUpStrings[2]));
                    }
                }

                XElement terminalsNode = (XElement)serverNode.NextNode;

                if (terminalsNode == null) return;

                List<DataRow> rowList = new List<DataRow>();
                
                // Получаем строки для таблицы
                foreach(XElement terminalNode in terminalsNode.Elements())
                {
                    DataRow dataRow;
                    if (TryCreateDataRow(terminalNode, out dataRow))
                    {
                        rowList.Add(dataRow);
                    }
                }
                
                //Добавляем новые строки в UI потоке
                Table.Rows.AddRangeUniqueOnUI(rowList);
            }
        }

        bool TryCreateDataRow(XElement terminalNode, out DataRow dataRow)
        {
            dataRow = null;
            if (terminalNode == null) throw new ArgumentNullException(nameof(terminalNode));

            var attributes = terminalNode.Attributes();
            var sensorNodes = terminalNode.Elements();

            if (!attributes.Any() && !sensorNodes.Any()) return false;

            dataRow = Table.NewRow();

            // Цикл по атрибутам
            foreach (XAttribute attribute in attributes)
            {
                string attrName = attribute.Name.ToString();
                string attrValue = attribute.Value;

                string terminalDateFormat = ConfigurationManager.AppSettings["terminalDateFormat"];

                DateTime timeAtrrValue;
                bool attrValueIsTime = DateTime.TryParseExact(attrValue, terminalDateFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out timeAtrrValue);

                if (!Table.Columns.Contains(attrName))
                {
                    if (!attrValueIsTime)
                    {
                        DataColumn newColumn = new DataColumn(attrName, typeof(string));
                        Table.Columns.Add(newColumn);

                        if (attrName == ConfigurationManager.AppSettings["keyAttribute"])
                        {
                            Table.PrimaryKey = new[] {newColumn};
                        }
                    }
                    else
                    {
                        DataColumn newColumn = new DataColumn(attrName, typeof(DateTime));
                        newColumn.AllowDBNull = true;
                        newColumn.Caption = attrName;
                        Table.Columns.Add(newColumn);
                    }
                }

                if (!attrValueIsTime)
                {
                    dataRow[attrName] = attrValue;
                }
                else
                {
                    if (timeAtrrValue != default(DateTime))
                    {
                        dataRow[attrName] = timeAtrrValue;
                    }
                    else
                    {
                        dataRow[attrName] = DBNull.Value;
                    }
                }
            }

            //Цикл по датчикам
            foreach (XElement sensorNode in sensorNodes)
            {
                string type = sensorNode.Attribute("type")?.Value;

                if (type == null) continue;

                string stringValue = sensorNode.Attribute("value")?.Value;

                Type valueType = null;
                int intValue = 0;
                double doubleValue = 0;

                if (int.TryParse(stringValue, out intValue))
                {
                    valueType = typeof(int);
                }
                else if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
                {
                    valueType = typeof(double);
                }
                else
                {
                    valueType = typeof(string);
                }

                if (!Table.Columns.Contains(type))
                {
                    DataColumn newColumn = new DataColumn(type, valueType);
                    Table.Columns.Add(newColumn);
                }

                if (valueType == typeof(int))
                {
                    dataRow[type] = intValue;
                }
                else if (valueType == typeof(double))
                {
                    dataRow[type] = doubleValue;
                }
                else
                {
                    dataRow[type] = stringValue;
                }
            }

            return true;
        }
    }
}
