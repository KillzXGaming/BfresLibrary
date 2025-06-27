using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BfresLibrary.Core;
using BfresLibrary.TextConvert;
using System.ComponentModel;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FMAA section in a <see cref="ResFile"/> subfile, storing material animation data.
    /// </summary>
    [DebuggerDisplay(nameof(MaterialAnim) + " {" + nameof(Name) + "}")]
    public class MaterialAnim : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAnim"/> class.
        /// </summary>
        public MaterialAnim()
        {
            Name = "";
            Path = "";
            BindModel = new Model();
            BindIndices = new ushort[0];
            TextureNames = new ResDict<TextureRef>();
            TextureBindArray = new List<long>();

            Flags = 0;
            FrameCount = 1;
            BakedSize = 0;

            MaterialAnimDataList = new List<MaterialAnimData>();
            UserData = new ResDict<UserData>();
        }

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal string signature;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{MaterialAnim}"/>
        /// instances.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Frame Count")]
        public int FrameCount { get; set; }

        [Browsable(true)]
        [Category("Animation")]
        public bool Loop
        {
            get
            {
                return Flags.HasFlag(MaterialAnimFlags.Looping);
            }
            set
            {
                if (value == true)
                    Flags |= MaterialAnimFlags.Looping;
                else
                    Flags &= ~MaterialAnimFlags.Looping;
            }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Baked
        {
            get
            {
                return Flags.HasFlag(MaterialAnimFlags.BakedCurve);
            }
            set
            {
                if (value == true)
                    Flags |= MaterialAnimFlags.BakedCurve;
                else
                    Flags &= ~MaterialAnimFlags.BakedCurve;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        [Browsable(false)]
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets the indices of the <see cref="Material"/> instances in the <see cref="Model.Materials"/> dictionary to
        /// bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<UserData> UserData { get; set; }

        [Browsable(false)]
        public IList<MaterialAnimData> MaterialAnimDataList { get; set; } = new List<MaterialAnimData>();

        [Browsable(false)]
        public ResDict<TextureRef> TextureNames { get; set; }

        [Browsable(false)]
        public IList<long> TextureBindArray { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlags"/> mode used to control looping and baked settings.
        /// </summary>
        [Browsable(false)]
        public MaterialAnimFlags Flags { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public void Import(Stream stream, ResFile ResFile)
        {
            ResFileLoader.ImportSection(stream, this, ResFile);
        }

        public void Import(string FileName, ResFile ResFile) {
            if (FileName.EndsWith(".json"))
                MaterialAnimConvert.FromJson(this, File.ReadAllText(FileName));
            else
                ResFileLoader.ImportSection(FileName, this, ResFile);
        }

        public void Export(string FileName, ResFile ResFile) {
            if (FileName.EndsWith(".json"))
                File.WriteAllText(FileName, MaterialAnimConvert.ToJson(this));
            else
                ResFileSaver.ExportSection(FileName, this, ResFile);
        }

        private uint UnknownValue;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            signature = loader.ReadString(4, Encoding.ASCII);
            uint materialAnimOffset = 0;
            ushort materialCount = 0;

            if (signature == "FMAA")
            {
                if (loader.ResFile.VersionMajor2 >= 9)
                {
                    Flags = loader.ReadEnum<MaterialAnimFlags>(true);
                    loader.ReadUInt16();
                }
                else
                    ((Switch.Core.ResFileSwitchLoader)loader).LoadHeaderBlock();

                Name = loader.LoadString();
                Path = loader.LoadString();
                BindModel = loader.Load<Model>(true);
                uint BindIndicesOffset = loader.ReadOffset();
                materialAnimOffset = loader.ReadOffset();
                uint unk = loader.ReadOffset(); //Empty section. Maybe set at runtime
                uint TextureNameArrayOffset = loader.ReadOffset();
                UserData = loader.LoadDictValues<UserData>();
                uint TextureBindArrayOffset = loader.ReadOffset();

                if (loader.ResFile.VersionMajor2 < 9)
                    Flags = loader.ReadEnum<MaterialAnimFlags>(true);

                ushort numUserData = 0;
                ushort CurveCount = 0;

                if (loader.ResFile.VersionMajor2 >= 9)
                {
                    FrameCount = loader.ReadInt32();
                    BakedSize = loader.ReadUInt32();
                    numUserData = loader.ReadUInt16();
                    materialCount = loader.ReadUInt16();
                    CurveCount = loader.ReadUInt16();
                }
                else
                {
                    numUserData = loader.ReadUInt16();
                    materialCount = loader.ReadUInt16();
                    CurveCount = loader.ReadUInt16();
                    FrameCount = loader.ReadInt32();
                    BakedSize = loader.ReadUInt32();
                }

                ushort ShaderParamAnimCount = loader.ReadUInt16();
                ushort TexturePatternAnimCount = loader.ReadUInt16();
                ushort VisabiltyAnimCount = loader.ReadUInt16();
                ushort TextureCount = loader.ReadUInt16();

                if (loader.ResFile.VersionMajor2 >= 9)
                    loader.ReadUInt16(); //padding

                BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(materialCount), BindIndicesOffset);
                var textureList = loader.LoadCustom(() => loader.LoadStrings(TextureCount), TextureNameArrayOffset);
                TextureBindArray = loader.LoadCustom(() => loader.ReadInt64s(TextureCount), TextureBindArrayOffset);

                if (textureList == null) textureList = new List<string>();

                foreach (var tex in textureList)
                {
                    if (TextureNames.ContainsKey(tex))
                        TextureNames.Add(tex + TextureNames.Count, new TextureRef() { Name = tex });
                    else
                        TextureNames.Add(tex, new TextureRef() { Name = tex });
                }
            }
            else if (signature == "FSHU")
            {
                if (loader.ResFile.Version >= 0x02040000)
                {
                    Name = loader.LoadString();
                    Path = loader.LoadString();
                    Flags = (MaterialAnimFlags)loader.ReadUInt32();

                    if (loader.ResFile.Version >= 0x03040000)
                    {
                        FrameCount = loader.ReadInt32();
                        materialCount = loader.ReadUInt16();
                        ushort numUserData = loader.ReadUInt16();
                        int numParamAnim = loader.ReadInt32();
                        int numCurve = loader.ReadInt32();
                        BakedSize = loader.ReadUInt32();
                    }
                    else
                    {
                        FrameCount = loader.ReadUInt16();
                        materialCount = loader.ReadUInt16();
                        UnknownValue = loader.ReadUInt32();
                        int numCurve = loader.ReadInt32();
                        BakedSize = loader.ReadUInt32();
                        int padding2 = loader.ReadInt32();
                    }
                    BindModel = loader.Load<Model>();
                    BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(materialCount));
                    materialAnimOffset = loader.ReadOffset();
                    UserData = loader.LoadDict<UserData>();
                }
                else
                {
                    Flags = (MaterialAnimFlags)loader.ReadUInt32();
                    FrameCount = loader.ReadInt16();
                     materialCount = loader.ReadUInt16();
                    ushort numUserData = loader.ReadUInt16();
                    ushort unk = loader.ReadUInt16();
                    BakedSize = loader.ReadUInt32();
                    Name = loader.LoadString();
                    Path = loader.LoadString();
                    BindModel = loader.Load<Model>();
                    BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(materialCount));
                    materialAnimOffset = loader.ReadOffset();
                }
            }
            else if (signature == "FTXP")
            {
                Name = loader.LoadString();
                Path = loader.LoadString();
                Flags = loader.ReadEnum<MaterialAnimFlags>(true);
                ushort numTextureRef = 0;
                if (loader.ResFile.Version >= 0x03040000)
                {
                    ushort numUserData = loader.ReadUInt16();
                    FrameCount = loader.ReadInt32();
                    numTextureRef = loader.ReadUInt16();
                    materialCount = loader.ReadUInt16();
                    int numPatAnim = loader.ReadInt32();
                    int numCurve = loader.ReadInt32();
                    BakedSize = loader.ReadUInt32();
                }
                else
                {
                    FrameCount = loader.ReadUInt16();
                    numTextureRef = loader.ReadUInt16();
                    materialCount = loader.ReadUInt16();
                    ushort numUserData = loader.ReadUInt16();
                    int numPatAnim = loader.ReadInt16();
                    int numCurve = loader.ReadInt32();
                    BakedSize = loader.ReadUInt32();
                    loader.Seek(4); //padding
                }


                BindModel = loader.Load<Model>();
                BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(materialCount));
                materialAnimOffset = loader.ReadOffset();
                if (loader.ResFile.Version >= 0x03040000)
                    TextureNames = loader.LoadDict<TextureRef>();
                else
                {
                    int TextureCount = 0;
                    foreach (var patternAnim in MaterialAnimDataList)
                    {
                        foreach (var curve in patternAnim.Curves)
                        {
                            List<uint> frames = new List<uint>();
                            foreach (float key in curve.Keys)
                                frames.Add((uint)key);
                            TextureCount = (short)frames.Max();
                        }
                    }
                    var TextureRefNames = loader.LoadList<TextureRef>(numTextureRef);
                    foreach (var texRef in TextureRefNames)
                        TextureNames.Add(texRef.Name, texRef);
                }
                UserData = loader.LoadDict<UserData>();
            }

            //Load materials and parse based on the signature of the section
            MaterialAnimDataList = loader.LoadCustom(() =>
            {
                List<MaterialAnimData> materialAnims = new List<MaterialAnimData>();
                for (int i = 0; i < materialCount; i++)
                    materialAnims.Add(new MaterialAnimData(loader, signature));
                return materialAnims;
            }, materialAnimOffset);

            if (MaterialAnimDataList == null)
                MaterialAnimDataList = new List<MaterialAnimData>();
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (MaterialAnimDataList == null)
                MaterialAnimDataList = new List<MaterialAnimData>();

            saver.WriteSignature(signature);
            if (signature == "FMAA")
            {
                if (saver.ResFile.VersionMajor2 >= 9)
                {
                    saver.Write(Flags, true);
                    saver.Seek(2);
                }
                else
                    saver.Seek(12);

                if (TextureBindArray == null || TextureNames.Count != TextureBindArray.Count)
                {
                    TextureBindArray = new long[TextureNames.Count].ToList();
                    for (int i = 0; i < TextureNames.Count; i++)
                        TextureBindArray[i] = -1;
                }

                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 2, 1, 0, 1, "Material Animation");
                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Write(0L); //Bind Model
                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 7, 1, 0, 1, "Material Animation");
                PosBindIndicesOffset = saver.SaveOffset();
                PosMatAnimDataOffset = saver.SaveOffset();
                PosTextureNamesUnkOffset = saver.SaveOffset();
                PosTextureNamesOffset = saver.SaveOffset();
                PosUserDataOffset = saver.SaveOffset();
                PosUserDataDictOffset = saver.SaveOffset();
                PosTextureBindArrayOffset = saver.SaveOffset();
                if (saver.ResFile.VersionMajor2 < 9)
                    saver.Write(Flags, true);

                if (saver.ResFile.VersionMajor2 >= 9)
                {
                    saver.Write(FrameCount);
                    saver.Write(BakedSize);
                    saver.Write((ushort)UserData.Count);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.Curves.Count));
                }
                else
                {
                    saver.Write((ushort)UserData.Count);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.Curves.Count));
                    saver.Write(FrameCount);
                    saver.Write(BakedSize);
                }

                saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.ParamCount));
                saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.TexturePatternCount));
                saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.VisibilyCount));
                saver.Write(TextureNames != null ? (ushort)TextureNames.Count : (ushort)0);
                if (saver.ResFile.VersionMajor2 >= 9)
                    saver.Write((ushort)0);
            }
            else if (signature == "FSHU")
            {
                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Write((uint)Flags);
                if (saver.ResFile.Version >= 0x03040000)
                {
                    saver.Write(FrameCount);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write((ushort)UserData.Count);

                    int curveCount = MaterialAnimDataList.Sum((x) => x.Curves.Count);
                    foreach (var mat in MaterialAnimDataList)
                        curveCount += mat.ParamAnimInfos.Sum((x) => x.ConstantCount);

                    saver.Write(curveCount);
                    saver.Write(MaterialAnimDataList.Sum((x) => x.Curves.Count));
                    saver.Write(BakedSize);
                }
                else
                {
                    saver.Write((ushort)FrameCount);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write(UnknownValue);
                    saver.Write(MaterialAnimDataList.Sum((x) => x.Curves.Count));
                    saver.Write(BakedSize);
                    saver.Write(0);
                }

                PosBindModelOffset = saver.SaveOffsetPos();
                PosBindIndicesOffset = saver.SaveOffsetPos();
                PosMatAnimDataOffset = saver.SaveOffsetPos();
                PosUserDataOffset = saver.SaveOffsetPos();
            }
            else if (signature == "FTXP")
            {
                if (TextureNames == null)
                    TextureNames = new ResDict<TextureRef>();
                if (MaterialAnimDataList == null)
                    MaterialAnimDataList = new List<MaterialAnimData>();

                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Write(Flags, true);

                if (saver.ResFile.Version > 0x03040000 && saver.ResFile.Version < 0x03040002)
                {
                    saver.Write((ushort)UserData.Count);
                    saver.Write(FrameCount);
                    saver.Write((ushort)TextureNames.Count);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write(MaterialAnimDataList.Sum((x) => x.PatternAnimInfos.Count));
                    saver.Write(MaterialAnimDataList.Sum((x) => x.Curves.Count));
                    saver.Write(BakedSize);
                }
                else if (saver.ResFile.Version >= 0x03040000)
                {
                    saver.Write((ushort)UserData.Count);
                    saver.Write(FrameCount);
                    saver.Write((ushort)TextureNames.Count);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write(MaterialAnimDataList.Sum((x) => x.PatternAnimInfos.Count));
                    saver.Write(MaterialAnimDataList.Sum((x) => x.Curves.Count));
                    saver.Write(BakedSize);
                }
                else
                {
                    saver.Write((ushort)FrameCount);
                    saver.Write((ushort)TextureNames.Count);
                    saver.Write((ushort)MaterialAnimDataList.Count);
                    saver.Write((ushort)UserData.Count);
                    saver.Write((ushort)MaterialAnimDataList.Sum((x) => x.PatternAnimInfos.Count));
                    saver.Write(MaterialAnimDataList.Sum((x) => x.Curves.Count));
                    saver.Write(BakedSize);
                    saver.Seek(4);
                }
                PosBindModelOffset = saver.SaveOffsetPos();
                PosBindIndicesOffset = saver.SaveOffsetPos();
                PosMatAnimDataOffset = saver.SaveOffsetPos();
                PosTextureNamesOffset = saver.SaveOffsetPos();
                PosUserDataOffset = saver.SaveOffsetPos();
            }
        }

        internal long PosBindModelOffset;
        internal long PosBindIndicesOffset;
        internal long PosMatAnimDataOffset;
        internal long PosTextureNamesUnkOffset;
        internal long PosTextureNamesOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
        internal long PosTextureBindArrayOffset;

        /// <summary>
        /// Represents flags specifying how animation data is stored or should be played.
        /// </summary>
        [Flags]
        public enum MaterialAnimFlags : ushort
        {
            /// <summary>
            /// The stored curve data has been baked.
            /// </summary>
            BakedCurve = 1 << 0,

            /// <summary>
            /// The animation repeats from the start after the last frame has been played.
            /// </summary>
            Looping = 1 << 2
        }
    }
}
