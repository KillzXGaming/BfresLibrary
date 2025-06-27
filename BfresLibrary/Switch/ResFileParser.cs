using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Syroot.BinaryData;
using BfresLibrary.Core;
using BfresLibrary.Switch.Core;
using Syroot.NintenTools.NSW.Bntx;

namespace BfresLibrary.Switch
{
    public class ResFileParser
    {
        public static void Load(ResFileSwitchLoader loader, ResFile resFile)
        {
            loader.CheckSignature("FRES");
            uint padding = loader.ReadUInt32();
            resFile.Version = loader.ReadUInt32();
            resFile.SetVersionInfo(resFile.Version);
            resFile.ByteOrder = loader.ReadByteOrder();
            resFile.Alignment = loader.ReadByte();
            resFile.TargetAddressSize = loader.ReadByte(); //Thanks MasterF0X for pointing out the layout of the these
            uint OffsetToFileName = loader.ReadUInt32();
            resFile.Flag = loader.ReadUInt16();
            resFile.BlockOffset = loader.ReadUInt16();
            uint RelocationTableOffset = loader.ReadUInt32();
            uint sizFile = loader.ReadUInt32();
            resFile.Name = loader.LoadString();
            long modelOffset = loader.ReadOffset();
            long modelDictOffset = loader.ReadOffset();
            if (loader.ResFile.VersionMajor2 >= 9) {
                loader.ReadBytes(32); //reserved
            }
            resFile.SkeletalAnims = loader.LoadDictValues<SkeletalAnim>();
            resFile.MaterialAnims = loader.LoadDictValues<MaterialAnim>();
            resFile.BoneVisibilityAnims = loader.LoadDictValues<VisibilityAnim>();
            resFile.ShapeAnims = loader.LoadDictValues<ShapeAnim>();
            resFile.SceneAnims = loader.LoadDictValues<SceneAnim>();
            resFile.MemoryPool = loader.Load<MemoryPool>();
            resFile.BufferInfo = loader.Load<BufferInfo>();

            if (loader.ResFile.VersionMajor2 >= 10)
            {
                //Peek at external flags
                byte PeekFlags()
                {
                    using (loader.TemporarySeek(0xee, SeekOrigin.Begin)) {
                        return loader.ReadByte();
                    }
                }

                var flag = (ResFile.ExternalFlags)PeekFlags();
                if (flag.HasFlag(ResFile.ExternalFlags.HoldsExternalStrings))
                {
                    long externalFileOffset = loader.ReadOffset();
                    var externalFileDict = loader.LoadDict<ResString>();

                    using (loader.TemporarySeek(externalFileOffset, SeekOrigin.Begin))
                    {
                        StringCache.Strings.Clear();
                        foreach (var str in externalFileDict.Keys)
                        {
                            long stringID = loader.ReadInt64();
                            StringCache.Strings.Add(stringID, str);
                        }
                    }
                    return;
                }
                //GPU section for TOTK
                if (flag.HasFlag(ResFile.ExternalFlags.HasExternalGPU))
                {
                    using (loader.TemporarySeek(sizFile, SeekOrigin.Begin))
                    {
                        uint gpuDataOffset = loader.ReadUInt32();
                        uint gpuBufferSize = loader.ReadUInt32();

                        resFile.BufferInfo = new BufferInfo();
                        BufferInfo.BufferOffset = sizFile + 288;
                    }
                }
            }

            resFile.ExternalFiles = loader.LoadDictValues<ExternalFile>();
            long padding1 = loader.ReadInt64();
            resFile.StringTable = loader.Load<StringTable>();
            uint StringPoolSize = loader.ReadUInt32();
            ushort numModel = loader.ReadUInt16();

            //Read models after buffer data
            resFile.Models = loader.LoadDictValues<Model>(modelDictOffset, modelOffset);

            if (loader.ResFile.VersionMajor2 >= 9)
            {
                //Count for 2 new sections
                ushort unkCount = loader.ReadUInt16();
                ushort unk2Count = loader.ReadUInt16();

                if (unkCount != 0) throw new System.Exception("unk1 has section!");
                if (unk2Count != 0) throw new System.Exception("unk2 has section!");
            }

            ushort numSkeletalAnim = loader.ReadUInt16();
            ushort numMaterialAnim = loader.ReadUInt16();
            ushort numBoneVisibilityAnim = loader.ReadUInt16();
            ushort numShapeAnim = loader.ReadUInt16();
            ushort numSceneAnim = loader.ReadUInt16();
            ushort numExternalFile = loader.ReadUInt16();
            resFile.ExternalFlag = (ResFile.ExternalFlags)loader.ReadByte();
            byte reserve10 = loader.ReadByte();

            uint padding3 = loader.ReadUInt32();

            if (reserve10 == 1 || resFile.ExternalFlag != 0)
            {
                resFile.DataAlignmentOverride = 0x1000;
            }

            resFile.Textures = new ResDict<TextureShared>();
            foreach (var ext in resFile.ExternalFiles) {
                if (ext.Key.Contains(".bntx")) 
                {
                    BntxFile bntx = new BntxFile(new MemoryStream(ext.Value.Data));
                    ext.Value.LoadedFileData = bntx;
                    foreach (var tex in bntx.Textures)
                        resFile.Textures.Add(tex.Name, new SwitchTexture(bntx, tex));

                    // Empty the data to save memory space. Bntx is resaved directly
                    ext.Value.Data = new byte[0];
                }
            }

            resFile.TexPatternAnims = new ResDict<MaterialAnim>();
            resFile.MatVisibilityAnims = new ResDict<MaterialAnim>();
            resFile.ShaderParamAnims = new ResDict<MaterialAnim>();
            resFile.ColorAnims = new ResDict<MaterialAnim>();
            resFile.TexSrtAnims = new ResDict<MaterialAnim>();

            //Split material animations into shader, texture, and visual animation lists
            foreach (var anim in resFile.MaterialAnims.Values)
            {
                if (anim.Name.Contains("_ftp"))
                    resFile.TexPatternAnims.Add(anim.Name, anim);
                else if(anim.Name.Contains("_fts"))
                    resFile.ShaderParamAnims.Add(anim.Name, anim);
                else if (anim.Name.Contains("_fcl"))
                    resFile.ColorAnims.Add(anim.Name, anim);
                else if (anim.Name.Contains("_fst"))
                    resFile.TexSrtAnims.Add(anim.Name, anim);
                else if (anim.Name.Contains("_fvt"))
                    resFile.MatVisibilityAnims.Add(anim.Name, anim);
                else if (anim.MaterialAnimDataList != null && anim.MaterialAnimDataList.Any(x => x.VisibilyCount > 0))
                    resFile.MatVisibilityAnims.Add(anim.Name, anim);
                else if (anim.MaterialAnimDataList != null && anim.MaterialAnimDataList.Any(x => x.TexturePatternCount > 0))
                    resFile.TexPatternAnims.Add(anim.Name, anim);
                else
                    resFile.ShaderParamAnims.Add(anim.Name, anim);
            }
        }

