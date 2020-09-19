using System.Diagnostics;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a subarray of a <see cref="Mesh"/> section, storing a slice of indices to draw from the index buffer
    /// referenced in the mesh, mostly used for hiding parts of a model when not visible.
    /// </summary>
    [DebuggerDisplay(nameof(SubMesh) + " [{" + nameof(Offset) + "},{" + nameof(Count) + "})")]
    public class SubMesh : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the offset into the index buffer in bytes.
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// Gets the number of indices to reference.
        /// </summary>
        public uint Count { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Offset = loader.ReadUInt32();
            Count = loader.ReadUInt32();
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(Offset);
            saver.Write(Count);
        }
    }
}