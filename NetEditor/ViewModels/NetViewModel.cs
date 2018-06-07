using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using PetriNetLib.NetStructure;
using NetEditor.Annotations;
using NetEditor.Exceptions;

namespace NetEditor.ViewModels
{
    /// <summary>
    /// Represents a Petri net as a diagram object.
    /// </summary>
    public class NetViewModel : INotifyPropertyChanged
    {
        #region NetStructure

        /// <summary>
        /// Related Petri net.
        /// </summary>
        private readonly Net _net;

        /// <summary>
        /// Collection of the nodes (places and transitions).
        /// </summary>
        public ObservableCollection<NodeViewModel> Nodes { get; private set; }

        /// <summary>
        /// Collection of the places.
        /// </summary>
        public ObservableCollection<PlaceViewModel> Places { get; private set; }

        /// <summary>
        /// Collection of the transitions.
        /// </summary>
        public ObservableCollection<TransitionViewModel> Transitions { get; private set; }

        /// <summary>
        /// Collection of the arcs.
        /// </summary>
        public ObservableCollection<ArcViewModel> Arcs { get; private set; }

        #endregion

        #region NetDecomposition

        /// <summary>
        /// Collection of the strongly-connected components.
        /// </summary>
        public ObservableCollection<ComponentViewModel> StronglyConnectedComponents { get; private set; }

        /// <summary>
        /// Collection of the elementary circuits.
        /// </summary>
        public ObservableCollection<CircuitViewModel> ElementaryCircuits { get; private set; }

        /// <summary>
        /// Collection of the handles.
        /// </summary>
        public ObservableCollection<HandleViewModel> Handles { get; private set; }

        #endregion

        /// <summary>
        /// Initializes view-model for the Petri net.
        /// </summary>
        public NetViewModel(Net net) : this()
        {
            const int x = 0;
            const int y = 0;

            _net = net;

            // Creates view-models for all elements of the net.

            foreach (var place in _net.Places) {
                var pvm = new PlaceViewModel(place.Key, x, y, place.Value);
                Places.Add(pvm);
                Nodes.Add(pvm);
            }
            foreach (var transition in _net.Transitions) {
                var tvm = new TransitionViewModel(transition, x, y);
                Transitions.Add(tvm);
                Nodes.Add(tvm);
            }
            foreach (var arc in _net.Arcs) {
                Arcs.Add(new ArcViewModel(arc, GetNodeViewModelFromArc(arc, true), GetNodeViewModelFromArc(arc, false)));
            }
        }

        /// <summary>
        /// Initialize a view-model for a new Petri net.
        /// </summary>
		public NetViewModel()
		{
		    _net = new Net();

            Nodes = new ObservableCollection<NodeViewModel>();
            Places = new ObservableCollection<PlaceViewModel>();
            Transitions = new ObservableCollection<TransitionViewModel>();
            Arcs = new ObservableCollection<ArcViewModel>();

            StronglyConnectedComponents = new ObservableCollection<ComponentViewModel>();
            ElementaryCircuits = new ObservableCollection<CircuitViewModel>();
            Handles = new ObservableCollection<HandleViewModel>();
		}

        private NodeViewModel _selectedNode;

