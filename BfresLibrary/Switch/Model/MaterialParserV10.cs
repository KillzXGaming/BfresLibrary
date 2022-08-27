using BfresLibrary.Core;
using BfresLibrary.Switch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BfresLibrary.Switch
{
    internal class MaterialParserV10
    {
        public static void Load(ResFileSwitchLoader loader, Material mat)
        {
            //V10 changes quite alot....

            //First change is a new struct with shader assign + tables for shader assign data
            var info = loader.Load<ShaderInfo>();
            long TextureArrayOffset = loader.ReadInt64();
            long TextureNameArray = loader.ReadInt64();
            long SamplerArrayOffset = loader.ReadInt64();
            mat.Samplers = loader.LoadDictValues<Sampler>();
            //Next is table data
            long renderInfoDataTable = loader.ReadInt64();
            long renderInfoCounterTable = loader.ReadInt64();
            long renderInfoBytesTable = loader.ReadInt64(); //WTF is this. Matches render info count though..
            long SourceParamOffset = loader.ReadInt64();
            long SourceParamIndices = loader.ReadInt64(); //0xFFFF a bunch per param. Set at runtime??
            loader.ReadUInt64(); //0
            mat.UserData = loader.LoadDictValues<UserData>();
            long VolatileFlagsOffset = loader.ReadInt64();
            long userPointer = loader.ReadInt64();
            long SamplerSlotArrayOffset = loader.ReadInt64();
            long TexSlotArrayOffset = loader.ReadInt64();
            ushort idx = loader.ReadUInt16();
            byte numTextureRef = loader.ReadByte();
            byte numSampler = loader.ReadByte();
            ushort numShaderParamVolatile = loader.ReadUInt16(); //idk
            ushort numUserData = loader.ReadUInt16();
            ushort sizParamRaw = loader.ReadUInt16();
            loader.ReadUInt16(); //0
            loader.ReadUInt16(); //0
            loader.ReadUInt16(); //0

            var textures = loader.LoadCustom(() => loader.LoadStrings(numTextureRef), (uint)TextureNameArray);

            mat.TextureRefs = new List<TextureRef>();
            if (textures != null)
            {
                foreach (var tex in textures)
                    mat.TextureRefs.Add(new TextureRef() { Name = tex });
            }

            //Add names to the value as switch does not store any
            foreach (var sampler in mat.Samplers)
                sampler.Value.Name = sampler.Key;

            mat.TextureSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numTextureRef), (uint)SamplerSlotArrayOffset);
            mat.SamplerSlotArray = loader.LoadCustom(() => loader.ReadInt64s(numSampler), (uint)TexSlotArrayOffset);

            mat.ShaderAssign = new ShaderAssign()
            {
                ShaderArchiveName = info.ShaderAssign.ShaderArchiveName,
                ShadingModelName = info.ShaderAssign.ShadingModelName,
            };
        }

        public static void Save(ResFileSwitchSaver saver, Material mat)
        {

        }

        class ShaderInfo : IResData
        {
            public ShaderAssignV10 ShaderAssign;

            void IResData.Load(ResFileLoader loader)
            {
                ShaderAssign = loader.Load<ShaderAssignV10>();
                long attribAssignOffset = loader.ReadInt64();
                loader.ReadUInt64(); //attribute pointer?
                long samplerAssignOffset = loader.ReadInt64();
                loader.ReadUInt64(); //sampler pointer?
                ulong optionChoiceToggleOffset = loader.ReadUInt64();
                ulong optionChoiceValuesOffset = loader.ReadUInt64();
                loader.ReadUInt64(); //padding
                loader.ReadUInt32(); //padding
                byte numAttributeAssign = loader.ReadByte();
                byte numSamplerAssign = loader.ReadByte();
                byte unk1 = loader.ReadByte();
                byte unk2 = loader.ReadByte();
                byte shaderOptionCount = loader.ReadByte();

                var attribAssigns = loader.LoadList<ResString>(numAttributeAssign, (uint)attribAssignOffset);
                var samplerAssigns = loader.LoadList<ResString>(numAttributeAssign, (uint)samplerAssignOffset);
            }

            void IResData.Save(ResFileSaver saver)
            {

            }
        }

        class ShaderAssignV10 : IResData
        {
            public ResDict<ResString> RenderInfos = new ResDict<ResString>();
            public ResDict<ResString> ShaderParameters = new ResDict<ResString>();

            public string ShaderArchiveName;
            public string ShadingModelName;

            void IResData.Load(ResFileLoader loader)
            {
                ShaderArchiveName = loader.LoadString();
                ShadingModelName = loader.LoadString();

                //List of names + type. Data in material section
                ulong renderInfoListOffset = loader.ReadUInt64();
                RenderInfos = loader.LoadDict<ResString>();
                //List of names + type. Data in material section
                ulong shaderParamOffset = loader.ReadUInt64();
                ShaderParameters = loader.LoadDict<ResString>();

            }
            void IResData.Save(ResFileSaver saver)
            {

            }
        }
    }

}
