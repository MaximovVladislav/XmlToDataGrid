using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XmlToDataGrid.ViewModels;
using XmlToDataGrid.Views;

namespace XmlToDataGrid
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindowViewModel vm = new MainWindowViewModel();
            Current.MainWindow = new MainWindow {DataContext = vm};
            Current.MainWindow.Show();
        }
    }
}