        /// <summary>
        /// Gets or sets selected node.
        /// </summary>
        public NodeViewModel SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode != null)
                    _selectedNode.IsSelected = false;
                _selectedNode = value;
                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = true;
                    IsSelected = true;
                }
                else
                    IsSelected = false;
                OnPropertyChanged();
            }
        }

        private bool _isAnalyzed;

        /// <summary>
        /// If true, already analyzed and avaiable for highlighting and export.
        /// </summary>
        public bool IsAnalyzed
        {
            get { return _isAnalyzed; }
            set
            {
                if (_isAnalyzed != value)
                {
                    _isAnalyzed = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isConnectNode;

        /// <summary>
        /// If true, connect_node button is pressed.
        /// </summary>
        public bool IsConnectNode
        {
            get { return _isConnectNode; }
            set
            {
                if (_isConnectNode != value) {
                    _isConnectNode = value;
                    if (value)
                        IsDisconnectNode = IsNewPlace = IsNewTransition = false;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isDisconnectNode;

        /// <summary>
        /// If true, disconnect_node button is pressed.
        /// </summary>
        public bool IsDisconnectNode
        {
            get { return _isDisconnectNode; }
            set
            {
                if (_isDisconnectNode != value) {
                    _isDisconnectNode = value;
                    if (value)
                        IsConnectNode = IsNewPlace = IsNewTransition = false;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNewPlace;

        /// <summary>
        /// If true, add_place button is pressed.
        /// </summary>
        public bool IsNewPlace
        {
            get { return _isNewPlace; }
            set
            {
                if (_isNewPlace != value) {
                    _isNewPlace = value;
                    if (value) {
                        IsNewTransition = false;
                        if (SelectedNode != null)
                            SelectedNode.IsSelected = false;
                        IsSelected = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNewTransition;

        /// <summary>
        /// If true, add_transition button is pressed.
        /// </summary>
        public bool IsNewTransition
        {
            get { return _isNewTransition; }
            set
            {
                if (_isNewTransition != value) {
                    _isNewTransition = value;
                    if (value) {
                        IsNewPlace = false;
                        if (SelectedNode != null)
                            SelectedNode.IsSelected = false;
                        IsSelected = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelected;

        /// <summary>
        /// If true, some node is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                    _isSelected = value;
                    if (value)
                    {
                        IsSelectedPlace = _selectedNode is PlaceViewModel;
                        IsSelectedTransition = _selectedNode is TransitionViewModel;
                    }
                    else
                    {
                        IsSelectedPlace = IsSelectedTransition =
                            IsConnectNode = IsDisconnectNode = false;
                    }
                    OnPropertyChanged();

            }
        }

        private bool _isSelectedPlace;

        /// <summary>
        /// If true, a place is selected.
        /// </summary>
        public bool IsSelectedPlace
        {
            get { return _isSelectedPlace; }
            set
            {
                if (_isSelectedPlace != value) {
                    _isSelectedPlace = value;
                    if (value)
                        IsSelectedTransition = false;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelectedTransition;

        /// <summary>
        /// If true, a transition is selected.
        /// </summary>
        public bool IsSelectedTransition
        {
            get { return _isSelectedTransition; }
            set
            {
                if (_isSelectedTransition != value) {
                    _isSelectedTransition = value;
                    if (value)
                        IsSelectedPlace = false;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Returns true if the net is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return (Nodes.Count == 0 && Arcs.Count == 0); }
        }

        /// <summary>
        /// Reset all selection.
        /// </summary>
        public void ResetSelection()
        {
            SelectedNode = null;
            IsNewPlace = IsNewTransition = false;
        }

        /// <summary>
        /// Adds a new place to the net.
        /// </summary>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate</param>
        /// <returns>A reference to the created place.</returns>
        public PlaceViewModel AddPlace(double x, double y)
        {
            var pvm = new PlaceViewModel(_net.AddPlace(),
                x - NodeViewModel.GridStep/2, y - 2 * NodeViewModel.GridStep);
            Places.Add(pvm);
            Nodes.Add(pvm);
            return pvm;
        }

        /// <summary>
        /// Adds a new transition to the net.
        /// </summary>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate</param>
        /// <returns>A reference to the created transition.</returns>
        public TransitionViewModel AddTransition(double x, double y)
        {
            const double halfWidth = NodeViewModel.DefaultSize/(2*NodeViewModel.TransitionWidthDivider);
            var tvm = new TransitionViewModel(_net.AddTransition(),
                x - halfWidth, y - 2 * NodeViewModel.GridStep);
            Transitions.Add(tvm);
            Nodes.Add(tvm);
            return tvm;
        }

        /// <summary>
        /// Connects two nodes.
        /// </summary>
        public void ConnectNode(NodeViewModel source, NodeViewModel target)
        {
            if (source is PlaceViewModel && target is TransitionViewModel)
            {
                var arc = _net.AddArcPT(source.Node as Place, target.Node as Transition, returnNullIfExists: true);
                if (arc != null) Arcs.Add(new ArcViewModel(arc, source, target));
            }
            else if (source is TransitionViewModel && target is PlaceViewModel)
            {
                var arc = _net.AddArcTP(source.Node as Transition, target.Node as Place, returnNullIfExists: true);
                if (arc != null) Arcs.Add(new ArcViewModel(arc, source, target));
            }
            else
                throw new ConnectionErrorException();
        }

        /// <summary>
        /// Disconnects two nodes.
        /// </summary>
        public void DisconnectNode(NodeViewModel source, NodeViewModel target)
        {
            var arc = FindArc(source, target);
            if (arc != null)
            {
                _net.RemoveArc(arc.Arc);
                Arcs.Remove(arc);
            }
            else
                throw new DisonnectionErrorException();
        }

        /// <summary>
        /// Deletes selected node from the net.
        /// </summary>
        public void DeleteSelectedNode()
        {
            if (_selectedNode != null)
            {
                var arcs = new List<ArcViewModel>(Arcs);
                var count = arcs.Count;
                for (var i = 0; i < count; i++)
                {
                    if (Arcs[i].SourceNode == _selectedNode || Arcs[i].TargetNode == _selectedNode)
                    {
                        Arcs.Remove(Arcs[i]);
                        i--;
                        count--;
                    }

                }
                if (_selectedNode is PlaceViewModel)
                    Places.Remove(_selectedNode as PlaceViewModel);
                else if (_selectedNode is TransitionViewModel)
                    Transitions.Remove(_selectedNode as TransitionViewModel);

                Nodes.Remove(_selectedNode);
                _net.RemoveNode(_selectedNode.Node);
                SelectedNode = null;
            }
        }

        #region Analysis

        /// <summary>
        /// Analyzes the net and initializes view-models for the output.
        /// </summary>
        public string Analyze(bool badHandles)
        {
            // Find strongly connected components of the net.
            StronglyConnectedComponents.Clear();
            var components = PetriNetLib.Algorithms.KosarajuAlgorithm.GetStronglyConnectedComponents(_net);
            var scc = components.Select(l => l.Select(FindNodeViewModel).ToList()).ToList();
            foreach (var component in scc)
                StronglyConnectedComponents.Add(new ComponentViewModel(component,
                    "SCC" + (StronglyConnectedComponents.Count + 1)));

            // Find all elementary circuits and TP-PT handles of the net.
            ElementaryCircuits.Clear();
            Handles.Clear();
            var circuits = PetriNetLib.Algorithms.JohnsonCircuitsAlgorithm.FindElementaryCircuits(_net);
            var circuitsvm = circuits.Select(c => c.Select(FindNodeViewModel).ToList()).ToList();
            foreach (var circuit in circuitsvm)
            {
                var cvm = new CircuitViewModel(circuit, "CRCT" + (ElementaryCircuits.Count + 1));
                ElementaryCircuits.Add(cvm);
                // Find all handles for this circuit.
                var handles = PetriNetLib.Algorithms.HandlesFindingAlgorithm.FindHandles(cvm.Nodes.Select(n => n.Node).ToList(), badHandles);
                var handlesvm = handles.Select(h => h.Select(FindNodeViewModel).ToList()).ToList();
                foreach (var handle in handlesvm)
                    Handles.Add(new HandleViewModel(handle, cvm, name: "HNDL" + (Handles.Count + 1)));
            }

            // Count numbers of TP and PT handles.
            var TPnum = Handles.Count(h => h.Type == HandleType.TP);
            var PTnum = Handles.Count(h => h.Type == HandleType.PT);

            bool isStructurallyBounded, isRepetitive, isConservative, isConsistent, isStructurallyLive;

            // Write results.
            var results = new StringBuilder();
            results.AppendFormat("Analysis results:{0}", Environment.NewLine);
            results.AppendFormat("Strongly connected components found: {0},{1}", StronglyConnectedComponents.Count, Environment.NewLine);
            results.AppendFormat("Elementary circuits found: {0},{1}", ElementaryCircuits.Count, Environment.NewLine);
            results.AppendFormat("{0}andles found: {1}.{2}", badHandles ? "Bad (PT/TP) h" : "H", badHandles ? TPnum + PTnum : Handles.Count, Environment.NewLine);
            results.Append(Environment.NewLine);

            if (StronglyConnectedComponents.Count == 1)
            {
                isStructurallyBounded = TPnum == 0;
                isRepetitive = PTnum == 0;
                isStructurallyLive = isConservative = isConsistent = ((TPnum == 0) && (PTnum == 0));

                if (TPnum > 0 && PTnum > 0)
                {
                    results.AppendFormat("The net is strongly connected, however there are PT and TP handles.{0}",
                        Environment.NewLine);
                    results.AppendFormat("Any further analysis is not possible.{0}", Environment.NewLine);
                } else if (TPnum > 0)
                {
                    results.AppendFormat("The net is strongly connected and no circuit has a PT-handle.{0}",
                        Environment.NewLine);
                    results.AppendFormat("The net is repetitive.{0}", Environment.NewLine);
                } else if (PTnum > 0)
                {
                    results.AppendFormat("The net is strongly connected and no circuit has a TP-handle.{0}",
                        Environment.NewLine);
                    results.AppendFormat("The net is structurally bounded.{0}", Environment.NewLine);
                }
                else
                {
                    results.AppendFormat("The net is strongly connected and no circuit has a TP-handle nor a PT-handle.{0}",
                        Environment.NewLine);
                    results.AppendFormat("The net is structurally bounded, repetitive, structurally live and covered{0}", Environment.NewLine);
                    results.AppendFormat("by P/T-components (therefore it is conservative and consistent). {0}", Environment.NewLine);
                }
            }
            else
                results.AppendFormat("The net is not strongly connected. Any further analysis is not possible.{0}", Environment.NewLine);

            IsAnalyzed = true;
            return results.ToString();
        }

        /// <summary>
        /// Serialize the information about analysis of the net to XML.
        /// </summary>
        /// <returns>XML-document with the info.</returns>
        public XDocument SerializeAnalysis()
        {
            var xdoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"));

            // Create namespaces.
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace hlpn = "mathtech.ru/npntool/hlpn";
            XNamespace npndiagrams = "http:/mathtech.ru/npntool/npndiagrams";
            XNamespace npnets = "mathtech.ru/npntool/npnets";

            // Root element.
            var root = new XElement(npnets + "NPnetAnalysis");
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", xsi));
            root.Add(new XAttribute(XNamespace.Xmlns + "hlpn", hlpn));
            root.Add(new XAttribute(XNamespace.Xmlns + "npndiagrams", npndiagrams));
            root.Add(new XAttribute(XNamespace.Xmlns + "npnets", npnets));
            root.Add(new XAttribute("id", Guid.NewGuid()));

            // Write SCCs.
            var scComponents = new XElement("scComponents");
            foreach (var stronglyConnectedComponent in StronglyConnectedComponents)
                scComponents.Add(stronglyConnectedComponent.Serialize());

            // Write circuits.
            var elementaryCircuits = new XElement("elementaryCircuits");
            foreach (var circuit in ElementaryCircuits)
                elementaryCircuits.Add(circuit.Serialize());

            // Write handles.
            var handles = new XElement("handles");
            foreach (var handle in Handles)
                handles.Add(handle.Serialize());

            // Add to the document.
            var netHandleAnalysis = new XElement("netHandleAnalysis",
                new XAttribute("netId", _net.Id)
            );

            netHandleAnalysis.Add(scComponents, elementaryCircuits, handles);
            root.Add(netHandleAnalysis);
            xdoc.Add(root);

            return xdoc;
        }

        #endregion

        /// <summary>
        /// Finds a view-model related to the particular node.
        /// </summary>
        /// <returns>NodeViewModel if found, null otherwise.</returns>
        public NodeViewModel FindNodeViewModel(Node node)
        {
            return Nodes.FirstOrDefault(nvm => nvm.Node == node);
        }

        #region Marking

        /// <summary>
        /// Adds one token to a certain place.
        /// </summary>
        /// <param name="p">A View-Model of the place.</param>
        public void AddToken(PlaceViewModel p)
        {
            p.Tokens++;
            _net.AddToken(p.Node as Place);
        }

        /// <summary>
        /// Removes one token from a certain place.
        /// </summary>
        /// <param name="p">A View-Model of the place.</param>
        public void RemoveToken(PlaceViewModel p)
        {
            if (p.Tokens == 0) return;
            p.Tokens--;
            _net.RemoveToken(p.Node as Place);
        }

        /// <summary>
        /// Removes all tokens from a certain place.
        /// </summary>
        /// <param name="p">A View-Model of the place.</param>
        public void RemoveAllTokens(PlaceViewModel p)
        {
            if (p.Tokens == 0) return;
            p.Tokens = 0;
            _net.RemoveAllTokens(p.Node as Place);
        }

        /// <summary>
        /// Removes all tokens from the net entirely.
        /// </summary>
        public void RemoveMarking()
        {
            foreach (var placeViewModel in Places)
            {
                RemoveAllTokens(placeViewModel);
            }
        }

        /// <summary>
        /// Fires a transition.
        /// </summary>
        /// <returns>True, if successful.</returns>
        public bool FireTransition(TransitionViewModel t)
        {
            if (_net.FireTransition(t.Node as Transition))
            {
                UpdateMarking();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Updates the information about marking from the model.
        /// </summary>
        public void UpdateMarking()
        {
            foreach (var placeViewModel in Places)
                placeViewModel.Tokens = _net.Places[placeViewModel.Node as Place];
        }

        #endregion

        /// <summary>
        /// Generates random coordinates for each node.
        /// </summary>
        /// <param name="maxWidth">Canvas width.</param>
        /// <param name="maxHeight">Canvas height.</param>
        public void RandomizeLayout(double minWidth, double minHeight, double maxWidth, double maxHeight)
        {
            var ran = new Random();
            var xStep = (int)((maxWidth - minWidth)/Math.Sqrt(Nodes.Count));
            var yStep = (int)((maxHeight - minHeight) / Math.Sqrt(Nodes.Count));
            var nodes = Nodes.ToList();
            Shuffle(nodes);

            // Increasing the area of generation.
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].X = ran.Next((int)minWidth + (i % (int)Math.Sqrt(nodes.Count)) * xStep,
                    (int)minWidth + (i % (int)Math.Sqrt(nodes.Count) + 1) * xStep);
                nodes[i].Y = ran.Next((int)minHeight + (i / (int)Math.Sqrt(nodes.Count)) * yStep,
                    (int)minHeight + (i / (int)Math.Sqrt(nodes.Count) + 1) * yStep);
            }
        }

        /// <summary>
        /// Shuffles a list.
        /// </summary>
        public static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Centralizes the layout of the node.
        /// </summary>
        /// <param name="maxWidth">Width of the canvas.</param>
        /// <param name="maxHeight">Height of the canvas.</param>
        public void CentralizeLayout(double maxWidth, double maxHeight)
        {
            var centerPoint = new Point(maxWidth / 2.0, maxHeight / 2.0);
            var netArea = GetNetArea(maxWidth, maxHeight, false);
            var diagramCenterPoint = new Point(netArea.X + netArea.Width / 2.0,
                                               netArea.Y + netArea.Height / 2.0);
            var shiftVector = centerPoint - diagramCenterPoint;
            foreach (var node in Nodes) {
                node.X += shiftVector.X;
                node.Y += shiftVector.Y;
            }
        }

        private SubnetViewModel _highlighted;
        private CircuitViewModel _highlightedCircuit;

        /// <summary>
        /// Highlights a subnet of the canvas.
        /// Color depends on the type of that subnet.
        /// </summary>
        /// <typeparam name="T">Type of the subnet.</typeparam>
        /// <param name="subnet"></param>
        /// <param name="circuit">If true, also highlights a circuit related to the handle.</param>
        /// <param name="replace">If false, highlight related circuit.</param>
        public void HighlightSubnet<T>(T subnet, bool circuit=false, bool replace=true) where T : SubnetViewModel
        {
            // Disable old selection.
            if (replace &&_highlighted != null)
            {
                foreach (var node in _highlighted.Nodes)
                    node.DisableHighlight();
                foreach (var arc in Arcs)
                    arc.DisableHighlight();
            }

            if (replace && _highlightedCircuit != null)
                foreach (var node in _highlightedCircuit.Nodes)
                    node.DisableHighlight();

            if (replace)
            {
                if (subnet == _highlighted)
                {
                    _highlighted = _highlightedCircuit = null;
                    return;
                }
                _highlighted = subnet;
            } else
                _highlightedCircuit = subnet as CircuitViewModel;

            // Turn on the nodes.
            foreach (var nodeViewModel in subnet.Nodes)
                HighlightObject(nodeViewModel, subnet);

            // Highlight the related arcs.
            var arcs = new List<ArcViewModel>();
            if (subnet is ComponentViewModel)
                arcs = Arcs.Where(a => subnet.Nodes.Contains(a.SourceNode)
                                       && subnet.Nodes.Contains(a.TargetNode)).ToList();
            else
            {
                for (var i = 0; i < subnet.Nodes.Count - 1; i++)
                    arcs.Add(FindArc(subnet.Nodes[i], subnet.Nodes[i + 1]));
                if (subnet is CircuitViewModel)
                    arcs.Add(FindArc(subnet.Nodes[subnet.Nodes.Count - 1], subnet.Nodes[0]));
            }

            foreach (var arc in arcs)
                HighlightObject(arc, subnet);

            if (subnet is HandleViewModel && circuit)
                HighlightSubnet((subnet as HandleViewModel).Circuit, circuit: false, replace: false);
        }

        public void HighlightObject(IHighlightable obj, SubnetViewModel subnet)
        {
            if (subnet is ComponentViewModel) obj.IsHighlightedSCC = true;
            else if (subnet is CircuitViewModel) obj.IsHighlightedCircuit = true;
            else if (subnet is HandleViewModel) obj.IsHighlightedHandle = true;
        }

        /// <summary>
        /// Returns True is the given nodes are connected.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        /// <returns>True if connected, false otherwise.</returns>
        public bool AreNodesConnected(NodeViewModel a, NodeViewModel b)
        {
            return _net.AreNodesConnected(a.Node, b.Node);
        }

        public void GridNodes()
        {
            foreach (var nodeViewModel in Nodes)
                nodeViewModel.IsGridded = true;
        }

        public void UngridNodes()
        {
            foreach (var nodeViewModel in Nodes)
                nodeViewModel.IsGridded = false;
        }

        /// <summary>
        /// Returns an actual net area.
        /// </summary>
        /// <param name="maxWidth">Width of the canvas.</param>
        /// <param name="maxHeight">Height of the canvas.</param>
        /// <param name="cutOnBorders">If true, points off the screen are not allowed.</param>
        public Int32Rect GetNetArea(double maxWidth, double maxHeight, bool cutOnBorders=true)
        {
            var xMax = (int)Nodes.Max(n => n.Center.X) + NodeViewModel.DefaultSize;
            var yMax = (int)Nodes.Max(n => n.Center.Y) + NodeViewModel.DefaultSize;
            var xMin = (int)Nodes.Min(n => n.Center.X) - NodeViewModel.DefaultSize;
            var yMin = (int)Nodes.Min(n => n.Center.Y) - NodeViewModel.DefaultSize;

            if (cutOnBorders) {
                xMax = xMax > maxWidth ? (int)maxWidth : xMax;
                yMax = yMax > maxHeight ? (int)maxHeight : yMax;
                xMin = xMin < 0 ? 0 : xMin;
                yMin = yMin < 0 ? 0 : yMin;
            }

            return new Int32Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        /// <summary>
        /// Find an arc by its source and target nodes.
        /// </summary>
        public ArcViewModel FindArc(NodeViewModel source, NodeViewModel target)
        {
            return Arcs.FirstOrDefault(a => a.SourceNode == source && a.TargetNode == target);
        }

        /// <summary>
        /// Find node view-model by its related arc.
        /// </summary>
        /// <param name="isSource">if true, the node is the source of that arc.</param>
        private NodeViewModel GetNodeViewModelFromArc(Arc arc, bool isSource)
        {
                return Nodes.FirstOrDefault(n => (isSource ? arc.Source : arc.Target) == n.Node);
        }

        /// <summary>
        /// Find node view-model by its id.
        /// </summary>
        private NodeViewModel GetNodeViewModelFromId(string id)
        {
            return Nodes.FirstOrDefault(n => n.Node.Id == id);
        }

        #region Serialization

        /// <summary>
        /// Serializes the net to XML.
        /// </summary>
        /// <param name="serializeMarking">If true, serialize marking.</param>
        /// <param name="serializeDiagram">If true, serialize coordinates info.</param>
        /// <returns>XML-document with the net.</returns>
        public XDocument Serialize(bool serializeMarking = false, bool serializeDiagram = false)
        {
            var xdoc = _net.Serialize(serializeMarking);
            if (serializeDiagram) {
                var npnets = xdoc.Root.Name.Namespace;
                xdoc.Element(npnets + "NPnetMarked").Element("net").Add(SerializeDiagram());
            }
            return xdoc;
        }

        /// <summary>
        /// Serialize the information about coordinates of the nodes to XML.
        /// </summary>
        /// <returns>netDiagram element.</returns>
        private XElement SerializeDiagram()
        {
            var netDiagram = new XElement("netDiagram", new XAttribute("netId", _net.Id));
            foreach (var nodeViewModel in Nodes) {
                netDiagram.Add(
                    new XElement("node",
                        new XAttribute("id", nodeViewModel.Node.Id),
                        new XAttribute("x", nodeViewModel.X),
                        new XAttribute("y", nodeViewModel.Y)
                    )
                );
            }
            return netDiagram;
        }

        /// <summary>
        /// Get the information about coordinates of the nodes from XML.
        /// </summary>
        public void DeserializeDiagram(XElement netDiagram)
        {
            foreach (var node in netDiagram.Elements("node")) {
                var nvm = GetNodeViewModelFromId(node.Attribute("id").Value);
                if (nvm != null) {
                    double x;
                    double y;
                    if (!double.TryParse(node.Attribute("x").Value, out x)
                        || !double.TryParse(node.Attribute("y").Value, out y))
                        throw new NetDiagramXmlException();
                    nvm.X = x;
                    nvm.Y = y;
                } else
                    throw new NetDiagramXmlException();
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
