using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PetriNetLib.NetStructure;
using NetEditor.Annotations;

namespace NetEditor.ViewModels
{
    /// <summary>
    /// Represents an arc as a diagram object.
    /// </summary>
    public class ArcViewModel : INotifyPropertyChanged, IHighlightable
    {
        /// <summary>
        /// Related arc.
        /// </summary>
        public Arc Arc { get; private set; }

        /// <summary>
        /// A reference to the source node (view-model).
        /// </summary>
        public NodeViewModel SourceNode { get; set; }

        /// <summary>
        /// A reference to the target node (view-model).
        /// </summary>
        public NodeViewModel TargetNode { get; set; }

        /// <summary>
        /// Initializes an arc by its source and target.
        /// </summary>
        public ArcViewModel(Arc relArc, NodeViewModel source, NodeViewModel target)
        {
            Arc = relArc;
            SourceNode = source;
            TargetNode = target;

            // Monitor the change of the related point coordinates.
            SourceNode.PropertyChanged += (sender, args) => UpdateArc(args);
            TargetNode.PropertyChanged += (sender, args) => UpdateArc(args);

            SourceBorderPoint = GetBorderPoint(SourceNode, TargetNode);
            TargetBorderPoint = GetBorderPoint(TargetNode, SourceNode);
        }

        #region Drawing

        private Point _sourceBorderPoint;

        /// <summary>
        /// Gets or sets source's border point on the line between nodes.
        /// </summary>
        public Point SourceBorderPoint
        {
            get { return _sourceBorderPoint; }
            set
            {
                if (value != _sourceBorderPoint)
                {
                    _sourceBorderPoint = value;
                    OnPropertyChanged();
                }
            }
        }

        private Point _targetBorderPoint;

        /// <summary>
        /// Gets or sets target's border point on the line between nodes.
        /// </summary>
        public Point TargetBorderPoint
        {
            get { return _targetBorderPoint; }
            set
            {
                if (value != _targetBorderPoint) {
                    _targetBorderPoint = value;
                    TargetArrowPoint = GetArrowPoint();
                    OnPropertyChanged();
                }
            }
        }

        private Point _targetArrowPoint;

        /// <summary>
        /// Calculates a point of the end of the arrow against the target node.
        /// </summary>
        public Point TargetArrowPoint
        {
            get { return _targetArrowPoint; }
            set
            {
                if (value != _targetArrowPoint) {
                    _targetArrowPoint = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Redraw arrows if nodes were moved.
        /// </summary>
        private void UpdateArc(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "X" || args.PropertyName == "Y") {
                SourceBorderPoint = GetBorderPoint(SourceNode, TargetNode);
                TargetBorderPoint = GetBorderPoint(TargetNode, SourceNode);
            }
        }



        private const double ArrowSize = 20;

        /// <summary>
        /// Calculates the point to draw an arrow correctly.
        /// </summary>
        private Point GetArrowPoint()
        {
            double dx = Math.Abs(TargetBorderPoint.X - SourceBorderPoint.X),
                   dy = Math.Abs(TargetBorderPoint.Y - SourceBorderPoint.Y);
            double xSign = Math.Sign(TargetBorderPoint.X - SourceBorderPoint.X),
                   ySign = Math.Sign(TargetBorderPoint.Y - SourceBorderPoint.Y);
            double len = Math.Sqrt(dx * dx + dy * dy);

            double x = TargetBorderPoint.X + xSign * (ArrowSize*dx)/len,
                   y = TargetBorderPoint.Y + ySign * (ArrowSize * dy) / len;

            return new Point(x, y);
        }

        /// <summary>
        /// Uses some math to calculate the point on the intersection of the
        /// node's border and the line between two nodes.
        /// </summary>
        private static Point GetBorderPoint(NodeViewModel source, NodeViewModel target)
        {
            double x = source.Center.X,
                   y = source.Center.Y;
            double dx = Math.Abs(target.Center.X - source.Center.X),
                   dy = Math.Abs(target.Center.Y - source.Center.Y);
            double xSign = Math.Sign(target.Center.X - source.Center.X),
                   ySign = Math.Sign(target.Center.Y - source.Center.Y);
            double len = Math.Sqrt(dx*dx + dy*dy);
            if (len > source.Size * 0.5) {
                if (source is PlaceViewModel) {
                    x += xSign * (source.Size * dx * 0.5) / len;
                    y += ySign * (source.Size * dy * 0.5) / len;
                } else if (source is TransitionViewModel) {
                    if (dy / dx < source.Size / source.Width) {
                        x += xSign * source.Width / 2;
                        y += ySign * (dy * source.Width * 0.5) / dx;
                    } else {
                        x += xSign * (dx * source.Size * 0.5) / dy;
                        y += ySign * source.Size / 2;
                    }
                }
            }
            return new Point(x, y);
        }

        #endregion

        #region IHighlightable

        /// <summary>
        /// Disables highlight of the arc.
        /// </summary>
        public void DisableHighlight()
        {
            IsHighlightedSCC = IsHighlightedCircuit = IsHighlightedHandle = false;
        }

        /// <summary>
        /// True if arc is highlighted in a strongly related component.
        /// </summary>
        private bool _isHighlightedSCC;
        public bool IsHighlightedSCC
        {
            get { return _isHighlightedSCC; }
            set
            {
                if (value != _isHighlightedSCC) {
                    _isHighlightedSCC = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True if arc is highlighted in a circuit.
        /// </summary>
        private bool _isHighlightedCircuit;
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

        /// <summary>
        /// True if arc is highlighted in a handle.
        /// </summary>
        private bool _isHighlightedHandle;
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

        /// <summary>
        /// Implementation of the INotifyPropertyChanged to be able to notify views.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
