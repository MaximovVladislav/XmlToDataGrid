using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XmlToDataGrid.ViewModels
{
    /// <summary>
    /// Базовый класс для ViewModel
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}