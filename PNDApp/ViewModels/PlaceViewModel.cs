using PetriNetLib.NetStructure;

namespace PNDApp.ViewModels
{
    /// <summary>
    /// Represents a place as a diagram object.
    /// </summary>
    public class PlaceViewModel : NodeViewModel
    {
        /// <summary>
        /// Initializates a place view model.
        /// </summary>
        /// <param name="relPlace">Related place.</param>
        /// <param name="x">x-coordinate.</param>
        /// <param name="y">y-coordinate.</param>
        /// <param name="tokens">Number of tokens to add.</param>
        public PlaceViewModel(Place relPlace, double x, double y, uint tokens=0)
            : base(relPlace, x, y)
        {
            Tokens = tokens;
        }

        #region Marking

        private uint _tokens;

        /// <summary>
        /// Gets of sets the number of tokens.
        /// </summary>
        public uint Tokens
        {
            get { return _tokens; }
            set
            {
                if (_tokens != value)
                {
                    IsToken = value > 0;
                    IsTokens = value > 1;
                    _tokens = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isToken;

        /// <summary>
        /// True if at least one token exists.
        /// </summary>
        public bool IsToken
        {
            get { return _isToken; }
            set
            {
                if (_isToken != value) {
                    _isToken = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTokens;

        /// <summary>
        /// True if at least two tokens exist.
        /// </summary>
        public bool IsTokens
        {
            get { return _isTokens; }
            set
            {
                if (_isTokens != value) {
                    _isTokens = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion
    }
}
