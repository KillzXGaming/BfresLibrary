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
    internal class MaterialParser
    {
        public static void Load(ResFileSwitchLoader loader, Material mat)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
                mat.Flags = loader.ReadEnum<MaterialFlags>(true);
            else
                loader.LoadHeaderBlock();

            mat.Name = loader.LoadString();

            if (loader.ResFile.VersionMajor2 >= 10)
            {
                MaterialParserV10.Load(loader, mat);
                return;
            }

            mat.RenderInfos = loader.LoadDictValues<RenderInfo>();
            mat.ShaderAssign = loader.Load<ShaderAssign>();
            long TextureArrayOffset = loader.ReadInt64();
            long TextureNameArray = loader.ReadInt64();
            long SamplerArrayOffset = loader.ReadInt64();
            mat.Samplers = loader.LoadDictValues<Sampler>();
            mat.ShaderParams = loader.LoadDictValues<ShaderParam>();
            long SourceParamOffset = loader.ReadInt64();
            mat.UserData = loader.LoadDictValues<UserData>();
            long VolatileFlagsOffset = loader.ReadInt64();
            long userPointer = loader.ReadInt64();
            long SamplerSlotArrayOffset = loader.ReadInt64();
            long TexSlotArrayOffset = loader.ReadInt64();
            if (loader.ResFile.VersionMajor2 < 9)
                mat.Flags = loader.ReadEnum<MaterialFlags>(true);
            ushort idx = loader.ReadUInt16();
            ushort numRenderInfo = loader.ReadUInt16();
            byte numTextureRef = loader.ReadByte();
            byte numSampler = loader.ReadByte();
            ushort numShaderParam = loader.ReadUInt16();
            ushort numShaderParamVolatile = loader.ReadUInt16();
            ushort sizParamSource = loader.ReadUInt16();
            ushort sizParamRaw = loader.ReadUInt16();
            ushort numUserData = loader.ReadUInt16();

            if (loader.ResFile.VersionMajor2 < 9)
                loader.ReadUInt32(); //Padding

            var textures = loader.LoadCustom(() => loader.LoadStrings(numTextureRef), (uint)TextureNameArray);

            mat.TextureRefs = new List<TextureRef>();
            if (textures != null) {
                foreach (var tex in textures)
                    mat.TextureRefs.Add(new TextureRef() { Name = tex });
            }

            //Add names to the value as switch does not store any
            foreach (var sampler in mat.Samplers) {
                sampler.Value.Name = sampler.Key;
            }

            mat.ShaderParamData = loader.LoadCustom(() => loader.ReadBytes(sizParamSource), (uint)SourceParamOffset);

            mat.VolatileFlags = loader.LoadCustom(() => loader.ReadBytes((int)Math.Ceiling(numShaderParam / 8f)), (uint)VolatileFlagsOffset);
            mat.TextureSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numTextureRef), (uint)SamplerSlotArrayOffset);
            mat.SamplerSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numSampler), (uint)TexSlotArrayOffset);
        }

        public static void Save(ResFileSwitchSaver saver, Material mat)
        {
            if (mat.VolatileFlags == null)
                mat.VolatileFlags = new byte[0];
            if (mat.TextureRefs == null)
                mat.TextureRefs = new List<TextureRef>();
            if (mat.ShaderParamData == null)
                mat.ShaderParamData = new byte[0];

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(mat.Flags, true);
            else
                saver.Seek(12);

            if (saver.ResFile.VersionMajor2 >= 10)
            {
                MaterialParserV10.Save(saver, mat);
                return;
            }

            saver.SaveRelocateEntryToSection(saver.Position, 15, 1, 0, ResFileSwitchSaver.Section1, "FMAT");
            saver.SaveString(mat.Name);
            mat.PosRenderInfoOffset = saver.SaveOffset();
            mat.PosRenderInfoDictOffset = saver.SaveOffset();
            mat.PosShaderAssignOffset = saver.SaveOffset();
            mat.PosTextureUnk1Offset = saver.SaveOffset();
            mat.PosTextureRefsOffset = saver.SaveOffset();
            mat.PosTextureUnk2Offset = saver.SaveOffset();
            mat.PosSamplersOffset = saver.SaveOffset();
            mat.PosSamplerDictOffset = saver.SaveOffset();
            mat.PosShaderParamsOffset = saver.SaveOffset();
            mat.PosShaderParamDictOffset = saver.SaveOffset();
            mat.PosShaderParamDataOffset = saver.SaveOffset();
            mat.PosUserDataMaterialOffset = saver.SaveOffset();
            mat.PosUserDataDictMaterialOffset = saver.SaveOffset();
            mat.PosVolatileFlagsOffset = saver.SaveOffset();
            saver.Write((long)0);

            //Set the slot offsets for both sampler and texture
            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSwitchSaver.Section1, "Material texture slots");
            mat.PosSamplerSlotArrayOffset = saver.SaveOffset();
            mat.PosTextureSlotArrayOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 != 9)
                saver.Write(mat.Flags, true);
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write((ushort)mat.RenderInfos.Count);
            saver.Write((byte)mat.Samplers.Count);
            saver.Write((byte)mat.TextureRefs.Count);
            saver.Write((ushort)mat.ShaderParams.Count);
            saver.Write((ushort)0); //VolatileFlags.Length
            saver.Write((ushort)mat.ShaderParamData.Length);
            saver.Write((ushort)0); // SizParamRaw
            saver.Write((ushort)mat.UserData.Count);

            if (saver.ResFile.VersionMajor2 != 9)
                saver.Write(0); // padding
        }
    }
}
