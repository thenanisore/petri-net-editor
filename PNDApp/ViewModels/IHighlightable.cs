namespace PNDApp.ViewModels
{
    /// <summary>
    /// Support for highlighting in the subgraphs.
    /// </summary>
    public interface IHighlightable
    {
        /// <summary>
        /// Disables highlight of the object.
        /// </summary>
        void DisableHighlight();

        /// <summary>
        /// True if the object is highlighted in a strongly connected component.
        /// </summary>
        bool IsHighlightedSCC { get; set; }

        /// <summary>
        /// True if the object is highlighted in a circuit.
        /// </summary>
        bool IsHighlightedCircuit { get; set; }

        /// <summary>
        /// True if the object is highlighted in a handle.
        /// </summary>
        bool IsHighlightedHandle { get; set; }
    }
}
