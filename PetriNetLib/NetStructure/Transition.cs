using System.Collections.Generic;
using System.Xml.Linq;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Represents a transition in a Petri net.
    /// </summary>
    public class Transition : Node
    {
        private readonly List<ArcPT> _inArcs;

        /// <summary>
        /// Arcs to the node.
        /// </summary>
        public IReadOnlyList<ArcPT> InArcs
        {
            get { return _inArcs.AsReadOnly(); }
        }

        private readonly List<ArcTP> _outArcs;

        /// <summary>
        /// Arcs from the node.
        /// </summary>
        public IReadOnlyList<ArcTP> OutArcs
        {
            get { return _outArcs.AsReadOnly(); }
        }

        /// <summary>
        /// Constructs a transition.
        /// </summary>
        public Transition(string name = "", string id = "")
            : base(name, id)
        {
            _inArcs = new List<ArcPT>();
            _outArcs = new List<ArcTP>();
        }

        /// <summary>
        /// Connect an input PT-arc.
        /// </summary>
        public void AddArcIn(ArcPT arc)
        {
            _inArcs.Add(arc);
        }

        /// <summary>
        /// Connect an output TP-arc.
        /// </summary>
        public void AddArcOut(ArcTP arc)
        {
            _outArcs.Add(arc);
        }

        /// <summary>
        /// Disconnects an arc from the transition.
        /// </summary>
        public override void DisconnectArc(Arc arc)
        {
            if (arc is ArcTP)
                _outArcs.Remove(arc as ArcTP);
            if (arc is ArcPT)
                _inArcs.Remove(arc as ArcPT);
        }

        #region Serialization

        /// <summary>
        /// Returns a transition as an XML-element.
        /// </summary>
        public XElement GetXml(XNamespace typeNs)
        {
            var elem = new XElement("nodes");

            elem.Add(new XAttribute(typeNs + "type", "hlpn:Transition"));
            elem.Add(new XAttribute("id", Id));
            if (Name.Length > 0)
                elem.Add(new XAttribute("name", Name));
            if (_outArcs.Count > 0)
                elem.Add(new XAttribute("outArcs", GetArcsString(_outArcs.ConvertAll(x => (Arc)x))));
            if (_inArcs.Count > 0)
                elem.Add(new XAttribute("inArcs", GetArcsString(_inArcs.ConvertAll(x => (Arc)x))));

            return elem;
        }

        #endregion
    }
}