using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using PetriNetLib.Exceptions;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Represents a Petri Net.
    /// </summary>
    public class Net : IDeepCloneable<Net>
    {
        /// <summary>
        /// The ID of the net.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the net.
        /// </summary>
        public string Name { get; set; }

        protected Dictionary<Place, uint> _places = new Dictionary<Place, uint>();
        protected List<Transition> _transitions = new List<Transition>();
        protected List<Arc> _arcs = new List<Arc>();

        /// <summary>
        /// Returns a read-only dictionary with places and marking of the net.
        /// </summary>
        public ReadOnlyDictionary<Place, uint> Places
        {
            get { return new ReadOnlyDictionary<Place, uint> (_places); }
        }

        /// <summary>
        /// Returns a read-only list of transitions.
        /// </summary>
        public IReadOnlyList<Transition> Transitions
        {
            get { return _transitions.AsReadOnly(); }
        }

        /// <summary>
        /// Returns a read-only list of arcs.
        /// </summary>
        public IReadOnlyList<Arc> Arcs
        {
            get { return _arcs.AsReadOnly(); }
        }

        /// <summary>
        /// Constructs an empty net. Random ID if not specified.
        /// </summary>
        public Net(string id="")
        {
            Id = id == "" ? Guid.NewGuid().ToString() : id;
        }

        /// <summary>
        /// Constructs a net from netsystem xml-element.
        /// </summary>
        public Net(XElement netElement, string name="", bool getMarking=false)
        {
            // Get net system.
            try
            {
                var netSystem = netElement.Element("netSystem");
                Id = netSystem.Attribute("id").Value;
                Name = name;
                XPathNavigator root = netSystem.CreateNavigator();
                XNamespace ns = root.GetNamespace("xsi");

            // Add each node to the net.
            foreach (XElement node in netSystem.Elements("nodes"))
                BuildNode(node, ns);

            // Connect the nodes with arcs.
            foreach (XElement arc in netSystem.Elements("arcs"))
                BuildArc(arc, ns);
            } 
            catch (XmlException) {
                throw new NetSystemXmlException();
            }

            if (getMarking)
                try
                {
                    // Get marking.
                    var netMarking = netElement.Element("netMarking");
                    if (netMarking != null)
                        foreach (var token in netMarking.Elements("token"))
                            GetToken(token);
                }
                catch (XmlException)
                {
                    throw new NetMarkingXmlException();
                }
        }

        /// <summary>
        /// Returns a list of nodes of the net.
        /// </summary>
        public IReadOnlyList<Node> GetNodeList
        {
            get
            {
                var nodeList = new List<Node>(_places.Keys);
                nodeList.AddRange(_transitions);
                return nodeList;
            }
        }

        /// <summary>
        /// Finds and returns a node by id.
        /// </summary>
        public Node FindNodeById(string id)
        {
            return GetNodeList.FirstOrDefault(n => n.Id == id);
        }

        /// <summary>
        /// Adds a place to the net.
        /// </summary>
        /// <returns>A pointer to the added place.</returns>
        public Place AddPlace(string name="", string id="")
        {
            if (name == "")
                name = "p" + (Places.Count + 1);
            var p = new Place(name, id);
            _places.Add(p, 0);
            return p;
        }

        /// <summary>
        /// Adds a transition to the net.
        /// </summary>
        /// <returns>A pointer to the added transition.</returns>
        public Transition AddTransition(string name="", string id="")
        {
            if (name == "")
                name = "t" + (Transitions.Count + 1);
            var t = new Transition(name, id);
            _transitions.Add(t);
            return t;
        }

        /// <summary>
        /// Adds a transition-place arc.
        /// </summary>
        public Arc AddArcTP(Transition source, Place target, string id = "",
            bool returnNullIfExists = false, int multiplicity = 1)
        {
            foreach (var arc in _arcs.Where(arc => arc.Source == source && arc.Target == target)) {
                arc.Multiplicity += multiplicity;
                return returnNullIfExists ? null : arc;
            }
            var newArcTP = new ArcTP(source, target, id, multiplicity);
            _arcs.Add(newArcTP);
            return newArcTP;
        }

        /// <summary>
        /// Adds a place-transition arc.
        /// </summary>
        public Arc AddArcPT(Place source, Transition target, string id = "", 
            bool returnNullIfExists=false, int multiplicity=1)
        {
            foreach (var arc in _arcs.Where(arc => arc.Source == source && arc.Target == target)) {
                arc.Multiplicity += multiplicity;
                return returnNullIfExists ? null : arc;
            }
            var newArcPT = new ArcPT(source, target, id, multiplicity);
            _arcs.Add(newArcPT);
            return newArcPT;
        }

        /// <summary>
        /// Removes an arc from the net.
        /// </summary>
        public void RemoveArc(Arc removedArc)
        {
            removedArc.Source.DisconnectArc(removedArc);
            removedArc.Target.DisconnectArc(removedArc);
            _arcs.Remove(removedArc);
        }

        /// <summary>
        /// Removes a node from the net as well as all the connected arcs.
        /// </summary>
        public void RemoveNode(Node removedNode)
        {
            if (removedNode is Place) {
                var rp = removedNode as Place;
                var arcs = new List<Arc>(rp.InArcs);
                arcs.AddRange(rp.OutArcs);
                
                foreach (var arc in arcs) {
                    RemoveArc(arc);
                }

                _places.Remove(rp);
            } else if (removedNode is Transition) {
                var rt = removedNode as Transition;
                var arcs = new List<Arc>(rt.InArcs);
                arcs.AddRange(rt.OutArcs);

                foreach (var arc in arcs) {
                    RemoveArc(arc);
                }

                _transitions.Remove(rt);
            }
        }

        /// <summary>
        /// Returns True is the given nodes are connected.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        /// <returns>True if connected, false otherwise.</returns>
        public bool AreNodesConnected(Node a, Node b)
        {
            return Arcs.Any(
                arc => (arc.Source == a && arc.Target == b) 
                       || (arc.Source == b && arc.Target == a)
                );
        }

        /// <summary>
        /// Adds one token to a place.
        /// </summary>
        public void AddToken(Place p)
        {
            _places[p]++;
        }

        /// <summary>
        /// Removes one token from a place.
        /// </summary>
        public void RemoveToken(Place p)
        {
            if (_places[p] > 0) _places[p]--;
        }

        /// <summary>
        /// Removes all tokens from a place.
        /// </summary>
        public void RemoveAllTokens(Place p)
        {
            _places[p] = 0;
        }

        /// <summary>
        /// Fires selected transition and changes the marking of the net.
        /// </summary>
        /// <param name="t"></param>
        public bool FireTransition(Transition t)
        {
            if (t.InArcs.Count == 0)
                return false;
            if (t.InArcs.Any(inarc => !_places.ContainsKey((Place)inarc.Source) || _places[(Place)inarc.Source] < inarc.Multiplicity))
                return false;
            foreach (var inarc in t.InArcs)
                _places[(Place)inarc.Source] = (uint)(_places[(Place)inarc.Source] - inarc.Multiplicity);
            foreach (var outarc in t.OutArcs)
                _places[(Place)outarc.Target] = (uint)(_places[(Place)outarc.Target] + outarc.Multiplicity);
            return true;
        }

        #region Deserialization

        /// <summary>
        /// Builds one node from an xml-element.
        /// </summary>
        /// <param name="netElement">Node element.</param>
        /// <param name="ns">XML xis-namespace.</param>
        private void BuildNode(XElement netElement, XNamespace ns)
        {
            string type = netElement.Attribute(ns + "type").Value;
            string name = netElement.Attribute("name") == null
                ? ""
                : netElement.Attribute("name").Value;

            try {
                switch (type) {
                    case "hlpn:Place":
                        AddPlace(name, netElement.Attribute("id").Value);
                        break;
                    case "hlpn:Transition":
                        AddTransition(name, netElement.Attribute("id").Value);
                        break;
                    default:
                        throw new NetSystemXmlException();
                }
            } catch (XmlException) {
                throw new NetSystemXmlException();
            }

        }

        /// <summary>
        /// Builds one arc from an xml-element.
        /// </summary>
        /// <param name="netElement">Node element.</param>
        /// <param name="ns">XML xis-namespace.</param>
        private void BuildArc(XElement netElement, XNamespace ns)
        {
            string type = netElement.Attribute(ns + "type").Value;
            string sourceId = netElement.Attribute("source").Value.Substring(1);
            string targetId = netElement.Attribute("target").Value.Substring(1);

            try {
                switch (type) {
                    case "hlpn:ArcPT":
                        AddArcPT(
                            _places.Single(p => p.Key.Id == sourceId).Key,
                            _transitions.Single(t => t.Id == targetId),
                            netElement.Attribute("id").Value
                            );
                        break;
                    case "hlpn:ArcTP":
                        AddArcTP(
                            _transitions.Single(p => p.Id == sourceId),
                            _places.Single(t => t.Key.Id == targetId).Key,
                            netElement.Attribute("id").Value
                            );
                        break;
                    default:
                        throw new NetSystemXmlException();
                }
            } catch (InvalidOperationException) {
                throw new NetSystemXmlException();
            }
        }

        /// <summary>
        /// Gets a token from an xml-element.
        /// </summary>
        /// <param name="token">Token element.</param>
        private void GetToken(XElement token)
        {
            var id = token.Attribute("id").Value;
            var multiplicity = token.Attribute("multiplicity") == null ? "1" : token.Attribute("multiplicity").Value;

            var place = _places.Keys.FirstOrDefault(p => p.Id == id);
            if (place != null) {
                uint m;
                if (!uint.TryParse(multiplicity, out m))
                    throw new NetMarkingXmlException("Token multiplicity value is invalid.");
                _places[place] = m;
            } else
                throw new NetMarkingXmlException("Corresponding place does not exist.");
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Constructs and returns an XML-element 
        /// that holds the info about the net.
        /// </summary>
        public XDocument Serialize(bool serializeMarking=false)
        {
            var xdoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"));

            // Create namespaces.
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace hlpn = "mathtech.ru/npntool/hlpn";
            XNamespace npndiagrams = "http:/mathtech.ru/npntool/npndiagrams";
            XNamespace npnets = "mathtech.ru/npntool/npnets";

            // Root element.
            var root = new XElement(npnets + "NPnetMarked");
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", xsi));
            root.Add(new XAttribute(XNamespace.Xmlns + "hlpn", hlpn));
            root.Add(new XAttribute(XNamespace.Xmlns + "npndiagrams", npndiagrams));
            root.Add(new XAttribute(XNamespace.Xmlns + "npnets", npnets));
            root.Add(new XAttribute("id", Guid.NewGuid()));

            // Net element.
            var net = new XElement("net");
            net.Add(new XAttribute("id", Guid.NewGuid()));

            // NetSystem element.
            var netSystem = new XElement("netSystem");
            netSystem.Add(new XAttribute("id", Id));

            // Nodes elements.
            foreach (var tr in _transitions)
                netSystem.Add(tr.GetXml(xsi));
            foreach (var place in _places.Keys)
                netSystem.Add(place.GetXml(xsi));

            // Arcs elements.
            foreach (var arc in _arcs)
                netSystem.Add(arc.GetXml(xsi));

            net.Add(netSystem);
            // Serialize marking, is selected.
            if (serializeMarking) 
                net.Add(SerializeMarking());
            root.Add(net);
            xdoc.Add(root);

            return xdoc;
        }

        /// <summary>
        /// Constructs and returns an XML-element 
        /// that holds the info about the marking of the net.
        /// </summary>
        public XElement SerializeMarking()
        {
            var netMarking = new XElement("netMarking");
            netMarking.Add(new XAttribute("netId", Id));
            foreach (var place in Places)
            {
                if (place.Value > 0)
                {
                    var tokenElem = new XElement("token",
                        new XAttribute("id", place.Key.Id)
                        );
                    if (place.Value > 1)
                        tokenElem.Add(new XAttribute("multiplicity", place.Value));
                    netMarking.Add(tokenElem);
                }
            }
            return netMarking;
        }

        #endregion

        #region IDeepCloneable

        /// <summary>
        /// Returns a deep copy of the net.
        /// </summary>
        public Net DeepClone()
        {
            var newNet = new Net(Id);

            // Copy places.
            foreach (var place in _places)
            {
                var newPlace = newNet.AddPlace(place.Key.Name, place.Key.Id);
                for (var i = 0; i < place.Value; i++)
                    newNet.AddToken(newPlace);
            }

            // Copy transitions.
            foreach (var transition in _transitions)
                newNet.AddTransition(transition.Name, transition.Id);

            // Copy connections.
            foreach (var arc in Arcs)
            {
                var idSource = arc.Source.Id;
                var idTarget = arc.Target.Id;

                if (arc is ArcPT) {
                    var sourcePlace = newNet.Places.Keys.First(p => p.Id == idSource);
                    var targetTransition = newNet.Transitions.First(t => t.Id == idTarget);
                    newNet.AddArcPT(sourcePlace, targetTransition, arc.Id, multiplicity: arc.Multiplicity);
                } else if (arc is ArcTP) {
                    var sourceTransition = newNet.Transitions.First(t => t.Id == idSource);
                    var targetPlace = newNet.Places.Keys.First(p => p.Id == idTarget);
                    newNet.AddArcTP(sourceTransition, targetPlace, arc.Id, multiplicity: arc.Multiplicity);
                }
            }

            return newNet;
        }

        #endregion
    }
}
