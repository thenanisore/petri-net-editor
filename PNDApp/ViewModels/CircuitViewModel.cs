using System.Collections.Generic;
using System.Xml.Linq;

namespace PNDApp.ViewModels
{
    /// <summary>
    /// Represents a circuit of a net as a diagram object.
    /// </summary>
    public class CircuitViewModel : SubnetViewModel
    {
        /// <summary>
        /// Initialize a new circuit.
        /// </summary>
        public CircuitViewModel(List<NodeViewModel> circuit, string name = "", string id = "") 
            : base(circuit, name, id)
        { }

        #region Serialize

        /// <summary>
        /// Returns a circuit as a XML-element.
        /// </summary>
        public override XElement Serialize()
        {
            var circuit = new XElement("circuit", 
                new XAttribute("id", Id)
            );
            if (Name != "")
                circuit.Add(new XAttribute("name", Name));

            // Serialize each node from the circuit.
            foreach (var nodeViewModel in Nodes)
                circuit.Add(new XElement("node",
                    new XAttribute("id", nodeViewModel.Node.Id))
                );

            return circuit;
        }

        #endregion
    }
}
