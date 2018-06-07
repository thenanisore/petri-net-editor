using PetriNetLib.NetStructure;

namespace NetEditor.ViewModels
{
    /// <summary>
    /// Represents a transition as a diagram object.
    /// </summary>
    public class TransitionViewModel : NodeViewModel
    {
        /// <summary>
        /// Initializates a transition view model.
        /// </summary>
        /// <param name="relTransition">Related transition.</param>
        /// <param name="x">x-coordinate.</param>
        /// <param name="y">y-coordinate.</param>
        public TransitionViewModel(Transition relTransition, double x, double y)
            : base(relTransition, x ,y)
        { }
    }
}
