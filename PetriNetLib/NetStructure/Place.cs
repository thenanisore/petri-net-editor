using System.Collections.Generic;
using System.Xml.Linq;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Represents a place in a Petri net.
    /// </summary>
    public class Place : Node
    {
        private readonly List<ArcTP> _inArcs;

        /// <summary>
        /// Arcs to the node.
        /// </summary>
        public IReadOnlyList<ArcTP> InArcs
        {
            get { return _inArcs.AsReadOnly(); }
        }

        private readonly List<ArcPT> _outArcs;

        /// <summary>
        /// Arcs from the node.
        /// </summary>
        public IReadOnlyList<ArcPT> OutArcs
        {
            get { return _outArcs.AsReadOnly(); }
        }

        /// <summary>
        /// Constructs a place.
        /// </summary>
        public Place(string name="", string id = "")
            : base(name, id)
        {
            _inArcs = new List<ArcTP>();
            _outArcs = new List<ArcPT>();
        }

        /// <summary>
        /// Connects an input TP-arc.
        /// </summary>
        public void AddArcIn(ArcTP arc)
        {
            _inArcs.Add(arc);
        }

        /// <summary>
        /// Connects an output PT-arc.
        /// </summary>
        public void AddArcOut(ArcPT arc)
        {
            _outArcs.Add(arc);
        }

        /// <summary>
        /// Disconnects an arc from the place.
        /// </summary>
        /// <param name="arc"></param>
        public override void DisconnectArc(Arc arc)
        {
            if (arc is ArcTP)
                _inArcs.Remove(arc as ArcTP);
            if (arc is ArcPT)
                _outArcs.Remove(arc as ArcPT);
        }

        #region Serialization

        /// <summary>
        /// Returns a place as an XML-element.
        /// </summary>
        public XElement GetXml(XNamespace typeNs)
        {
            var elem = new XElement("nodes");

            elem.Add(new XAttribute(typeNs + "type", "hlpn:Place"));
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
