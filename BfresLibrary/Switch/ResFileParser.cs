using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Syroot.BinaryData;
using Syroot.NintenTools.Bfres.Core;
using Syroot.NintenTools.Bfres.Switch.Core;
using Syroot.NintenTools.NSW.Bntx;

namespace Syroot.NintenTools.Bfres.Switch
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
            if (loader.ResFile.VersionMajor2 == 9) {
                loader.ReadBytes(32); //reserved
            }
            resFile.SkeletalAnims = loader.LoadDictValues<SkeletalAnim>();
            resFile.MaterialAnims = loader.LoadDictValues<MaterialAnim>();
            resFile.BoneVisibilityAnims = loader.LoadDictValues<VisibilityAnim>();
            resFile.ShapeAnims = loader.LoadDictValues<ShapeAnim>();
            resFile.SceneAnims = loader.LoadDictValues<SceneAnim>();
            resFile.MemoryPool = loader.Load<MemoryPool>();
            resFile.BufferInfo = loader.Load<BufferInfo>();
            resFile.ExternalFiles = loader.LoadDictValues<ExternalFile>();
            long padding1 = loader.ReadInt64();
            resFile.StringTable = loader.Load<StringTable>();
            uint StringPoolSize = loader.ReadUInt32();
            ushort numModel = loader.ReadUInt16();

            //Read models after buffer data
            resFile.Models = loader.LoadDictValues<Model>(modelDictOffset, modelOffset);

            if (loader.ResFile.VersionMajor2 == 9)
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
            uint padding2 = loader.ReadUInt16();
            uint padding3 = loader.ReadUInt32();

            resFile.Textures = new ResDict<TextureShared>();
            foreach (var ext in resFile.ExternalFiles) {
                Console.WriteLine("EXT " + ext.Key);
                if (ext.Key.Contains(".bntx")) 
                {
                    BntxFile bntx = new BntxFile(new MemoryStream(ext.Value.Data));
                    ext.Value.LoadedFileData = bntx;
                    foreach (var tex in bntx.Textures)
                        resFile.Textures.Add(tex.Name, new SwitchTexture(tex));
                }
            }
        }

        public static void Save(ResFileSwitchSaver saver, ResFile resFile)
        {
            if (resFile.Models.Count > 0 && resFile.Models.Values.Any(x => x.Shapes.Count > 0)) {
                resFile.MemoryPool = new MemoryPool();
                resFile.BufferInfo = new BufferInfo();
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

            if (saver.ResFile.VersionMajor2 == 9)
                saver.SaveRelocateEntryToSection(saver.Position, 15, 1, 0, ResFileSwitchSaver.Section1, "ResFile");
            else
                saver.SaveRelocateEntryToSection(saver.Position, 13, 1, 0, ResFileSwitchSaver.Section1, "ResFile");

            saver.SaveString(resFile.Name);
            resFile.ModelOffset = saver.SaveOffset();
            resFile.ModelDictOffset = saver.SaveOffset();

            //2 New sections
            if (saver.ResFile.VersionMajor2 == 9)
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
            if (saver.ResFile.VersionMajor2 == 9)
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

            if (saver.ResFile.VersionMajor2 == 9)
                saver.Seek(2); // padding
            else
                saver.Seek(6); // padding
        }
    }
}
