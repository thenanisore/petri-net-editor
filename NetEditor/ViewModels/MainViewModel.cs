using System.ComponentModel;
using System.Runtime.CompilerServices;
using PetriNetLib.NetStructure;
using NetEditor.Annotations;

namespace NetEditor.ViewModels
{
    /// <summary>
    /// Connects appearance with the logic of the application.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// A Petri Net, opened in the application.
        /// </summary>
        public NetViewModel NetVM { get; private set; }

        /// <summary>
        /// Initializes a main view-model of the application with a loaded net.
        /// </summary>
        public MainViewModel(Net newNet)
        {
            NetVM = new NetViewModel(newNet);

            IsGrid = true;
            NetVM.GridNodes();
        }

        /// <summary>
        /// Initializes a main view-model of the application with an empty net.
        /// </summary>
        public MainViewModel()
        {
            NetVM = new NetViewModel();
            IsGrid = true;
        }

        private bool _isGrid;

        /// <summary>
        /// On/off grid of the main canvas.
        /// </summary>
        public bool IsGrid
        {
            get { return _isGrid; }
            set
            {
                if (value != _isGrid) {
                    _isGrid = value;
                    if (value) NetVM.GridNodes();
                    else NetVM.UngridNodes();
                    OnPropertyChanged();
                }
            }
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
