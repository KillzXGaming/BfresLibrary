using System;
using System.Collections.Generic;
using System.Diagnostics;
using BfresLibrary.Core;
using BfresLibrary.GX2;
using Syroot.BinaryData;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an attribute of a <see cref="VertexBuffer"/> describing the data format, type and layout of a
    /// specific data subset in the buffer.
    /// </summary>
    [DebuggerDisplay(nameof(VertexAttrib) + " {" + nameof(Name) + "}")]
    public class VertexAttrib : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttrib"/> class.
        /// </summary>
        public VertexAttrib()
        {
            Name = "";
            BufferIndex = 0;
            Offset = 0;
            Format = GX2AttribFormat.Format_32_32_32_32_Single;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{VertexAttrib}"/> instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the index of the buffer storing the data in the <see cref="VertexBuffer.Buffers"/> list.
        /// </summary>
        public byte BufferIndex { get; set; }

        /// <summary>
        /// Gets or sets the offset in bytes to the attribute in each vertex.
        /// </summary>
        public ushort Offset { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="GX2AttribFormat"/> determining the type in which attribute data is available.
        /// </summary>
        public GX2AttribFormat Format { get; set; }
        
        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                Name = loader.LoadString();
                loader.ByteOrder = ByteOrder.BigEndian;
                Format = ConvertToGX2(loader.ReadEnum<SwitchAttribFormat>(true));
                loader.ByteOrder = ByteOrder.LittleEndian;
                loader.Seek(2); //padding
                Offset = loader.ReadUInt16();
                BufferIndex = (byte)loader.ReadUInt16();
            }
            else
            {
                Name = loader.LoadString();
                BufferIndex = loader.ReadByte();
                loader.Seek(1);
                Offset = loader.ReadUInt16();
                Format = loader.ReadEnum<GX2AttribFormat>(true);
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                saver.SaveString(Name);
                saver.ByteOrder = ByteOrder.BigEndian;
                saver.Write(ConvertFromGX2(Format), true);
                saver.ByteOrder = ByteOrder.LittleEndian;
                saver.Write((short)0);
                saver.Write(Offset);
                saver.Write(BufferIndex);
                saver.Seek(1);
            }
            else
            {
                saver.SaveString(Name);
                saver.Write(BufferIndex);
                saver.Seek(1);
                saver.Write(Offset);
                saver.Write(Format, true);
            }
        }

        private GX2AttribFormat ConvertToGX2(SwitchAttribFormat att) {
            return (GX2AttribFormat)Enum.Parse(typeof(GX2AttribFormat), att.ToString());
        }

        private SwitchAttribFormat ConvertFromGX2(GX2AttribFormat att) {
            return (SwitchAttribFormat)Enum.Parse(typeof(SwitchAttribFormat), att.ToString());
        }

        public enum SwitchAttribFormat : ushort
        {
            // 8 bits (8 x 1)
            Format_8_UNorm = 0x00000102, //
            Format_8_UInt = 0x00000302, //
            Format_8_SNorm = 0x00000202, //
            Format_8_SInt = 0x00000402, //
            Format_8_UIntToSingle = 0x00000802,
            Format_8_SIntToSingle = 0x00000A02,
            // 8 bits (4 x 2)
            Format_4_4_UNorm = 0x00000001,
            // 16 bits (16 x 1)
            Format_16_UNorm = 0x0000010A,
            Format_16_UInt = 0x0000020A,
            Format_16_SNorm = 0x0000030A,
            Format_16_SInt = 0x0000040A,
            Format_16_Single = 0x0000050A,
            Format_16_UIntToSingle = 0x00000803,
            Format_16_SIntToSingle = 0x00000A03,
            // 16 bits (8 x 2)
            Format_8_8_UNorm = 0x00000109, //
            Format_8_8_UInt = 0x00000309, //
            Format_8_8_SNorm = 0x00000209, //
            Format_8_8_SInt = 0x00000409, //
            Format_8_8_UIntToSingle = 0x00000804,
            Format_8_8_SIntToSingle = 0x00000A04,
            // 32 bits (16 x 2)
            Format_16_16_UNorm = 0x00000112, //
            Format_16_16_SNorm = 0x00000212, //
            Format_16_16_UInt = 0x00000312,
            Format_16_16_SInt = 0x00000412,
            Format_16_16_Single = 0x00000512, //
            Format_16_16_UIntToSingle = 0x00000807,
            Format_16_16_SIntToSingle = 0x00000A07,
            // 32 bits (10/11 x 3)
            Format_10_11_11_Single = 0x00000809,
            // 32 bits (8 x 4)
            Format_8_8_8_8_UNorm = 0x0000010B, //
            Format_8_8_8_8_SNorm = 0x0000020B, //
            Format_8_8_8_8_UInt = 0x0000030B, //
            Format_8_8_8_8_SInt = 0x0000040B, //
            Format_8_8_8_8_UIntToSingle = 0x0000080B,
            Format_8_8_8_8_SIntToSingle = 0x00000A0B,
            // 32 bits (10 x 3 + 2)
            Format_10_10_10_2_UNorm = 0x0000000B,
            Format_10_10_10_2_UInt = 0x0000090B,
            Format_10_10_10_2_SNorm = 0x0000020E, // High 2 bits are UNorm //
            Format_10_10_10_2_SInt = 0x0000099B,
            // 64 bits (16 x 4)
            Format_16_16_16_16_UNorm = 0x00000115, //
            Format_16_16_16_16_SNorm = 0x00000215, //
            Format_16_16_16_16_UInt = 0x00000315, //
            Format_16_16_16_16_SInt = 0x00000415, //
            Format_16_16_16_16_Single = 0x00000515, //
            Format_16_16_16_16_UIntToSingle = 0x0000080E,
            Format_16_16_16_16_SIntToSingle = 0x00000A0E,
            // 32 bits (32 x 1)
            Format_32_UInt = 0x00000314,
            Format_32_SInt = 0x00000416,
            Format_32_Single = 0x00000516,
            // 64 bits (32 x 2)
            Format_32_32_UInt = 0x00000317, //
            Format_32_32_SInt = 0x00000417, //
            Format_32_32_Single = 0x00000517, //
                                              // 96 bits (32 x 3)
            Format_32_32_32_UInt = 0x00000318, //
            Format_32_32_32_SInt = 0x00000418, //
            Format_32_32_32_Single = 0x00000518, //
                                                 // 128 bits (32 x 4)
            Format_32_32_32_32_UInt = 0x00000319, //
            Format_32_32_32_32_SInt = 0x00000419, //
            Format_32_32_32_32_Single = 0x00000519 //
        }
    }
}