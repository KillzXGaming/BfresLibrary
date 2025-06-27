using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using BfresLibrary.Core;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FVIS subfile in a <see cref="ResFile"/>, storing visibility animations of <see cref="Bone"/> or
    /// <see cref="Material"/> instances.
    /// </summary>
    [DebuggerDisplay(nameof(VisibilityAnim) + " {" + nameof(Name) + "}")]
    public class VisibilityAnim : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityAnim"/> class.
        /// </summary>
        public VisibilityAnim()
        {
            Name = "";
            Path = "";
            Flags = 0;
            FrameCount = 1;
            BakedSize = 0;
            Curves = new List<AnimCurve>();
            BindIndices = new ushort[0];
            Names = new List<string>();
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FVIS";

        private const ushort _flagsMask = 0b00000000_00000111;
        private const ushort _flagsMaskType = 0b00000001_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal ushort _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{VisibilityAnim}"/> instances.
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

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        [Browsable(false)]
        public VisibilityAnimFlags Flags
        {
            get { return (VisibilityAnimFlags)(_flags & _flagsMask); }
            set { _flags = (ushort)(_flags & ~_flagsMask | (ushort)value); }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Loop
        {
            get
            {
                return Flags.HasFlag(VisibilityAnimFlags.Looping);
            }
            set
            {
                if (value == true)
                    Flags |= VisibilityAnimFlags.Looping;
                else
                    Flags &= ~VisibilityAnimFlags.Looping;
            }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Baked
        {
            get
            {
                return Flags.HasFlag(VisibilityAnimFlags.BakedCurve);
            }
            set
            {
                if (value == true)
                    Flags |= VisibilityAnimFlags.BakedCurve;
                else
                    Flags &= ~VisibilityAnimFlags.BakedCurve;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="AnimCurve"/> instances of all
        /// <see cref="BoneAnims"/>.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Animation")]
        [DisplayName("Baked Size")]
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        [Browsable(false)]
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets or sets the indices of entries in the <see cref="Skeleton.Bones"/> or <see cref="Model.Materials"/>
        /// dictionaries to bind to for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        [Browsable(false)]
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets the names of entries in the <see cref="Skeleton.Bones"/> or <see cref="Model.Materials"/>
        /// dictionaries to bind to for each animation.
        /// </summary>
        [Browsable(false)]
        public IList<string> Names { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        [Browsable(false)]
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets boolean values storing the initial visibility for each <see cref="Bone"/> or
        /// <see cref="Material"/>.
        /// </summary>
        [Browsable(false)]
        public bool[] BaseDataList { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<UserData> UserData { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public void Import(string FileName, ResFile ResFile)
        {
            if (FileName.EndsWith(".json"))
            {
                TextConvert.BoneVisibilityAnimConvert.FromJson(this, File.ReadAllText(FileName));
            }
            else
                ResFileLoader.ImportSection(FileName, this, ResFile);
        }

        public void Export(string FileName, ResFile ResFile)
        {
            if (FileName.EndsWith(".json"))
                File.WriteAllText(FileName, TextConvert.BoneVisibilityAnimConvert.ToJson(this));
            else
                ResFileSaver.ExportSection(FileName, this, ResFile);
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
                Switch.VisibilityAnimParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                loader.CheckSignature(_signature);
                Name = loader.LoadString();
                Path = loader.LoadString();
                _flags = loader.ReadUInt16();
                ushort numAnim = 0;
                ushort numCurve = 0;
                if (loader.ResFile.Version >= 0x03040000)
                {
                    ushort numUserData = loader.ReadUInt16();
                    FrameCount = loader.ReadInt32();
                    numAnim = loader.ReadUInt16();
                    numCurve = loader.ReadUInt16();
                    BakedSize = loader.ReadUInt32();
                }
                else
                {
                    FrameCount = loader.ReadInt16();
                    numAnim = loader.ReadUInt16();
                    numCurve = loader.ReadUInt16();
                    ushort numUserData = loader.ReadUInt16();
                    BakedSize = loader.ReadUInt32();
                    int padding2 = loader.ReadInt16();
                }
                BindModel = loader.Load<Model>();
                BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numAnim));
                Names = loader.LoadCustom(() => loader.LoadStrings(numAnim)); // Offset to name list.
                Curves = loader.LoadList<AnimCurve>(numCurve);
                BaseDataList = loader.LoadCustom(() =>
                {
                    return loader.ReadBit32Booleans(numAnim);
                });
                UserData = loader.LoadDict<UserData>();
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
                Switch.VisibilityAnimParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.WriteSignature(_signature);
                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Write(_flags);
                if (saver.ResFile.Version >= 0x03040000)
                {
                    saver.Write((ushort)UserData.Count);
                    saver.Write(FrameCount);
                    saver.Write((ushort)Names.Count);
                    saver.Write((ushort)Curves.Count);
                    saver.Write(BakedSize);
                }
                else
                {
                    saver.Write((ushort)FrameCount);
                    saver.Write((ushort)Names.Count);
                    saver.Write((ushort)Curves.Count);
                    saver.Write((ushort)UserData.Count);
                    saver.Write(BakedSize);
                    saver.Write((ushort)0);
                }

                saver.Save(BindModel);
                saver.SaveCustom(BindIndices, () => saver.Write(BindIndices));
                saver.SaveCustom(Names, () => saver.SaveStrings(Names));
                saver.SaveList(Curves);
                saver.SaveCustom(BaseDataList, () =>
                {
                    saver.WriteBit32Booleans(BaseDataList);
                });
                saver.SaveDict(UserData);
            }
        }

        internal long PosBindIndicesOffset;
        internal long PosCurvesOffset;
        internal long PosBaseDataOffset;
        internal long PosNamesOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
    }

    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum VisibilityAnimFlags : ushort
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
