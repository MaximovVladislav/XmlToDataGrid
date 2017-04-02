using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Win32;
using XmlToDataGrid.Infrastructure;
using XmlToDataGrid.Models;

namespace XmlToDataGrid.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ICommand _updateCommand;
        private DateTime _beginDate;
        private DateTime _endDate;
        private string _serverState;
        private TimeSpan _upTime;

        //private string _dataDateFormat = "dd.MM.yyyy - HH:mm:ss";
        //private string _terminalDateFormat = "dd.MM.yyyy.HH.mm.ss";

        public MainWindowViewModel()
        {
            Data = new DataTable();
            //Columns = new ObservableCollection<DataGridColumn>();
            Columns = new ObservableCollection<Column>();
        }

        public string Title => "Датчики";

        public DateTime BeginDate
        {
            get { return _beginDate; }
            set
            {
                _beginDate = value; 
                RaisePropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value; 
                RaisePropertyChanged();
            }
        }

        public string ServerState
        {
            get { return _serverState; }
            set
            {
                _serverState = value; 
                RaisePropertyChanged();
            }
        }

        public TimeSpan UpTime
        {
            get { return _upTime; }
            set
            {
                _upTime = value; 
                RaisePropertyChanged();
            }
        }

        public DataTable Data { get; set; }

        //public ObservableCollection<DataGridColumn> Columns { get; set; }

        public ObservableCollection<Column> Columns { get; set; }

        public ICommand UpdateCommand
        {
            get { return _updateCommand ?? (_updateCommand = new RelayCommand(async action => await UpdateAsync())); }
        }
        

        private async Task UpdateAsync()
        {
            try
            {
                await Task.Run(() => UpdateInner());
            }
            catch (Exception ex)
            {
                //TODO: сделать логирование, а пока так
                MessageBox.Show(ex.Message);
            }
            
        }

        private void UpdateInner()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.DefaultExt = "xml";

            if (openFileDialog.ShowDialog() == true)
            {
                var doc = XDocument.Load(openFileDialog.FileName);

                XElement dataNode = (XElement)doc.FirstNode;
                //XAttribute beginDateAttribute = 

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

                AddDataRow((XElement)terminalsNode.FirstNode);
            }
        }

        void AddDataRow(XElement terminalNode)
        {
            if (terminalNode == null) return;

            var attributes = terminalNode.Attributes();

            if (!attributes.Any()) return;

            DataRow newRow = Data.NewRow();

            foreach (XAttribute attribute in attributes)
            {
                string attrName = attribute.Name.ToString();
                string attrValue = attribute.Value;

                string terminalDateFormat = ConfigurationManager.AppSettings["terminalDateFormat"];

                DateTime timeAtrrValue;
                bool attrValueIsTime = DateTime.TryParseExact(attrValue, terminalDateFormat,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out timeAtrrValue);

                if (!Data.Columns.Contains(attrName))
                {
                    if (!attrValueIsTime)
                    {
                        DataColumn newColumn = new DataColumn(attrName, typeof(string));
                        Data.Columns.Add(newColumn);

                        Columns.Add(new Column {Name = attrName, ValueType = typeof(string)});

                        //DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
                        //newDataGridColumn.Binding = new Binding(attrName);
                        //newDataGridColumn.Header = attrName;
                        //Columns.Add(newDataGridColumn);
                    }
                    else
                    {
                        DataColumn newColumn = new DataColumn(attrName, typeof(DateTime));
                        newColumn.AllowDBNull = true;
                        newColumn.Caption = attrName;
                        Data.Columns.Add(newColumn);

                        Columns.Add(new Column {Name = attrName, ValueType = typeof(DateTime)});

                        //DataGridTextColumn newDataGridColumn = new DataGridTextColumn();
                        //Binding binding = new Binding(attrName);
                        //binding.StringFormat = ConfigurationManager.AppSettings["terminalDateFormat"];
                        //newDataGridColumn.Binding = binding;
                        //newDataGridColumn.Header = attrName;
                        //Columns.Add(newDataGridColumn);
                    }
                }

                if (Data.Columns.Contains(attrName))
                {
                    if (!attrValueIsTime)
                    {
                        newRow[attrName] = attrValue;
                    }
                    else
                    {
                        //DateTime connectionTime = DateTime.ParseExact(attrValue, _terminalDateFormat,
                        //    CultureInfo.InvariantCulture);

                        if (timeAtrrValue != default(DateTime))
                        {
                            newRow[attrName] = timeAtrrValue;
                        }
                        else
                        {
                            newRow[attrName] = DBNull.Value;
                        }
                    }
                }
            }

            //terminalNode.Elements()

            Data.Rows.Add(newRow);

            AddDataRow((XElement) terminalNode.NextNode);
        }

    }
}