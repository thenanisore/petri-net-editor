using System.Xml;

namespace PetriNetLib.Exceptions
{
    /// <summary>
    /// Thrown if a net system element of the npnets-file is invalid.
    /// </summary>
    class NetSystemXmlException : XmlException
    {
        public NetSystemXmlException() 
            : base("The information about the net system is invalid. Check the npnets-file.") { }
        public NetSystemXmlException(string message) 
            : base(message) { }
    }
}