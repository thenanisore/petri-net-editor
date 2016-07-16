using System.Xml;

namespace PetriNetLib.Exceptions
{
    /// <summary>
    /// Thrown if the net marking element of the npnets-file is invalid.
    /// </summary>
    class NetMarkingXmlException : XmlException
    {
        public NetMarkingXmlException() 
            : base("The information about the net marking is invalid. Check the npnets-file.") { }
        public NetMarkingXmlException(string message) 
            : base(message) { }
    }
}