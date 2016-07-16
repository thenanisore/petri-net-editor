using System;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Represents a place-transition arc.
    /// </summary>
    public class ArcPT : Arc
    {
        // Source/target setting properties.
        
        private Place _source;

        /// <summary>
        /// Source node, place for PT-arcs.
        /// </summary>
        public override Node Source
        {
            get { return _source; }
            set
            {
                if (!(value is Place)) throw new ArgumentException("Source of a PT-arc must be a place.");
                _source = (Place) value;
            }
        }

        private Transition _target;

        /// <summary>
        /// Target node, transition for PT-arcs.
        /// </summary>
        public override Node Target
        {
            get { return _target; }
            set
            {
                if (!(value is Transition)) throw new ArgumentException("Target of a PT-arc must be a transition.");
                _target = (Transition)value;
            }
        }

        /// <summary>
        /// Initialize a place-transition arc.
        /// </summary>
        public ArcPT(Place source, Transition target, 
            string id="", int multiplicity=1)
            : base(id, multiplicity)
        {
            source.AddArcOut(this);
            _source = source;
            target.AddArcIn(this);
            _target = target;
        }
    }
}