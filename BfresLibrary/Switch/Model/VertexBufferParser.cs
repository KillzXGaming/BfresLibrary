using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;
using System.IO;

namespace BfresLibrary.Switch
{
    internal class VertexBufferParser
    {
        public static void Load(ResFileSwitchLoader loader, VertexBuffer vertexBuffer)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
                vertexBuffer.Flags = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            vertexBuffer.Attributes = loader.LoadDictValues<VertexAttrib>();
            vertexBuffer.MemoryPool = loader.Load<MemoryPool>();
            long unk = loader.ReadOffset();
            if (loader.ResFile.VersionMajor2 > 2 || loader.ResFile.VersionMajor > 0)
                loader.ReadOffset();// unk2
            long VertexBufferSizeOffset = loader.ReadOffset();
            long VertexStrideSizeOffset = loader.ReadOffset();
            long padding = loader.ReadInt64();
            int BufferOffset = loader.ReadInt32();
            byte numVertexAttrib = loader.ReadByte();
            byte numBuffer = loader.ReadByte();
            ushort Idx = loader.ReadUInt16();
            vertexBuffer.VertexCount = loader.ReadUInt32();
            vertexBuffer.VertexSkinCount = (byte)loader.ReadUInt16();
            vertexBuffer.GPUBufferAlignent = loader.ReadUInt16();

            //Buffers use the index buffer offset from memory info section
            //This goes to a section in the memory pool which stores all the buffer data, including faces
            //To obtain a list of all the buffer data, it would be by the index buffer offset + BufferOffset

            var StrideArray = loader.LoadList<VertexBufferStride>(numBuffer, (uint)VertexStrideSizeOffset);
            var VertexBufferSizeArray = loader.LoadList<VertexBufferSize>(numBuffer, (uint)VertexBufferSizeOffset);

            vertexBuffer.Buffers = new List<Buffer>();
            using (loader.TemporarySeek(BufferInfo.BufferOffset + BufferOffset, SeekOrigin.Begin))
            {
                for (int buff = 0; buff < numBuffer; buff++)
                {
                    Buffer buffer = new Buffer();
                    buffer.Data = new byte[1][];
                    buffer.Stride = (ushort)StrideArray[buff].Stride;

                    loader.Align(8);
                    buffer.Data[0] = loader.ReadBytes((int)VertexBufferSizeArray[buff].Size);
                    vertexBuffer.Buffers.Add(buffer);
                }
            }
        }

        public static void Save(ResFileSwitchSaver saver, VertexBuffer vertexBuffer)
        {
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(vertexBuffer.Flags);
            else
                saver.Seek(12);
            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSwitchSaver.Section1, "FVTX");
            vertexBuffer.AttributeOffset = saver.SaveOffset();
            vertexBuffer.AttributeDictOffset = saver.SaveOffset();
            if (vertexBuffer.MemoryPool != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSwitchSaver.Section4, "Vertex Memory pool");
            saver.SaveMemoryPoolPointer();

            saver.SaveRelocateEntryToSection(saver.Position, 4, 1, 0, ResFileSwitchSaver.Section1, "Vertex buffer info");
            vertexBuffer.UnkBufferOffset = saver.SaveOffset();
            vertexBuffer.UnkBuffer2Offset = saver.SaveOffset();
            vertexBuffer.BufferSizeArrayOffset = saver.SaveOffset();
            vertexBuffer.StideArrayOffset = saver.SaveOffset();
            saver.Write(0L); //padding
            saver.Write(SetVertexBufferArrayOffset(vertexBuffer, saver)); //Buffer Offset
            saver.Write((byte)vertexBuffer.Attributes.Count);
            saver.Write((byte)vertexBuffer.Buffers.Count);
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write(vertexBuffer.VertexCount);
            saver.Write((ushort)vertexBuffer.VertexSkinCount);
            saver.Write((ushort)vertexBuffer.GPUBufferAlignent);

        }

        public static uint SetVertexBufferArrayOffset(VertexBuffer vertexBuffer, ResFileSaver saver)
        {
            //Add all previous buffers until it reaches current one
            //This adds all face buffers (those goes first) then the vertex ones

            uint Pos = (uint)BufferInfo.BufferOffset;

            uint TotalSize = Pos;

            if (saver.ExportedShape != null)
            {
                foreach (Mesh msh in saver.ExportedShape.Meshes)
                {
                    if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                    TotalSize += (uint)msh.Data.Length;
                }
                return TotalSize - Pos;
            }

            foreach (Model fmdl in saver.ResFile.Models.Values)
            {
                foreach (Shape shp in fmdl.Shapes.Values)
                {
                    foreach (Mesh msh in shp.Meshes)
                    {
                        if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                        TotalSize += (uint)msh.Data.Length;
                    }
                }
            }
            foreach (Model fmdl in saver.ResFile.Models.Values)
            {
                foreach (VertexBuffer vtx in fmdl.VertexBuffers)
                {
                    foreach (Buffer buff in vtx.Buffers)
                    {
                        if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                        if (vtx == vertexBuffer)
                            return TotalSize - Pos;

                        TotalSize += buff.Size;
                    }
                }
            }

            TotalSize = TotalSize - Pos;

            return TotalSize;   
        }
    }
}
