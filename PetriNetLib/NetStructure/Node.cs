using System.Collections.Generic;
using System.Text;

namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// A base for a node of a Petri Net.
    /// </summary>
    public abstract class Node : Element
    {
        /// <summary>
        /// A name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initialize a node by name and id.
        /// </summary>
        /// <param name="id">Generated randomly, if not given.</param>
        protected Node(string name = "", string id = "") 
            : base(id)
        {
            Name = name;
        }

        /// <summary>
        /// Disconnects an arc from this node.
        /// </summary>
        public abstract void DisconnectArc(Arc removedArc);

        #region Serialization

        /// <summary>
        /// Returns a string that contains IDs of the related arcs.
        /// For serialization purposes.
        /// </summary>
        /// <param name="arcList">Related arcs.</param>
        protected static string GetArcsString(List<Arc> arcList)
        {
            var arcs = new StringBuilder();
            foreach (var arc in arcList) {
                arcs.AppendFormat("{0}#{1}",
                    arcs.Length > 0 ? " " : "",
                    arc.Id);
            }
            return arcs.ToString();
        }

        #endregion
    }
}
