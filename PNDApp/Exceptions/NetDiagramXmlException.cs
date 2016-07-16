using System;
using System.Xml;

namespace PNDApp.Exceptions
{
    /// <summary>
    /// Thrown if the net diagram element of the npnets-file is invalid.
    /// </summary>
    class NetDiagramXmlException : XmlException
    {
        public NetDiagramXmlException() 
            : base("The information about the net diagram is invalid. Check the npnets-file." + Environment.NewLine + "The net will be arranged automatically.") { }
        public NetDiagramXmlException(string message) 
            : base(message) { }
    }
}