        public static void Save(ResFileSwitchSaver saver, ResFile resFile)
        {
            if (resFile.Models.Count > 0 && resFile.Models.Values.Any(x => x.Shapes.Count > 0)) {
                resFile.MemoryPool = new MemoryPool();
                resFile.BufferInfo = new BufferInfo();

                foreach (var model in resFile.Models.Values)
                {
                    foreach (var vertexBuffer in model.VertexBuffers)
                        vertexBuffer.MemoryPool = resFile.MemoryPool;

                    foreach (var shape in model.Shapes.Values) {
                        foreach (var mesh in shape.Meshes)
                            mesh.MemoryPool = resFile.MemoryPool;
                    }
                }
            }

            saver.WriteSignature("FRES");
            saver.Write(0x20202020);
            saver.Write(resFile.Version);
            saver.WriteByteOrder(resFile.ByteOrder);
            saver.Write((byte)resFile.Alignment);
            saver.Write((byte)resFile.TargetAddressSize);
            saver.SaveFileNameString(resFile.Name);
            saver.Write((ushort)resFile.Flag);
            saver.SaveHeaderBlock(true);
            saver.SaveRelocationTablePointerPointer();
            saver.SaveFieldFileSize();

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.SaveRelocateEntryToSection(saver.Position, 17, 1, 0, ResFileSwitchSaver.Section1, "ResFile");
            else
                saver.SaveRelocateEntryToSection(saver.Position, 13, 1, 0, ResFileSwitchSaver.Section1, "ResFile");

            saver.SaveString(resFile.Name);
            resFile.ModelOffset = saver.SaveOffset();
            resFile.ModelDictOffset = saver.SaveOffset();

            //2 New sections
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(0L);
                saver.Write(0L);
                saver.Write(0L);
                saver.Write(0L);
            }

            resFile.SkeletonAnimationOffset = saver.SaveOffset();
            resFile.SkeletonAnimationDictOffset = saver.SaveOffset();
            resFile.MaterialAnimationOffset = saver.SaveOffset();
            resFile.MaterialAnimationnDictOffset = saver.SaveOffset();
            resFile.BoneVisAnimationOffset = saver.SaveOffset();
            resFile.BoneVisAnimationDictOffset = saver.SaveOffset();
            resFile.ShapeAnimationOffset = saver.SaveOffset();
            resFile.ShapeAnimationDictOffset = saver.SaveOffset();
            resFile.SceneAnimationOffset = saver.SaveOffset();
            resFile.SceneAnimationDictOffset = saver.SaveOffset();

            if (resFile.MemoryPool != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSwitchSaver.Section4, "Memory pool");
            saver.SaveMemoryPoolPointer();

            if (resFile.BufferInfo != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSwitchSaver.Section1, "Buffer info");

            resFile.BufferInfoOffset = saver.SaveOffset();
            if (resFile.ExternalFiles.Count > 0)
                saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSwitchSaver.Section1, "External Files");
            resFile.ExternalFileOffset = saver.SaveOffset();
            resFile.ExternalFileDictOffset = saver.SaveOffset();
            saver.Write(0L); // padding
            saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSwitchSaver.Section1, "String pool");
            saver.SaveFieldStringPool();
            saver.Write((ushort)resFile.Models.Count);

            //2 New sections
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write((ushort)0);
                saver.Write((ushort)0);
            }

            saver.Write((ushort)resFile.SkeletalAnims.Count);
            saver.Write((ushort)resFile.MaterialAnims.Count);
            saver.Write((ushort)resFile.BoneVisibilityAnims.Count);
            saver.Write((ushort)resFile.ShapeAnims.Count);
            saver.Write((ushort)resFile.SceneAnims.Count);
            saver.Write((ushort)resFile.ExternalFiles.Count);

            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write((byte)0); //external flags (set to 0)
                if (saver.ResFile.VersionMajor2 >= 10)
                    saver.Write((byte)1); //saved flags
                else
                    saver.Write((byte)0);
            }
            else
                saver.Seek(6); // padding
        }
    }
}
