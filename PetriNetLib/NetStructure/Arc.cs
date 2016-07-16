using System;
using System.Xml.Linq;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// A base for an arc of a Petri net.
    /// </summary>
    public abstract class Arc : Element
    {
        /// <summary>
        /// Source node.
        /// </summary>
        public abstract Node Source { get; set; }

        /// <summary>
        /// Target node.
        /// </summary>
        public abstract Node Target { get; set; }

        /// <summary>
        /// Initializes an arc by id, name and multiplicity.
        /// </summary>
        /// <param name="id">Element ID.</param>
        /// <param name="multiplicity">Multiplicity, 1 by default.</param>
        protected Arc(string id = "", int multiplicity = 1) : base(id)
        {
            Multiplicity = multiplicity;
        }

        private int _multiplicity = 1;
        /// <summary>
        /// Shows the number of connections within an arc.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Multiplicity
        {
            get { return _multiplicity; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException($"Multiplicity must be positive integer.");
                _multiplicity = value;
            }
        }

        #region Serialization

        /// <summary>
        /// Returns an arc as an XML-element.
        /// </summary>
        public XElement GetXml(XNamespace typeNs)
        {
            var elem = new XElement("arcs");

            var type = this is ArcPT ? "hlpn:ArcPT" : "hlpn:ArcTP";
            elem.Add(new XAttribute(typeNs + "type", type));
            elem.Add(new XAttribute("id", Id));
            elem.Add(new XAttribute("source", "#" + Source.Id));
            elem.Add(new XAttribute("target", "#" + Target.Id));

            return elem;
        }

        #endregion
    }
}