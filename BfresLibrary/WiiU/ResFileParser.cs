using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary.Core;
using Syroot.BinaryData;
using BfresLibrary.WiiU.Core;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace BfresLibrary.WiiU
{
    public class ResFileParser
    {
        public static void Load(ResFileLoader loader, ResFile resFile)
        {
            loader.CheckSignature("FRES");
            resFile.Version = loader.ReadUInt32();
            resFile.SetVersionInfo(resFile.Version);
            resFile.ByteOrder = loader.ReadByteOrder();
            ushort sizHeader = loader.ReadUInt16();
            uint sizFile = loader.ReadUInt32();
            resFile.Alignment = loader.ReadUInt32();
            resFile.Name = loader.LoadString();
            uint sizStringPool = loader.ReadUInt32();
            uint ofsStringPool = loader.ReadOffset();
            resFile.Models = loader.LoadDict<Model>();
            var textures = loader.LoadDict<Texture>();
            resFile.SkeletalAnims = loader.LoadDict<SkeletalAnim>();
            resFile.ShaderParamAnims = loader.LoadDict<MaterialAnim>();
            resFile.ColorAnims = loader.LoadDict<MaterialAnim>();
            resFile.TexSrtAnims = loader.LoadDict<MaterialAnim>();
            resFile.TexPatternAnims = loader.LoadDict<MaterialAnim>();
            resFile.BoneVisibilityAnims = loader.LoadDict<VisibilityAnim>();
            loader.LoadDict<VisibilityAnim>();
            resFile.ShapeAnims = loader.LoadDict<ShapeAnim>();

            resFile.Textures = new ResDict<TextureShared>();
            foreach (var tex in textures)
                resFile.Textures.Add(tex.Key, tex.Value);

            if (loader.ResFile.Version >= 0x02040000) 
            { 
                resFile.SceneAnims = loader.LoadDict<SceneAnim>();
                resFile.ExternalFiles = loader.LoadDict<ExternalFile>();
                ushort numModel = loader.ReadUInt16();
                ushort numTexture = loader.ReadUInt16();
                ushort numSkeletalAnim = loader.ReadUInt16();
                ushort numShaderParamAnim = loader.ReadUInt16();
                ushort numColorAnim = loader.ReadUInt16();
                ushort numTexSrtAnim = loader.ReadUInt16();
                ushort numTexPatternAnim = loader.ReadUInt16();
                ushort numBoneVisibilityAnim = loader.ReadUInt16();
                ushort numMatVisibilityAnim = loader.ReadUInt16();
                ushort numShapeAnim = loader.ReadUInt16();
                ushort numSceneAnim = loader.ReadUInt16();
                ushort numExternalFile = loader.ReadUInt16();
                uint userPointer = loader.ReadUInt32();
            }
            else //Note very old versions have no counts and is mostly unkown atm
            {
                uint userPointer = loader.ReadUInt32();
                uint userPointer2 = loader.ReadUInt32();

                resFile.SceneAnims = loader.LoadDict<SceneAnim>();
                resFile.ExternalFiles = loader.LoadDict<ExternalFile>();
            }
        }

        public static void Save(ResFileWiiUSaver saver, ResFile resFile)
        {
            saver.WriteSignature("FRES");
            saver.Write(resFile.Version);
            saver.Write(resFile.ByteOrder, true);
            saver.Write((ushort)0x0010); // SizHeader
            saver.SaveFieldFileSize();
            saver.Write(resFile.Alignment);
            saver.SaveString(resFile.Name);
            saver.SaveFieldStringPool();
            saver.SaveDict(resFile.Models);
            saver.SaveDict(resFile.Textures);
            resFile.SkeletonAnimationOffset = saver.SaveOffsetPos();
            saver.SaveDict(resFile.ShaderParamAnims);
            saver.SaveDict(resFile.ColorAnims);
            saver.SaveDict(resFile.TexSrtAnims);
            saver.SaveDict(resFile.TexPatternAnims);
            saver.SaveDict(resFile.BoneVisibilityAnims);
            saver.SaveDict(resFile.MatVisibilityAnims);
            saver.SaveDict(resFile.ShapeAnims);
            saver.SaveDict(resFile.SceneAnims);
            saver.SaveDict(resFile.ExternalFiles);
            saver.Write((ushort)resFile.Models.Count);
            saver.Write((ushort)resFile.Textures.Count);
            saver.Write((ushort)resFile.SkeletalAnims.Count);
            saver.Write((ushort)resFile.ShaderParamAnims.Count);
            saver.Write((ushort)resFile.ColorAnims.Count);
            saver.Write((ushort)resFile.TexSrtAnims.Count);
            saver.Write((ushort)resFile.TexPatternAnims.Count);
            saver.Write((ushort)resFile.BoneVisibilityAnims.Count);
            saver.Write((ushort)resFile.MatVisibilityAnims.Count);
            saver.Write((ushort)resFile.ShapeAnims.Count);
            saver.Write((ushort)resFile.SceneAnims.Count);
            saver.Write((ushort)resFile.ExternalFiles.Count);
            saver.Write(0); // UserPointer
        }
    }
}
