using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PNDApp.ViewModels
{
    /// <summary>
    /// Types of handles (depends on the starting and the ending nodes). 
    /// </summary>
    public enum HandleType
    { PP, TT, PT, TP }

    /// <summary>
    /// Represents a handle as a diagram object.
    /// </summary>
    public class HandleViewModel : SubnetViewModel
    {
        /// <summary>
        /// A parent circuit of the handle.
        /// </summary>
        public CircuitViewModel Circuit { get; private set; }

        /// <summary>
        ///  A type of the handle.
        /// </summary>
        public HandleType Type { get; private set; }

        /// <summary>
        /// Initializes a view model for a certain handle.
        /// </summary>
        public HandleViewModel(List<NodeViewModel> handle, 
            CircuitViewModel parentCircuit, string name = "", string id = "")
            : base(handle, name, id)
        {
            Circuit = parentCircuit;

            // Get the type of the handle.
            if (Nodes[0] is PlaceViewModel) {
                if (Nodes[Nodes.Count - 1] is PlaceViewModel)
                    Type = HandleType.PP;
                else if (Nodes[Nodes.Count - 1] is TransitionViewModel)
                    Type = HandleType.PT;
            } else if (Nodes[0] is TransitionViewModel) {
                if (Nodes[Nodes.Count - 1] is PlaceViewModel)
                    Type = HandleType.TP;
                else if (Nodes[Nodes.Count - 1] is TransitionViewModel)
                    Type = HandleType.TT;
            }
        }

        #region Serialization

        /// <summary>
        /// Returns a handle as a XML-element.
        /// </summary>
        public override XElement Serialize()
        {
            var handle = new XElement("handle", 
                new XAttribute("id", Id),
                new XAttribute("circuitId", Circuit.Id),
                new XAttribute("type", Enum.GetName(typeof(HandleType), Type))
            );

            if (Name != "")
                handle.Add(new XAttribute("name", Name));

            // Serialize each node from the handle.
            foreach (var node in Nodes)
                handle.Add(new XElement("node", 
                    new XAttribute("id", node.Node.Id)
                ));

            return handle;
        }

        #endregion
    }
}
