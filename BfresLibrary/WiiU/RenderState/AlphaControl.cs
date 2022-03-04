using BfresLibrary.Core;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents GX2 settings controlling additional alpha blending options.
    /// </summary>
    public class AlphaControl
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _alphaFuncBit = 0, _alphaFuncBits = 3;
        private const int _alphaFuncEnabledBit = 3;
        
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal uint Value;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether alpha testing is enabled at all.
        /// </summary>
        public bool AlphaTestEnabled
        {
            get { return Value.GetBit(_alphaFuncEnabledBit); }
            set { Value = Value.SetBit(_alphaFuncEnabledBit, value); }
        }

        /// <summary>
        /// Gets or sets the comparison functions to use for alpha testing.
        /// </summary>
        public GX2CompareFunction AlphaFunc
        {
            get { return (GX2CompareFunction)Value.Decode(_alphaFuncBit, _alphaFuncBits); }
            set { Value = Value.Encode((uint)value, _alphaFuncBit, _alphaFuncBits); }
        }
    }
}