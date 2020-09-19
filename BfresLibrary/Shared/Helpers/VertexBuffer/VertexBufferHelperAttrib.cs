using System;
using System.Diagnostics;
using Syroot.Maths;
using BfresLibrary.GX2;

namespace BfresLibrary.Helpers
{
    /// <summary>
    /// Represents an attribute and the data it stores in a <see cref="VertexBufferHelper"/> instance.
    /// </summary>
    [DebuggerDisplay(nameof(VertexBufferHelperAttrib) + " {" + nameof(Name) + "} {" + nameof(Format) + "}")]
    public class VertexBufferHelperAttrib
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The name of the attribute, typically used to determine the use of the data.
        /// </summary>
        public string Name;

        /// <summary>
        /// The <see cref="GX2AttribFormat"/> into which data will be converted upon creating a
        /// <see cref="VertexBuffer"/>.
        /// </summary>
        public GX2AttribFormat Format;

        /// <summary>
        /// The data stored for this attribute. Has to be of the same length as every other
        /// <see cref="VertexBufferHelperAttrib"/>. Depending on <see cref="Format"/>, not every component of the
        /// <see cref="Vector4F"/> elements is used.
        /// </summary>
        public Vector4F[] Data;

        public byte BufferIndex { get; set; }

        //Allow the user to edit this manually if necessary
        //Some games will align data inside and keep a fixed size
        private uint stride = 0;
        public uint Stride
        {
            get {
                if (stride == 0)
                    return (uint)FormatSize;
                else
                    return (uint)stride;
            }
            set { stride = value; }
        }

        /// <summary>
        /// The offset of the buffer data.
        /// </summary>
        public uint Offset = 0;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        internal int FormatSize
        {
            get
            {
                switch (Format)
                {
                    case GX2AttribFormat.Format_8_UNorm:
                    case GX2AttribFormat.Format_8_UInt:
                    case GX2AttribFormat.Format_8_SNorm:
                    case GX2AttribFormat.Format_8_SInt:
                    case GX2AttribFormat.Format_8_UIntToSingle:
                    case GX2AttribFormat.Format_8_SIntToSingle:
                    case GX2AttribFormat.Format_4_4_UNorm:
                        return 1;
                    case GX2AttribFormat.Format_16_UNorm:
                    case GX2AttribFormat.Format_16_UInt:
                    case GX2AttribFormat.Format_16_SNorm:
                    case GX2AttribFormat.Format_16_SInt:
                    case GX2AttribFormat.Format_16_Single:
                    case GX2AttribFormat.Format_16_UIntToSingle:
                    case GX2AttribFormat.Format_16_SIntToSingle:
                    case GX2AttribFormat.Format_8_8_UNorm:
                    case GX2AttribFormat.Format_8_8_UInt:
                    case GX2AttribFormat.Format_8_8_SNorm:
                    case GX2AttribFormat.Format_8_8_SInt:
                    case GX2AttribFormat.Format_8_8_UIntToSingle:
                    case GX2AttribFormat.Format_8_8_SIntToSingle:
                        return 2;
                    case GX2AttribFormat.Format_32_UInt:
                    case GX2AttribFormat.Format_32_SInt:
                    case GX2AttribFormat.Format_32_Single:
                    case GX2AttribFormat.Format_16_16_UNorm:
                    case GX2AttribFormat.Format_16_16_UInt:
                    case GX2AttribFormat.Format_16_16_SNorm:
                    case GX2AttribFormat.Format_16_16_SInt:
                    case GX2AttribFormat.Format_16_16_Single:
                    case GX2AttribFormat.Format_16_16_UIntToSingle:
                    case GX2AttribFormat.Format_16_16_SIntToSingle:
                    case GX2AttribFormat.Format_10_11_11_Single:
                    case GX2AttribFormat.Format_8_8_8_8_UNorm:
                    case GX2AttribFormat.Format_8_8_8_8_UInt:
                    case GX2AttribFormat.Format_8_8_8_8_SNorm:
                    case GX2AttribFormat.Format_8_8_8_8_SInt:
                    case GX2AttribFormat.Format_8_8_8_8_UIntToSingle:
                    case GX2AttribFormat.Format_8_8_8_8_SIntToSingle:
                    case GX2AttribFormat.Format_10_10_10_2_UNorm:
                    case GX2AttribFormat.Format_10_10_10_2_UInt:
                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                    case GX2AttribFormat.Format_10_10_10_2_SInt:
                        return 4;
                    case GX2AttribFormat.Format_32_32_UInt:
                    case GX2AttribFormat.Format_32_32_SInt:
                    case GX2AttribFormat.Format_32_32_Single:
                    case GX2AttribFormat.Format_16_16_16_16_UNorm:
                    case GX2AttribFormat.Format_16_16_16_16_UInt:
                    case GX2AttribFormat.Format_16_16_16_16_SNorm:
                    case GX2AttribFormat.Format_16_16_16_16_SInt:
                    case GX2AttribFormat.Format_16_16_16_16_Single:
                    case GX2AttribFormat.Format_16_16_16_16_UIntToSingle:
                    case GX2AttribFormat.Format_16_16_16_16_SIntToSingle:
                        return 8;
                    case GX2AttribFormat.Format_32_32_32_UInt:
                    case GX2AttribFormat.Format_32_32_32_SInt:
                    case GX2AttribFormat.Format_32_32_32_Single:
                        return 12;
                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                    case GX2AttribFormat.Format_32_32_32_32_SInt:
                    case GX2AttribFormat.Format_32_32_32_32_Single:
                        return 16;
                    default: throw new ArgumentException($"Invalid {nameof(GX2AttribFormat)} {Format}.",
                        nameof(Format));
                }
            }
        }
    }
}
