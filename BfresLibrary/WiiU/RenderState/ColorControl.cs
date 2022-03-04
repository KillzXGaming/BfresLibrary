using BfresLibrary.Core;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents GX2 settings controlling additional color blending options.
    /// </summary>
    public class ColorControl
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _multiWriteBit = 1;
        private const int _colorBufferBit = 4;
        private const int _blendEnableBit = 8, _blendEnableBits = 8;
        private const int _logicOpBit = 16, _logicOpBits = 8;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal uint Value;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether multi writes are enabled.
        /// </summary>
        public bool MultiWriteEnabled
        {
            get { return Value.GetBit(_multiWriteBit); }
            set { Value = Value.SetBit(_multiWriteBit, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the color buffer is enabled.
        /// </summary>
        public bool ColorBufferEnabled
        {
            get { return Value.GetBit(_colorBufferBit); }
            set { Value = Value.SetBit(_colorBufferBit, value); }
        }

        /// <summary>
        /// Gets or sets the bitmask used for blending.
        /// </summary>
        public byte BlendEnableMask
        {
            get { return (byte)Value.Decode(_blendEnableBit, _blendEnableBits); }
            set { Value = Value.Encode(value, _blendEnableBit, _blendEnableBits); }
        }

        /// <summary>
        /// Gets or sets the ROP3 logic operation.
        /// </summary>
        public GX2LogicOp LogicOp
        {
            get { return (GX2LogicOp)Value.Decode(_logicOpBit, _logicOpBits); }
            set { Value = Value.Encode((uint)value, _logicOpBit, _logicOpBits); }
        }
    }
}