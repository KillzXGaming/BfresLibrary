using System;
using System.Diagnostics;
using BfresLibrary.Core;

namespace BfresLibrary.Switch
{
    /// <summary>
    /// Represents stride and size in a <see cref="VertexBuffer"/>
    /// specific data subset in the buffer.
    /// </summary>
    [DebuggerDisplay(nameof(VertexBufferSize) + " {" + nameof(Size) + "}")]
    public class VertexBufferSize : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of a full vertex in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// The gpu access flags.
        /// </summary>
        public uint GpuAccessFlags { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Size = loader.ReadUInt32();
            GpuAccessFlags = loader.ReadUInt32();
            loader.Seek(8);
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(Size);
            saver.Write(GpuAccessFlags);
            saver.Seek(8);
        }
    }
}