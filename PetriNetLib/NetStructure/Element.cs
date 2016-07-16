using System;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Base for an element of a Petri net.
    /// </summary>
    public abstract class Element
    {
        /// <summary>
        /// An element's ID.
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Initialize elements by an ID.
        /// </summary>
        /// <param name="id">Generated, if not given.</param>
        protected Element(string id = "")
        {
            Id = (id == "") ? Guid.NewGuid().ToString() : id;
        }
    }
}
