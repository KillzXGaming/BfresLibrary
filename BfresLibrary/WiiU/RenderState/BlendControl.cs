using BfresLibrary.Core;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents GX2 settings controlling color and alpha blending.
    /// </summary>
    public class BlendControl
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _colorSourceBlendBit = 0, _colorSourceBlendBits = 5;
        private const int _colorCombineBit = 5, _colorCombineBits = 3;
        private const int _colorDestinationBlendBit = 8, _colorDestinationBlendBits = 5;
        private const int _alphaSourceBlendBit = 16, _alphaSourceBlendBits = 5;
        private const int _alphaCombineBit = 21, _alphaCombineBits = 3;
        private const int _alphaDestinationBlendBit = 24, _alphaDestinationBlendBits = 5;
        private const int _separateAlphaBlendBit = 29;

        // ---- FIELDS -------------------------------------------------------------------------------------------------
        
        internal uint Value;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the color source blend operation.
        /// </summary>
        public GX2BlendFunction ColorSourceBlend
        {
            get { return (GX2BlendFunction)Value.Decode(_colorSourceBlendBit, _colorSourceBlendBits); }
            set { Value = Value.Encode((uint)value, _colorSourceBlendBit, _colorSourceBlendBits); }
        }

        /// <summary>
        /// Gets or sets the color combine operation.
        /// </summary>
        public GX2BlendCombine ColorCombine
        {
            get { return (GX2BlendCombine)Value.Decode(_colorCombineBit, _colorCombineBits); }
            set { Value = Value.Encode((uint)value, _colorCombineBit, _colorCombineBits); }
        }

        /// <summary>
        /// Gets or sets the color destination blend operation.
        /// </summary>
        public GX2BlendFunction ColorDestinationBlend
        {
            get { return (GX2BlendFunction)Value.Decode(_colorDestinationBlendBit, _colorDestinationBlendBits); }
            set { Value = Value.Encode((uint)value, _colorDestinationBlendBit, _colorDestinationBlendBits); }
        }

        /// <summary>
        /// Gets or sets the alpha source blend operation.
        /// </summary>
        public GX2BlendFunction AlphaSourceBlend
        {
            get { return (GX2BlendFunction)Value.Decode(_alphaSourceBlendBit, _alphaSourceBlendBits); }
            set { Value = Value.Encode((uint)value, _alphaSourceBlendBit, _alphaSourceBlendBits); }
        }

        /// <summary>
        /// Gets or sets the alpha combine operation.
        /// </summary>
        public GX2BlendCombine AlphaCombine
        {
            get { return (GX2BlendCombine)Value.Decode(_alphaCombineBit, _alphaCombineBits); }
            set { Value = Value.Encode((uint)value, _alphaCombineBit, _alphaCombineBits); }
        }

        /// <summary>
        /// Gets or sets the alpha destination blend operation.
        /// </summary>
        public GX2BlendFunction AlphaDestinationBlend
        {
            get { return (GX2BlendFunction)Value.Decode(_alphaDestinationBlendBit, _alphaDestinationBlendBits); }
            set { Value = Value.Encode((uint)value, _alphaDestinationBlendBit, _alphaDestinationBlendBits); }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether alpha blending is separated from color blending.
        /// </summary>
        public bool SeparateAlphaBlend
        {
            get { return Value.GetBit(_separateAlphaBlendBit); }
            set { Value = Value.SetBit(_separateAlphaBlendBit, value); }
        }
    }
}