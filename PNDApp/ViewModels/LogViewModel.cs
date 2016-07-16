using System.ComponentModel;
using System.Runtime.CompilerServices;
using PNDApp.Annotations;
using PNDApp.Models;

namespace PNDApp.ViewModels
{
    /// <summary>
    /// Represents a view model for log.
    /// </summary>
    public class LogViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Related log.
        /// </summary>
        private readonly Log _log;

        /// <summary>
        /// Constructs a viewmodel and connects it with the given log.
        /// </summary>
        public LogViewModel(Log log)
        {
            _log = log;
        }

        public LogViewModel()
            : this(new Log())
        { }

        /// <summary>
        /// Related log.
        /// </summary>
        public string Log
        {
            get { return _log.GetLog; }
        }

        /// <summary>
        /// Makes a new record in the related log.
        /// </summary>
        public void MakeRecord(string record)
        {
            _log.MakeRecord(record);
            OnPropertyChanged("Log");
        }

        /// <summary>
        /// Clears the related log.
        /// </summary>
        public void ClearLog()
        {
            _log.ClearLog();
            OnPropertyChanged("Log");
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Implementation of the INotifyPropertyChanged to be able to notify views.
        /// </summary>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
