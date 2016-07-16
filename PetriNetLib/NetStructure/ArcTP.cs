using System;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Represents a transition-place arc.
    /// </summary>
    public class ArcTP : Arc
    {
        // Source/target setting properties.

        private Transition _source;

        /// <summary>
        /// Source node, transition for TP-arcs.
        /// </summary>
        public override Node Source
        {
            get { return _source; }
            set
            {
                if (!(value is Transition)) throw new ArgumentException("Source of a TP-arc must be a transition.");
                _source = (Transition)value;
            }
        }

        private Place _target;

        /// <summary>
        /// Target node, place for TP-arcs.
        /// </summary>
        public override Node Target
        {
            get { return _target; }
            set
            {
                if (!(value is Place)) throw new ArgumentException("Target of a TP-arc must be a place.");
                _target = (Place)value;
            }
        }

        /// <summary>
        /// Constructs a transition-place arc.
        /// </summary>
        public ArcTP(Transition source, Place target,
            string id = "", int multiplicity = 1)
            : base(id, multiplicity)
        {
            source.AddArcOut(this);
            _source = source;
            target.AddArcIn(this);
            _target = target;
        }
    }
}
