namespace PetriNetLib.NetStructure
{
    /// <summary>
    /// Provides deep copying of objects.
    /// </summary>
    public interface IDeepCloneable<out T>
    {
        T DeepClone();
    }
}
