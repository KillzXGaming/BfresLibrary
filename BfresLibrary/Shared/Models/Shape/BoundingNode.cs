using BfresLibrary.Core;
using System.ComponentModel;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a node in a <see cref="SubMesh"/> bounding tree to determine when to show which sub mesh of a
    /// <see cref="Mesh"/>.
    /// </summary>
    public class BoundingNode : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        [Category("Binary Search Tree")]
        [DisplayName("Left ID")]
        public ushort LeftChildIndex { get; set; }

        [Category("Binary Search Tree")]
        public ushort NextSibling { get; set; }

        [Category("Binary Search Tree")]
        [DisplayName("Right ID")]
        public ushort RightChildIndex { get; set; }

        [Category("Unknown")]
        [DisplayName("Unknown")]
        public ushort Unknown { get; set; }

        [Category("Visibilty Groups")]
        [DisplayName("SubMesh ID")]
        public ushort SubMeshIndex { get; set; }

        [Category("Visibilty Groups")]
        [DisplayName("SubMesh Count")]
        public ushort SubMeshCount { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            LeftChildIndex = loader.ReadUInt16();
            RightChildIndex = loader.ReadUInt16();
            Unknown = loader.ReadUInt16();
            NextSibling = loader.ReadUInt16();
            SubMeshIndex = loader.ReadUInt16();
            SubMeshCount = loader.ReadUInt16();
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(LeftChildIndex);
            saver.Write(RightChildIndex);
            saver.Write(Unknown);
            saver.Write(NextSibling);
            saver.Write(SubMeshIndex);
            saver.Write(SubMeshCount);
        }
    }
}