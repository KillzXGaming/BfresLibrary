using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;
using BfresLibrary.Core;
using BfresLibrary.GX2;
using System.Linq;

namespace BfresLibrary
{
    /// <summary>
    /// Represents the surface net of a <see cref="Shape"/> section, storing information on which
    /// index <see cref="Buffer"/> to use for referencing vertices of the shape, mostly used for different levels of
    /// detail (LoD) models.
    /// </summary>
    public class Mesh : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh()
        {
            PrimitiveType = GX2PrimitiveType.Triangles;
            IndexFormat = GX2IndexFormat.UInt16;
            SubMeshes = new List<SubMesh>();
            IndexBuffer = new Buffer();
            MemoryPool = new MemoryPool();
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="GX2PrimitiveType"/> which determines how indices are used to form polygons.
        /// </summary>
        public GX2PrimitiveType PrimitiveType { get; set; }

        /// <summary>
        /// Gets the <see cref="GX2IndexFormat"/> determining the data type of the indices in the
        /// <see cref="IndexBuffer"/>.
        /// </summary>
        public GX2IndexFormat IndexFormat { get; private set; }

        /// <summary>
        /// Gets the number of indices stored in the <see cref="IndexBuffer"/>.
        /// </summary>
        public uint IndexCount
        {
            get
            {
                // Sum indices in all bufferings together, even if only first is mostly used.
                int elementCount = 0;
                int formatSize = FormatSize;
                for (int i = 0; i < IndexBuffer.Data.Length; i++)
                {
                    int bufferingSize = IndexBuffer.Data[i].Length;
                    if (bufferingSize % formatSize != 0)
                    {
                        throw new InvalidDataException($"Cannot form complete indices from {IndexBuffer}.");
                    }
                    elementCount += bufferingSize / formatSize;
                }
                return (uint)elementCount;
            }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="SubMesh"/> instances which split up a mesh into parts which can be
        /// hidden if they are not visible to optimize rendering performance.
        /// </summary>
        public List<SubMesh> SubMeshes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Buffer"/> storing the index data.
        /// </summary>
        public Buffer IndexBuffer { get; set; }

        internal byte[] Data
        {
            get { return IndexBuffer.Data[0]; }
        }

        /// <summary>
        /// Gets or sets the offset to the first vertex element of a <see cref="VertexBuffer"/> to reference by indices.
        /// </summary>
        public uint FirstVertex { get; set; }
        
        internal int FormatSize
        {
            get
            {
                switch (IndexFormat)
                {
                    case GX2IndexFormat.UInt16:
                    case GX2IndexFormat.UInt16LittleEndian:
                        return sizeof(ushort);
                    case GX2IndexFormat.UInt32:
                    case GX2IndexFormat.UInt32LittleEndian:
                        return sizeof(uint);
                    default:
                        throw new ArgumentException($"Invalid {nameof(GX2IndexFormat)} {IndexFormat}.", nameof(IndexFormat));
                }
            }
        }

        internal ByteOrder FormatByteOrder
        {
            get
            {
                switch (IndexFormat)
                {
                    case GX2IndexFormat.UInt16LittleEndian:
                    case GX2IndexFormat.UInt32LittleEndian:
                        return ByteOrder.LittleEndian;
                    case GX2IndexFormat.UInt16:
                    case GX2IndexFormat.UInt32:
                        return ByteOrder.BigEndian;
                    default:
                        throw new ArgumentException($"Invalid {nameof(GX2IndexFormat)} {IndexFormat}.", nameof(IndexFormat));
                }
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the indices stored in the <see cref="IndexBuffer"/> as <see cref="UInt32"/> instances.
        /// </summary>
        /// <returns>The indices stored in the <see cref="IndexBuffer"/>.</returns>
        public IEnumerable<uint> GetIndices()
        {
            using (BinaryDataReader reader = new BinaryDataReader(new MemoryStream(IndexBuffer.Data[0])))
            {
                reader.ByteOrder = FormatByteOrder;

                // Read and return the elements.
                uint elementCount = IndexCount;
                switch (IndexFormat)
                {
                    case GX2IndexFormat.UInt16:
                    case GX2IndexFormat.UInt16LittleEndian:
                        for (; elementCount > 0; elementCount--)
                        {
                            yield return reader.ReadUInt16();
                        }
                        break;
                    default:
                        for (; elementCount > 0; elementCount--)
                        {
                            yield return reader.ReadUInt32();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Stores the given <paramref name="indices"/> in the <see cref="IndexBuffer"/> in the provided
        /// <paramref name="format"/>, or the current <see cref="IndexFormat"/> if none was specified.
        /// </summary>
        /// <param name="indices">The indices to store in the <see cref="IndexBuffer"/>.</param>
        /// <param name="format">The <see cref="GX2IndexFormat"/> to use or <c>null</c> to use the current format.
        /// </param>
        public void SetIndices(IList<uint> indices, GX2IndexFormat? format = null)
        {
            IndexFormat = format ?? IndexFormat;
            IndexBuffer.Data = new byte[1][] { new byte[indices.Count * FormatSize] };
            using (BinaryDataWriter writer = new BinaryDataWriter(new MemoryStream(IndexBuffer.Data[0], true)))
            {
                writer.ByteOrder = FormatByteOrder;

                // Write the elements.
                switch (IndexFormat)
                {
                    case GX2IndexFormat.UInt16:
                    case GX2IndexFormat.UInt16LittleEndian:
                        foreach (uint index in indices)
                        {
                            writer.Write((ushort)index);
                        }
                        break;
                    default:
                        writer.Write(indices);
                        break;
                }
            }
        }

        internal MemoryPool MemoryPool;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                uint SubMeshArrayOffset = loader.ReadOffset();
                MemoryPool = loader.Load<MemoryPool>();
                long Buffer = loader.ReadOffset();
                var bufferSize = loader.Load<BufferSize>();
                uint FaceBufferOffset = loader.ReadUInt32();
                PrimitiveType = PrimitiveTypeList[loader.ReadEnum<SwitchPrimitiveType>(true)];
                IndexFormat = IndexList[loader.ReadEnum<SwitchIndexFormat>(true)];
                uint indexCount = loader.ReadUInt32();
                FirstVertex = loader.ReadUInt32();
                ushort numSubMesh = loader.ReadUInt16();
                ushort padding = loader.ReadUInt16();
                SubMeshes = loader.LoadList<SubMesh>(numSubMesh, SubMeshArrayOffset).ToList();

                uint DataOffset = (uint)BufferInfo.BufferOffset + FaceBufferOffset;

                //Load buffer data from mem block
                IndexBuffer = new Buffer();
                IndexBuffer.Flags = bufferSize.Flag;
                IndexBuffer.Data = new byte[1][];
                IndexBuffer.Data[0] = loader.LoadCustom(() => loader.ReadBytes((int)bufferSize.Size), DataOffset);
            }
            else
            {
                PrimitiveType = loader.ReadEnum<GX2PrimitiveType>(true);
                IndexFormat = loader.ReadEnum<GX2IndexFormat>(true);
                uint indexCount = loader.ReadUInt32();
                ushort numSubMesh = loader.ReadUInt16();
                loader.Seek(2);
                SubMeshes = loader.LoadList<SubMesh>(numSubMesh).ToList();
                IndexBuffer = loader.Load<Buffer>();
                FirstVertex = loader.ReadUInt32();
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
           if (saver.IsSwitch)
            {
                var bufferSize = new BufferSize();
                bufferSize.Size = (uint)Data.Length;

                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 1, 1, 0, Switch.Core.ResFileSwitchSaver.Section1, "Mesh");
                PosSubMeshesOffset = saver.SaveOffset();
                if (MemoryPool != null)
                    ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 1, 1, 0, Switch.Core.ResFileSwitchSaver.Section4, "Mesh Memory pool");
                ((Switch.Core.ResFileSwitchSaver)saver).SaveMemoryPoolPointer();
                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 2, 1, 0, Switch.Core.ResFileSwitchSaver.Section1, "Mesh buffer info");
                PosBufferUnkOffset = saver.SaveOffset();
                PosBufferSizeOffset = saver.SaveOffset();
                saver.Write(SetFaceBufferOffset(saver)); //face buffer
                saver.Write(PrimitiveTypeList.FirstOrDefault(x => x.Value == PrimitiveType).Key, true);
                saver.Write(IndexList.FirstOrDefault(x => x.Value == IndexFormat).Key, true);
                saver.Write(IndexCount);
                saver.Write(FirstVertex);
                saver.Write((ushort)SubMeshes.Count);
                saver.Seek(2);
            }
            else
            {
                saver.Write(PrimitiveType, true);
                saver.Write(IndexFormat, true);
                saver.Write(IndexCount);
                saver.Write((ushort)SubMeshes.Count);
                saver.Seek(2);
                saver.SaveList(SubMeshes);
                saver.Save(IndexBuffer);
                saver.Write(FirstVertex);
            }
        }

        public void UpdateIndexBufferByteOrder(ByteOrder byteOrder)
        {
            var indices = GetIndices();
            if (byteOrder == ByteOrder.BigEndian)
            {
                if (IndexFormat == GX2IndexFormat.UInt16 || IndexFormat == GX2IndexFormat.UInt16LittleEndian)
                    SetIndices(indices.ToList(), GX2IndexFormat.UInt16);
                else
                    SetIndices(indices.ToList(), GX2IndexFormat.UInt32);
            }
            else
            {
                if (IndexFormat == GX2IndexFormat.UInt16 || IndexFormat == GX2IndexFormat.UInt16LittleEndian)
                    SetIndices(indices.ToList(), GX2IndexFormat.UInt16LittleEndian);
                else
                    SetIndices(indices.ToList(), GX2IndexFormat.UInt32LittleEndian);
            }
        }

        Dictionary<SwitchIndexFormat, GX2IndexFormat> IndexList = new Dictionary<SwitchIndexFormat, GX2IndexFormat>()
        {
            { SwitchIndexFormat.UInt16, GX2IndexFormat.UInt16LittleEndian },
            { SwitchIndexFormat.UInt32, GX2IndexFormat.UInt32LittleEndian },
            { SwitchIndexFormat.UnsignedByte, GX2IndexFormat.UInt16LittleEndian },
        };

        Dictionary<SwitchPrimitiveType, GX2PrimitiveType> PrimitiveTypeList = new Dictionary<SwitchPrimitiveType, GX2PrimitiveType>()
        {
            { SwitchPrimitiveType.Triangles, GX2PrimitiveType.Triangles },
            { SwitchPrimitiveType.TrianglesAdjacency, GX2PrimitiveType.TrianglesAdjacency },
            { SwitchPrimitiveType.TriangleStrip, GX2PrimitiveType.TriangleStrip },
            { SwitchPrimitiveType.TriangleStripAdjacency, GX2PrimitiveType.TriangleStripAdjacency },
            { SwitchPrimitiveType.Lines, GX2PrimitiveType.Lines },
            { SwitchPrimitiveType.LinesAdjacency, GX2PrimitiveType.LinesAdjacency },
            { SwitchPrimitiveType.LineStrip, GX2PrimitiveType.LineStrip },
            { SwitchPrimitiveType.LineStripAdjacency, GX2PrimitiveType.LineStripAdjacency },
            { SwitchPrimitiveType.Points, GX2PrimitiveType.Points },
        };

        enum SwitchIndexFormat : uint
        {
            UnsignedByte = 0,
            UInt16 = 1,
            UInt32 = 2,
        }

        enum SwitchPrimitiveType : uint
        {
            Points = 0x00,
            Lines = 0x01,
            LineStrip = 0x02,
            Triangles = 0x03,
            TriangleStrip = 0x04,
            LinesAdjacency = 0x05,
            LineStripAdjacency = 0x06,
            TrianglesAdjacency = 0x07,
            TriangleStripAdjacency = 0x08,
            Patches = 0x09,
        }

        private uint SetFaceBufferOffset(ResFileSaver saver)
        {
            //Add all previous buffers until it reaches current one
            uint TotalSize = 0;

            if (saver.ExportedShape != null)
            {
                foreach (Mesh msh in saver.ExportedShape.Meshes)
                {
                    if (msh == this)
                        return TotalSize;

                    TotalSize += (uint)msh.Data.Length;
                    if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                }
                return TotalSize;
            }

            foreach (Model fmdl in saver.ResFile.Models.Values)
            {
                foreach (Shape shp in fmdl.Shapes.Values)
                {
                    foreach (Mesh msh in shp.Meshes)
                    {
                        if (msh == this)
                            return TotalSize;

                        TotalSize += (uint)msh.Data.Length;
                        if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                    }
                }
            }
            return TotalSize;
        }

        internal long PosSubMeshesOffset;
        internal long PosBufferUnkOffset;
        internal long PosBufferSizeOffset;
    }
}