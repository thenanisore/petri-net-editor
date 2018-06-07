using System.Collections.Generic;
using System.Xml.Linq;

namespace NetEditor.ViewModels
{
    /// <summary>
    /// Represents a component of a net as a diagram object.
    /// </summary>
    public class ComponentViewModel : SubnetViewModel
    {
        /// <summary>
        /// Initialize a new component.
        /// </summary>
        public ComponentViewModel(List<NodeViewModel> component, string name = "", string id = "")
            : base(component, name, id)
        { }

        #region Serialize

        /// <summary>
        /// Returns a component as a XML-element.
        /// </summary>
        public override XElement Serialize()
        {
            var component = new XElement("component",
                new XAttribute("id", Id)
            );
            if (Name != "")
                component.Add(new XAttribute("name", Name));

            // Serialize each node from the component.
            foreach (var nodeViewModel in Nodes)
                component.Add(new XElement("node",
                    new XAttribute("id", nodeViewModel.Node.Id))
                    );

            return component;
        }

        #endregion
    }
}
