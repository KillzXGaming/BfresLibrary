using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    internal class ShapeParser
    {
        public static void Read(ResFileSwitchLoader loader, Shape shape)
        {
            if (loader.ResFile.VersionMajor2 == 9)
                shape.Flags = loader.ReadEnum<ShapeFlags>(false);
            else
                loader.LoadHeaderBlock();

            shape.Name = loader.LoadString();
            shape.VertexBuffer = loader.Load<VertexBuffer>();
            long MeshArrayOffset = loader.ReadOffset();
            long SkinBoneIndexListOffset = loader.ReadOffset();
            shape.KeyShapes = loader.LoadDictValues<KeyShape>();
            long BoundingBoxArrayOffset = loader.ReadOffset();
            long RadiusOffset = 0;
            if (loader.ResFile.VersionMajor2 > 2 || loader.ResFile.VersionMajor > 0)
            {
                RadiusOffset = loader.ReadOffset();
                long UserPointer = loader.ReadInt64();
            }
            else
            {
                long UserPointer = loader.ReadInt64();
                shape.RadiusArray.Add(loader.ReadSingle());
            }
            if (loader.ResFile.VersionMajor2 != 9)
                shape.Flags = loader.ReadEnum<ShapeFlags>(true);
            ushort idx = loader.ReadUInt16();
            shape.MaterialIndex = loader.ReadUInt16();
            shape.BoneIndex = loader.ReadUInt16();
            shape.VertexBufferIndex = loader.ReadUInt16();
            ushort numSkinBoneIndex = loader.ReadUInt16();
            shape.VertexSkinCount = loader.ReadByte();
            byte numMesh = loader.ReadByte();
            byte numKeys = loader.ReadByte();
            byte numTargetAttr = loader.ReadByte();
            if (loader.ResFile.VersionMajor2 <= 2 && loader.ResFile.VersionMajor == 0)
                loader.Seek(2); //padding
            else if (loader.ResFile.VersionMajor2 == 9)
                loader.Seek(2); //padding
            else
                loader.Seek(6); //padding

            shape.Meshes = numMesh == 0 ? new List<Mesh>() : loader.LoadList<Mesh>(numMesh, (uint)MeshArrayOffset).ToList();
            shape.SkinBoneIndices = numSkinBoneIndex == 0 ? new List<ushort>() : loader.LoadCustom(() => loader.ReadUInt16s(numSkinBoneIndex), (uint)SkinBoneIndexListOffset)?.ToList();

            if (RadiusOffset != 0)
                shape.RadiusArray = numMesh == 0 ? new List<float>() : loader.LoadCustom(() => loader.ReadSingles(numMesh), (uint)RadiusOffset).ToList();

            int boundingboxCount = shape.Meshes.Sum(x => x.SubMeshes.Count + 1);
            shape.SubMeshBoundings = boundingboxCount == 0 ? new List<Bounding>() : loader.LoadCustom(() => 
                            loader.ReadBoundings(boundingboxCount), (uint)BoundingBoxArrayOffset)?.ToList();

            shape.SubMeshBoundingNodes = new List<BoundingNode>();
        }

        public static void Write(ResFileSwitchSaver saver, Shape shape)
        {
            if (saver.ResFile.VersionMajor2 == 9)
                saver.Write(shape.Flags, true);
            else
                saver.Seek(12);

            if (shape.SkinBoneIndices == null)
                shape.SkinBoneIndices = new List<ushort>();

            int boundingboxCount = shape.Meshes.Sum(x => x.SubMeshes.Count+1);
            List<Bounding> boundings = new List<Bounding>();
            if (shape.SubMeshBoundingNodes.Count > 0)
            {
                for (int i = 0; i < shape.SubMeshBoundingNodes.Count; i++) {
                    if (shape.SubMeshBoundingNodes[i].SubMeshCount == 1) {
                        boundings.Add(shape.SubMeshBoundings[i]);
                    }
                }
                boundings.Add(shape.SubMeshBoundings[0]);
            }
            else
            {
                for (int i = 0; i < boundingboxCount; i++)
                {
                    if (shape.SubMeshBoundings.Count <= i)
                        boundings.Add(shape.SubMeshBoundings.LastOrDefault());
                    else
                        boundings.Add(shape.SubMeshBoundings[i]);
                }
            }

            float[] boundingRadius = new float[shape.Meshes.Count];
            for (int i = 0; i < shape.Meshes.Count; i++)
            {
                if (shape.RadiusArray.Count <= i)
                    boundingRadius[i] = shape.RadiusArray.LastOrDefault();
                else
                    boundingRadius[i] = shape.RadiusArray[i];
            }

            shape.RadiusArray = boundingRadius.ToList();
            shape.SubMeshBoundings = boundings.ToList();

            saver.SaveRelocateEntryToSection(saver.Position, 8, 1, 0, ResFileSwitchSaver.Section1, "FSHP");
            saver.SaveString(shape.Name);
            saver.Write(shape.VertexBuffer.Position);
            shape.PosMeshArrayOffset = saver.SaveOffset();
            shape.PosSkinBoneIndicesOffset = saver.SaveOffset();
            shape.PosKeyShapesOffset = saver.SaveOffset();
            shape.PosKeyShapeDictOffset = saver.SaveOffset();
            shape.PosSubMeshBoundingsOffset = saver.SaveOffset();
            shape.PosRadiusArrayOffset = saver.SaveOffset();
            saver.Write(0L); //padding
            if (saver.ResFile.VersionMajor2 != 9)
                saver.Write(shape.Flags, true);
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write(shape.MaterialIndex);
            saver.Write(shape.BoneIndex);
            saver.Write(shape.VertexBufferIndex);
            saver.Write((ushort)shape.SkinBoneIndices.Count);
            saver.Write(shape.VertexSkinCount);
            saver.Write((byte)shape.Meshes.Count);
            saver.Write((byte)shape.KeyShapes.Count);
            saver.Write(shape.TargetAttribCount);

            if (saver.ResFile.VersionMajor2 != 9)
                saver.Seek(6);
            else
                saver.Seek(2);
        }
    }
}
