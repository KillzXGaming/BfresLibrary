using System;
using System.Collections.Generic;
using System.Diagnostics;
using BfresLibrary.Core;
using System.IO;
using System.ComponentModel;
using System.Linq;
using Syroot.BinaryData;
using BfresLibrary.Switch;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FMAT subsection of a <see cref="Model"/> subfile, storing information on with which textures and
    /// how technically a surface is drawn.
    /// </summary>
    [DebuggerDisplay(nameof(Material) + " {" + nameof(Name) + "}")]
    public class Material : IResData, IBinarySection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class.
        /// </summary>
        public Material()
        {
            Name = "";
            Flags = MaterialFlags.Visible;

            ShaderAssign = new ShaderAssign();

            RenderInfos = new ResDict<RenderInfo>();
            TextureRefs = new List<TextureRef>();
            Samplers = new ResDict<Sampler>();
            UserData = new ResDict<UserData>();
            ShaderParams = new ResDict<ShaderParam>();

            ShaderParamData = new byte[0];
            VolatileFlags = new byte[0];
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FMAT";

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Material}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets flags specifying how a <see cref="Material"/> is rendered.
        /// </summary>
        [Browsable(false)]
        public MaterialFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the visible flag to display the material.
        /// </summary>
        public bool Visible
        {
            get { return Flags.HasFlag(MaterialFlags.Visible); }
            set
            {
                if (value)
                    Flags |= MaterialFlags.Visible;
                else
                    Flags = MaterialFlags.None;
            }
        }

        [Browsable(false)]
        public ResDict<RenderInfo> RenderInfos { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("Rendering")]
        [DisplayName("Render State")]
        public RenderState RenderState { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("Shader Assign")]
        public ShaderAssign ShaderAssign { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MaterialParserV10.ShaderInfo ShaderInfoV10 { get; set; }

        internal int RenderInfoSize;

        /// <summary>
        /// Gets or sets the list of <see cref="TextureRef"/> instances referencing the <see cref="Texture"/> instances
        /// required to draw the material.
        /// </summary>
        [Browsable(false)]
        public IList<TextureRef> TextureRefs { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of <see cref="Sampler"/> instances which configure how to draw
        /// <see cref="Texture"/> instances referenced by the <see cref="TextureRefs"/> list.
        /// </summary>
        [Browsable(false)]
        public ResDict<Sampler> Samplers { get; set; }

        [Browsable(false)]
        public ResDict<ShaderParam> ShaderParams { get; set; }

        /// <summary>
        /// Gets or sets the raw data block which stores <see cref="ShaderParam"/> values.
        /// </summary>
        [Browsable(false)]
        public byte[] ShaderParamData { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict<UserData> UserData { get; set; }

        /// <summary>
        /// Gets or sets a set of bits determining whether <see cref="ShaderParam"/> instances are volatile.
        /// </summary>
        // TODO: Wrap into a bool array.

        [Browsable(false)]
        public byte[] VolatileFlags { get; set; }

        // TODO: Methods to access ShaderParam variable values.

        internal long[] TextureSlotArray { get; set; }
        internal long[] SamplerSlotArray { get; set; }

        internal int[] ParamIndices { get; set; }

        private long[] FillSlots(int count)
        {
            long[] slots = new long[count];
            for (int i = 0; i < count; i++)
                slots[i] = -1;
            return slots;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public void Import(Stream stream, ResFile ResFile) {
            ResFileLoader.ImportSection(stream, this, ResFile);
        }

        public void Import(string FileName, ResFile ResFile) {
            string ext = Path.GetExtension(FileName);
            if (ext == ".json")
                TextConvert.MaterialConvert.FromJson(this, File.ReadAllText(FileName));
            else
                ResFileLoader.ImportSection(FileName, this, ResFile);
        }

        public void Export(string FileName, ResFile ResFile) {
            var model = ResFile.Models[0];

            string ext = Path.GetExtension(FileName);
            if (ext == ".json")
                File.WriteAllText(FileName, TextConvert.MaterialConvert.ToJson(this, model));
            else
                ResFileSaver.ExportSection(FileName, this, ResFile);
        }


        /// <summary>
        /// Sets a value directly to render info given the name and value.
        /// Render info entry is added if none with the given name exists.
        /// Value can be type of int, string, float, or an array of those 3.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetRenderInfo(string name, object value)
        {
            var renderInfo = new RenderInfo();
            renderInfo.Name = name;
            if (value is int) renderInfo.SetValue(new int[1] { (int)value });
            else if (value is float) renderInfo.SetValue(new float[1] { (float)value });
            else if (value is string) renderInfo.SetValue(new string[1] { (string)value });
            else if (value is int[]) renderInfo.SetValue((int[])value);
            else if (value is float[]) renderInfo.SetValue((float[])value);
            else if (value is string[]) renderInfo.SetValue((string[])value);

            if (!RenderInfos.ContainsKey(name))
                RenderInfos.Add(name, renderInfo);
            else
                RenderInfos[name] = renderInfo;
        }

        /// <summary>
        /// Sets a value directly to user data given the name and value.
        /// User data entry is added if none with the given name exists.
        /// Value can be type of int, string, float, or an array of those 3.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="isUnicode"></param>
        public void SetUserData(string name, object value, bool isUnicode = false)
        {
            var userData = new UserData();
            userData.Name = name;
            if (value is int) userData.SetValue(new int[1] { (int)value });
            else if (value is float) userData.SetValue(new float[1] { (float)value });
            else if (value is string) userData.SetValue(new string[1] { (string)value }, isUnicode);
            else if (value is int[]) userData.SetValue((int[])value);
            else if (value is float[]) userData.SetValue((float[])value);
            else if (value is string[]) userData.SetValue((string[])value, isUnicode);

            if (!UserData.ContainsKey(name))
                UserData.Add(name, userData);
            else
                UserData[name] = userData;
        }

        public void SetShaderParameter(string name, ShaderParamType type, object value)
        {
            ShaderParam param = new ShaderParam();
            param.Name = name;
            param.DataValue = value;
            param.Type = type;

            if (!ShaderParams.ContainsKey(name))
                ShaderParams.Add(name, param);
            else
                ShaderParams[name] = param;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.IsSwitch) {
                Switch.MaterialParser.Load((Switch.Core.ResFileSwitchLoader)loader, this);
            }
            else
            {
                Name = loader.LoadString();
                Flags = loader.ReadEnum<MaterialFlags>(true);
                ushort idx = loader.ReadUInt16();
                ushort numRenderInfo = loader.ReadUInt16();
                byte numSampler = loader.ReadByte();
                byte numTextureRef = loader.ReadByte();
                ushort numShaderParam = loader.ReadUInt16();
                ushort numShaderParamVolatile = loader.ReadUInt16();
                ushort sizParamSource = loader.ReadUInt16();
                ushort sizParamRaw = loader.ReadUInt16();
                ushort numUserData = loader.ReadUInt16();
                RenderInfos = loader.LoadDict<RenderInfo>();
                RenderState = loader.Load<RenderState>();
                ShaderAssign = loader.Load<ShaderAssign>();
                TextureRefs = loader.LoadList<TextureRef>(numTextureRef);
                uint ofsSamplerList = loader.ReadOffset(); // Only use dict.
                Samplers = loader.LoadDict<Sampler>();
                uint ofsShaderParamList = loader.ReadOffset(); // Only use dict.
                ShaderParams = loader.LoadDict<ShaderParam>();
                ShaderParamData = loader.LoadCustom(() => loader.ReadBytes(sizParamSource));
                UserData = loader.LoadDict<UserData>();
                VolatileFlags = loader.LoadCustom(() => loader.ReadBytes((int)Math.Ceiling(numShaderParam / 8f)));
                uint userPointer = loader.ReadUInt32();
            }

            ReadShaderParams(ShaderParamData, loader.IsSwitch ? ByteOrder.LittleEndian: ByteOrder.BigEndian);
        }

        void IResData.Save(ResFileSaver saver)
        {
            ShaderParamData = WriteShaderParams(saver.IsSwitch ? ByteOrder.LittleEndian : ByteOrder.BigEndian);

            TextureSlotArray = FillSlots(TextureRefs.Count);
            SamplerSlotArray = FillSlots(Samplers.Count);

            saver.WriteSignature(_signature);
            if (saver.IsSwitch) {
                Switch.MaterialParser.Save((Switch.Core.ResFileSwitchSaver)saver, this);
            }
            else
            {
                if (VolatileFlags == null)
                    VolatileFlags = new byte[0];

                saver.SaveString(Name);
                saver.Write(Flags, true);
                saver.Write((ushort)saver.CurrentIndex);
                saver.Write((ushort)RenderInfos.Count);
                saver.Write((byte)Samplers.Count);
                saver.Write((byte)TextureRefs.Count);
                saver.Write((ushort)ShaderParams.Count);
                if (saver.ResFile.Version >= 0x03030000)
                    saver.Write((ushort)VolatileFlags.Length);
                else
                    saver.Write((ushort)TextureRefs.Count);
                saver.Write((ushort)ShaderParamData.Length);
                saver.Write((ushort)0); // SizParamRaw
                saver.Write((ushort)UserData.Count);
                saver.SaveDict(RenderInfos);
                saver.Save(RenderState);
                saver.Save(ShaderAssign);
                saver.SaveList(TextureRefs);
                saver.SaveList(Samplers.Values);
                saver.SaveDict(Samplers);
                saver.SaveList(ShaderParams.Values);
                saver.SaveDict(ShaderParams);
                saver.SaveCustom(ShaderParamData, () => saver.Write(ShaderParamData));
                saver.SaveDict(UserData);
                if (saver.ResFile.Version >= 0x03030000)
                    saver.SaveCustom(VolatileFlags, () => saver.Write(VolatileFlags));
                saver.Write(0); // UserPointer
            }
        }

        private void ReadShaderParams(byte[] data, ByteOrder byteOrder)
        {
            if (data == null)
                return;

            using (var reader = new BinaryDataReader(new MemoryStream(data))) {
                reader.ByteOrder = byteOrder;
                foreach (var param in ShaderParams.Values)
                {
                    reader.BaseStream.Seek(param.DataOffset, SeekOrigin.Begin);
                    param.DataValue = ReadParamData(param.Type, reader);
                }
            }
        }

        private object ReadParamData(ShaderParamType type, BinaryDataReader reader)
        {
            switch (type)
            {
                case ShaderParamType.Bool: return reader.ReadBoolean();
                case ShaderParamType.Bool2: return reader.ReadBooleans(2);
                case ShaderParamType.Bool3: return reader.ReadBooleans(3);
                case ShaderParamType.Bool4: return reader.ReadBooleans(4);
                case ShaderParamType.Float: return reader.ReadSingle();
                case ShaderParamType.Float2: return reader.ReadSingles(2);
                case ShaderParamType.Float2x2: return reader.ReadSingles(2 * 2);
                case ShaderParamType.Float2x3: return reader.ReadSingles(2 * 3);
                case ShaderParamType.Float2x4: return reader.ReadSingles(2 * 4);
                case ShaderParamType.Float3: return reader.ReadSingles(3);
                case ShaderParamType.Float3x2: return reader.ReadSingles(3 * 2);
                case ShaderParamType.Float3x3: return reader.ReadSingles(3 * 3);
                case ShaderParamType.Float3x4: return reader.ReadSingles(3 * 4);
                case ShaderParamType.Float4: return reader.ReadSingles(4);
                case ShaderParamType.Float4x2: return reader.ReadSingles(4 * 2);
                case ShaderParamType.Float4x3: return reader.ReadSingles(4 * 3);
                case ShaderParamType.Float4x4: return reader.ReadSingles(4 * 4);
                case ShaderParamType.Int: return reader.ReadInt32();
                case ShaderParamType.Int2: return reader.ReadInt32s(2);
                case ShaderParamType.Int3: return reader.ReadInt32s(3);
                case ShaderParamType.Int4: return reader.ReadInt32s(4);
                case ShaderParamType.UInt: return reader.ReadInt32();
                case ShaderParamType.UInt2: return reader.ReadInt32s(2);
                case ShaderParamType.UInt3: return reader.ReadInt32s(3);
                case ShaderParamType.UInt4: return reader.ReadInt32s(4);
                case ShaderParamType.Reserved2: return reader.ReadBytes(2);
                case ShaderParamType.Reserved3: return reader.ReadBytes(3);
                case ShaderParamType.Reserved4: return reader.ReadBytes(4);
                case ShaderParamType.Srt2D:
                    return new Srt2D()
                    {
                        Scaling = reader.ReadVector2F(),
                        Rotation = reader.ReadSingle(),
                        Translation = reader.ReadVector2F(),
                    };
                case ShaderParamType.Srt3D:
                    return new Srt3D()
                    {
                        Scaling = reader.ReadVector3F(),
                        Rotation = reader.ReadVector3F(),
                        Translation = reader.ReadVector3F(),
                    };
                case ShaderParamType.TexSrt:
                case ShaderParamType.TexSrtEx:
                    return new TexSrt()
                    {
                        Mode = (TexSrtMode)reader.ReadInt32(),
                        Scaling = reader.ReadVector2F(),
                        Rotation = reader.ReadSingle(),
                        Translation = reader.ReadVector2F(),
                    };
            }
            return 0;
        }

        private byte[] WriteShaderParams(ByteOrder byteOrder)
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryDataWriter(mem)) {
                writer.ByteOrder = byteOrder;

                int index = 0;

                uint Offset = 0;
                foreach (var param in ShaderParams.Values) {
                    param.DataOffset = (ushort)Offset;
                    param.DependIndex = (ushort)index;
                    param.DependedIndex = (ushort)index;

                    writer.Seek(param.DataOffset, SeekOrigin.Begin);
                    WriteParamData(writer, param.Type, param.DataValue);

                    Offset += (param.DataSize + (uint)param.PaddingLength);
                    index++;
                }
            }
            return mem.ToArray();
        }

        private void WriteParamData(BinaryDataWriter writer, ShaderParamType type, object data)
        {
            if (data is float) writer.Write((float)data);
            else if (data is uint) writer.Write((uint)data);
            else if (data is int) writer.Write((int)data);
            else if (data is bool) writer.Write((bool)data);
            else if (data is float[]) writer.Write((float[])data);
            else if (data is bool[]) writer.Write((bool[])data);
            else if (data is uint[]) writer.Write((uint[])data);
            else if (data is int[]) writer.Write((int[])data);
            else if (data is Srt2D)
            {
                writer.Write(((Srt2D)data).Scaling);
                writer.Write(((Srt2D)data).Rotation);
                writer.Write(((Srt2D)data).Translation);
            }
            else if (data is Srt3D)
            {
                writer.Write(((Srt3D)data).Scaling);
                writer.Write(((Srt3D)data).Rotation);
                writer.Write(((Srt3D)data).Translation);
            }
            else if (data is TexSrt)
            {
                writer.Write(((TexSrt)data).Mode, false);
                writer.Write(((TexSrt)data).Scaling);
                writer.Write(((TexSrt)data).Rotation);
                writer.Write(((TexSrt)data).Translation);
                if (type == ShaderParamType.TexSrtEx)
                    writer.Write(0); //pointer at runtime
            }
        }

        internal long PosRenderInfoOffset;
        internal long PosRenderInfoDictOffset;
        internal long PosShaderAssignOffset;
        internal long PosTextureUnk1Offset;
        internal long PosTextureRefsOffset;
        internal long PosTextureUnk2Offset;
        internal long PosSamplersOffset;
        internal long PosSamplerDictOffset;
        internal long PosShaderParamsOffset;
        internal long PosShaderParamDictOffset;
        internal long PosShaderParamDataOffset;
        internal long PosUserDataMaterialOffset;
        internal long PosUserDataDictMaterialOffset;
        internal long PosVolatileFlagsOffset;
        internal long PosSamplerSlotArrayOffset;
        internal long PosTextureSlotArrayOffset;
    }

    /// <summary>
    /// Represents general flags specifying how a <see cref="Material"/> is rendered.
    /// </summary>
    public enum MaterialFlags : uint
    {
        /// <summary>
        /// The material is not rendered at all.
        /// </summary>
        None,

        /// <summary>
        /// The material is rendered.
        /// </summary>
        Visible
    }
}