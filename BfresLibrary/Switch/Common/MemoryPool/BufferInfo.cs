using System.Collections.Generic;
using Syroot.Maths;
using BfresLibrary.Core;
using BfresLibrary.Switch.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an buffer info section in a <see cref="ResFile"/> subfile. References vertex and index buffers
    /// </summary>
    public class BufferInfo : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public uint getIndexBufferSize(ResFileSaver saver)
        {
            uint Size = 0;

            //Add all buffer byte[] to a list
            foreach (Model fmdl in saver.ResFile.Models.Values)
            {
                foreach (Shape shp in fmdl.Shapes.Values)
                {
                    foreach (Mesh msh in shp.Meshes)
                    {
                        Size += (uint)msh.Data.Length;
                        if (Size % 8 != 0) Size = Size + (8 - (Size % 8));
                    }
                }
            }
            return Size;
        }

        public uint GetVertexBufferSize(ResFileSaver saver)
        {
            uint Size = 0;

            foreach (Model fmdl in saver.ResFile.Models.Values)
            {
                foreach (VertexBuffer vtx in fmdl.VertexBuffers) {
                    foreach (Buffer buff in vtx.Buffers) {
                        Size += (uint)buff.Data[0].Length;
                        if (Size % 8 != 0) Size = Size + (8 - (Size % 8));
                    }
                }
            }
            return Size;
        }


        private void SetIndexBufferData(ResFileSaver saver)
       {
            List<byte[]> IndxBuffList = new List<byte[]>();

            //Add all buffer byte[] to a list
            foreach (Model fmdl in saver.ResFile.Models.Values) {
                foreach (Shape shp in fmdl.Shapes.Values) {
                    foreach (Mesh msh in shp.Meshes)
                    {
                        IndxBuffList.Add(msh.Data);
                    }
                }
            }

            IndexBufferData = new byte[IndxBuffList.Count][];

            for (int i = 0; i < IndxBuffList.Count; i++)
            {
                IndexBufferData[i] = IndxBuffList[i];
            }
            IndxBuffList.Clear();
        }

        private void SetVertexBufferData(ResFileSaver saver)
        {
            List<byte[]> VtxBuffList = new List<byte[]>();

            foreach (Model fmdl in saver.ResFile.Models.Values) {
                foreach (VertexBuffer vtx in fmdl.VertexBuffers) {
                    foreach (Buffer buff in vtx.Buffers) {
                        VtxBuffList.Add(buff.Data[0]);
                    }
                }
            }

            //Add all buffer byte[] to a list

            VertexBufferData = new byte[VtxBuffList.Count][];

            for (int i = 0; i < VtxBuffList.Count; i++)
            {
                VertexBufferData[i] = VtxBuffList[i];
            }
            VtxBuffList.Clear();
        }

        // ---- PROPERTIES (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the buffer instance that stores face data first, then vertex buffer after.
        /// </summary>
        public static long BufferOffset { get; set; } //Note this is temp

        /// <summary>
        /// Gets or sets the buffer instance that stores face data
        /// </summary>
        public byte[][] VertexBufferData { get; set; }

        /// <summary>
        /// Gets or sets the buffer instance that stores vertex data
        /// </summary>
        public byte[][] IndexBufferData { get; set; }

        /// <summary>
        /// Gets or sets an unkown value
        /// </summary>
        public uint unk { get; set; } = 34;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            unk                 = loader.ReadUInt32();
            uint Size           = loader.ReadUInt32();
            BufferOffset        = loader.ReadInt64();
            byte[] padding      = loader.ReadBytes(16);
        }

        void IResData.Save(ResFileSaver saver)
        {
            SetIndexBufferData(saver);
            SetVertexBufferData(saver);
            saver.Write(unk);

            uint BufferTotalSize = 0;
            for (int i = 0; i < IndexBufferData.Length; i++)
                BufferTotalSize += (uint)IndexBufferData[i].Length;
            for (int i = 0; i < VertexBufferData.Length; i++)
                BufferTotalSize += (uint)VertexBufferData[i].Length;

            if (BufferTotalSize % saver.ResFile.Alignment != 0) BufferTotalSize = BufferTotalSize + (saver.ResFile.Alignment - (BufferTotalSize % saver.ResFile.Alignment));

            ((ResFileSwitchSaver)saver).SaveBufferTotalSize();
            ((ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSwitchSaver.Section2, "Buffer Data");
            ((ResFileSwitchSaver)saver).SaveIndexBufferPointer();
            ((ResFileSwitchSaver)saver).Seek(16);
        }
    }
}
