using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PetriNetLib.NetStructure;
using PNDApp.Annotations;

namespace PNDApp.ViewModels
{
    /// <summary>
    /// Represents a graph node as a diagram object.
    /// </summary>
    public class NodeViewModel : INotifyPropertyChanged, IHighlightable
    {
        public const int DefaultSize = 40;
        public const int TransitionWidthDivider = 2;
        public const int GridStep = DefaultSize;

        /// <summary>
        /// Related node.
        /// </summary>
        public Node Node { get; protected set; }

        public NodeViewModel(Node relatedNode, double x, double y)
        {
            Node = relatedNode;
            Size = DefaultSize;
            IsSelected = false;
            _x = x; _y = y;
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name
        {
            get { return Node.Name; }
            set
            {
                if (value != Node.Name)
                {
                    if (value.Length > 4) value = value.Substring(0, 4);
                    Node.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isGridded;

        /// <summary>
        /// If true, moves right on the cells of the grid.
        /// </summary>
        public bool IsGridded
        {
            get { return _isGridded; }
            set
            {
                _isGridded = value;
                if (value) {
                    X = _x;
                    Y = _y;
                }
            }
        }

        private bool _isSelected;

        /// <summary>
        /// Is selected if true.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _x;

        /// <summary>
        /// The X-coordinate of the node.
        /// </summary>
        public double X
        {
            get { return _x; }
            set {
                _x = IsGridded ? Math.Round(value/GridStep)*GridStep + (Size - Width)/2 : value;
                Center = new Point(_x + Width / 2, Center.Y);
                OnPropertyChanged();
            }
        }

        private double _y;

        /// <summary>
        /// The Y-coordinate of the node.
        /// </summary>
        public double Y
        {
            get { return _y; }
            set
            {
                _y = IsGridded ? Math.Round(value/GridStep)*GridStep : value;          
                Center = new Point(Center.X, _y + Size / 2);
                OnPropertyChanged();
            }
        }

        private Point _center;

        /// <summary>
        /// Center point of the node.
        /// </summary>
        public Point Center
        {
            get { return _center; }
            set
            {
                if (_center != value) {
                    _center = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sets the center point of the node in the centerPoint.
        /// </summary>
        public void SetCenter(Point centerPoint)
        {
            X = centerPoint.X - Width/2.0;
            Y = centerPoint.Y - Size/2.0;
        }

        private int _size;

        /// <summary>
        /// Height (diameter) of the node.
        /// </summary>
        public int Size
        {
            get { return _size; }
            private set
            {
                if (_size != value) {
                    _size = value;
                    Width = _size / (this is TransitionViewModel ? TransitionWidthDivider : 1);
                    OnPropertyChanged();
                }
            }
        }

        private int _width;

        /// <summary>
        /// Width of the node.
        /// </summary>
        public int Width
        {
            get { return _width; }
            protected set
            {
                if (_width != value) {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        #region IHighlightable

        /// <summary>
        /// Disables highlight of the node.
        /// </summary>
        public void DisableHighlight()
        {
            IsHighlightedSCC = IsHighlightedCircuit = IsHighlightedHandle = false;
        }

        private bool _isHighlightedSCC;

        /// <summary>
        /// True if the node is highlighted in a strongly related component.
        /// </summary>
        public bool IsHighlightedSCC
        {
            get { return _isHighlightedSCC; }
            set
            {
                if (value != _isHighlightedSCC)
                {
                    _isHighlightedSCC = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHighlightedCircuit;

        /// <summary>
        /// True if the node is highlighted in a circuit.
        /// </summary>
        public bool IsHighlightedCircuit
        {
            get { return _isHighlightedCircuit; }
            set
            {
                if (value != _isHighlightedCircuit) {
                    _isHighlightedCircuit = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHighlightedHandle;

        /// <summary>
        /// True if the node is highlighted in a handle.
        /// </summary>
        public bool IsHighlightedHandle
        {
            get { return _isHighlightedHandle; }
            set
            {
                if (value != _isHighlightedHandle) {
                    _isHighlightedHandle = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

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
