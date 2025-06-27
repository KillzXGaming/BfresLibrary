using System;
using System.Collections.Generic;
using System.Diagnostics;
using BfresLibrary.Core;
using System.IO;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FLIT section in a <see cref="SceneAnim"/> subfile, storing animations controlling light settings.
    /// </summary>
    [DebuggerDisplay(nameof(LightAnim) + " {" + nameof(Name) + "}")]
    public class LightAnim : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightAnim"/> class.
        /// </summary>
        public LightAnim()
        {
            Name = "";
            DistanceAttnFuncName = "";
            AngleAttnFuncName = "";
            Flags = 0;
            LightTypeIndex = 0;
            AnimatedFields = 0;
            FrameCount = 0;
            BakedSize = 0;
            DistanceAttnFuncIndex = 0;
            AngleAttnFuncIndex = 0;

            BaseData = new LightAnimData();
            Curves = new List<AnimCurve>();
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FLIT";

        private const ushort _flagsMask = 0b00000001_00000101;
        private const ushort _flagsMaskFields = 0b11111110_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal ushort _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets flags controlling how the animation should be played.
        /// </summary>
        public LightAnimFlags Flags
        {
            get { return (LightAnimFlags)(_flags & _flagsMask); }
            set { _flags = (ushort)(_flags & ~_flagsMask | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public LightAnimField AnimatedFields
        {
            get { return (LightAnimField)(_flags & _flagsMaskFields); }
            set { _flags = (ushort)(_flags & ~_flagsMaskFields | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the light type.
        /// </summary>
        public sbyte LightTypeIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the index of the distance attenuation function to use.
        /// </summary>
        public sbyte DistanceAttnFuncIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the index of the angle attenuation function to use.
        /// </summary>
        public sbyte AngleAttnFuncIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{LightAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the light type.
        /// </summary>
        public string LightTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the distance attenuation function to use.
        /// </summary>
        public string DistanceAttnFuncName { get; set; }

        /// <summary>
        /// Gets or sets the name of the angle attenuation function to use.
        /// </summary>
        public string AngleAttnFuncName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }
        
        /// <summary>
        /// Gets the <see cref="LightAnimData"/> instance storing initial light parameters.
        /// </summary>
        public LightAnimData BaseData { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict<UserData> UserData { get; set; } = new ResDict<UserData>();

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public void Import(string FileName, ResFile ResFile) {
            ResFileLoader.ImportSection(FileName, this, ResFile);
        }

        public void Export(string FileName, ResFile ResFile) {
            ResFileSaver.ExportSection(FileName, this, ResFile);
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.IsSwitch)
                Switch.LightAnimParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                Flags = loader.ReadEnum<LightAnimFlags>(true);
                ushort numUserData = loader.ReadUInt16();
                FrameCount = loader.ReadInt32();
                byte numCurve = loader.ReadByte();
                LightTypeIndex = loader.ReadSByte();
                DistanceAttnFuncIndex = loader.ReadSByte();
                AngleAttnFuncIndex = loader.ReadSByte();
                BakedSize = loader.ReadUInt32();
                Name = loader.LoadString();
                LightTypeName = loader.LoadString();
                DistanceAttnFuncName = loader.LoadString();
                AngleAttnFuncName = loader.LoadString();
                Curves = loader.LoadList<AnimCurve>(numCurve);
                BaseData = loader.LoadCustom(() => new LightAnimData(loader, AnimatedFields));
                UserData = loader.LoadDict<UserData>();
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);

            if (saver.IsSwitch)
                Switch.LightAnimParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.Write(Flags, true);
                saver.Write((ushort)UserData.Count);
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Write(LightTypeIndex);
                saver.Write(DistanceAttnFuncIndex);
                saver.Write(AngleAttnFuncIndex);
                saver.Write(BakedSize);
                saver.SaveString(Name);
                saver.SaveString(LightTypeName);
                saver.SaveString(DistanceAttnFuncName);
                saver.SaveString(AngleAttnFuncName);
                saver.SaveList(Curves);
                saver.SaveCustom(BaseData, () => BaseData.Save(saver, AnimatedFields));
                saver.SaveDict(UserData);
            }
        }

        internal long PosCurveArrayOffset;
        internal long PosBaseDataOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored.
    /// </summary>
    [Flags]
    public enum LightAnimFlags : ushort
    {
        /// <summary>
        /// The stored curve data has been baked.
        /// </summary>
        BakedCurve = 1 << 0,

        /// <summary>
        /// The animation repeats from the start after the last frame has been played.
        /// </summary>
        Looping = 1 << 2,

        EnableCurve = 1 << 8,

        BaseEnable = 1 << 9,

        BasePos = 1 << 10,

        BaseDir = 1 << 11,

        BaseDistAttn = 1 << 12,

        BaseAngleAttn = 1 << 13,

        BaseColor0 = 1 << 14,

        BaseColor1 = 1 << 15,
    }

    /// <summary>
    /// Represents flags specifying which fields are animated.
    /// </summary>
    [Flags]
    public enum LightAnimField : ushort
    {
        /// <summary>
        /// Enabled state is animated.
        /// </summary>
        Enable = 1 << 9,

        /// <summary>
        /// Position is animated.
        /// </summary>
        Position = 1 << 10,

        /// <summary>
        /// Rotation is animated.
        /// </summary>
        Rotation = 1 << 11,

        /// <summary>
        /// Distance attenuation is animated.
        /// </summary>
        DistanceAttn = 1 << 12,

        /// <summary>
        /// Angle attenuation is animated in degrees.
        /// </summary>
        AngleAttn = 1 << 13,

        /// <summary>
        /// Color 0 is animated.
        /// </summary>
        Color0 = 1 << 14,

        /// <summary>
        /// Color 1 is animated.
        /// </summary>
        Color1 = 1 << 15
    }
}