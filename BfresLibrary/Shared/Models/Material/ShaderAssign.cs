using BfresLibrary.Core;
using System.ComponentModel;
using System.Collections.Generic;

namespace BfresLibrary
{
    public class ShaderAssign : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAssign"/> class.
        /// </summary>
        public ShaderAssign()
        {
            ShaderArchiveName = "";
            ShadingModelName = "";
            Revision = 0;
            AttribAssigns = new ResDict<ResString>();
            SamplerAssigns = new ResDict<ResString>();
            ShaderOptions = new ResDict<ResString>();
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public string ShaderArchiveName { get; set; }

        public string ShadingModelName { get; set; }

        public uint Revision { get; set; }

        public ResDict<ResString> AttribAssigns { get; set; }
        public ResDict<ResString> SamplerAssigns { get; set; }
        public ResDict<ResString> ShaderOptions { get; set; }

        public List<ResStringDisplay> ShaderOptionsList
        {
            get
            {
                List<ResStringDisplay> strings = new List<ResStringDisplay>();
                foreach (var option in ShaderOptions)
                    strings.Add(new ResStringDisplay()
                    {
                        Name = option.Key,
                        Value = option.Value,
                    });

                return strings;
            }
            set
            {
                ShaderOptions.Clear();

                foreach (var option in value)
                    ShaderOptions.Add(option.Name, option.Value);
            }
        }

        public class ResStringDisplay
        {
            [Category("Shader Data")]
            [DisplayName("Name")]
            public string Name { get; set; }

            [Category("Shader Data")]
            [DisplayName("Value")]
            public string Value { get; set; }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            ShaderArchiveName = loader.LoadString();
            ShadingModelName = loader.LoadString();

            if (loader.IsSwitch)
            {
                AttribAssigns = loader.LoadDictValues<ResString>();
                SamplerAssigns = loader.LoadDictValues<ResString>();
                ShaderOptions = loader.LoadDictValues<ResString>();
                Revision = loader.ReadUInt32();
                byte numAttribAssign = loader.ReadByte();
                byte numSamplerAssign = loader.ReadByte();
                ushort numShaderOption = loader.ReadUInt16();
            }
            else
            {
                Revision = loader.ReadUInt32();
                byte numAttribAssign = loader.ReadByte();
                byte numSamplerAssign = loader.ReadByte();
                ushort numShaderOption = loader.ReadUInt16();
                AttribAssigns = loader.LoadDict<ResString>();
                SamplerAssigns = loader.LoadDict<ResString>();
                ShaderOptions = loader.LoadDict<ResString>();
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                saver.SaveString(ShaderArchiveName);
                saver.SaveString(ShadingModelName);
                PosAttribAssigns = saver.SaveOffset();
                PosAttribAssignDict = saver.SaveOffset();
                PosSamplerAssigns = saver.SaveOffset();
                PosSamplerAssignDict = saver.SaveOffset();
                PosShaderOptions = saver.SaveOffset();
                PosShaderOptionsDict = saver.SaveOffset();
                saver.Write(Revision);
                saver.Write((byte)AttribAssigns.Count);
                saver.Write((byte)SamplerAssigns.Count);
                saver.Write((ushort)ShaderOptions.Count);
            }
            else
            {
                saver.SaveString(ShaderArchiveName);
                saver.SaveString(ShadingModelName);
                saver.Write(Revision);
                saver.Write((byte)AttribAssigns.Count);
                saver.Write((byte)SamplerAssigns.Count);
                saver.Write((ushort)ShaderOptions.Count);
                saver.SaveDict(AttribAssigns);
                saver.SaveDict(SamplerAssigns);
                saver.SaveDict(ShaderOptions);
            }
        }

        internal long PosAttribAssigns;
        internal long PosAttribAssignDict;
        internal long PosSamplerAssigns;
        internal long PosSamplerAssignDict;
        internal long PosShaderOptions;
        internal long PosShaderOptionsDict;
    }
}