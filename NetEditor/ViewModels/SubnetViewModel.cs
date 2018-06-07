using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using NetEditor.Annotations;

namespace NetEditor.ViewModels
{
    public abstract class SubnetViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Nodes of subnet.
        /// </summary>
        public ObservableCollection<NodeViewModel> Nodes { get; protected set; }

        /// <summary>
        /// Initialize a new subnet based on the list of nodes.
        /// </summary>
        protected SubnetViewModel(List<NodeViewModel> nodes, string name = "", string id = "")
        {
            Nodes = new ObservableCollection<NodeViewModel>(nodes);
            if (name != "") Name = name;
            Id = id != "" ? id : Guid.NewGuid().ToString();
        }

        protected string _name;

        /// <summary>
        /// Gets or sets the name of the subnet.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                    _name = value;
                OnPropertyChanged();
            }
        }

        protected string _id;

        /// <summary>
        /// Gets of sets the id of the subnet.
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                    _id = value;
                OnPropertyChanged();
            }
        }

        public abstract XElement Serialize();

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
