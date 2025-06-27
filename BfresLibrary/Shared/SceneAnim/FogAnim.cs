using System;
using System.Collections.Generic;
using System.Diagnostics;
using BfresLibrary.Core;
using System.IO;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FCAM section in a <see cref="SceneAnim"/> subfile, storing animations controlling fog settings.
    /// </summary>
    [DebuggerDisplay(nameof(FogAnim) + " {" + nameof(Name) + "}")]
    public class FogAnim : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FogAnim"/> class.
        /// </summary>
        public FogAnim()
        {
            Name = "";
            DistanceAttnFuncName = "";
            Flags = 0;
            FrameCount = 0;
            BakedSize = 0;
            DistanceAttnFuncIndex = 0;

            BaseData = new FogAnimData();
            Curves = new List<AnimCurve>();
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FFOG";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public FogAnimFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the distance attenuation function to use.
        /// </summary>
        public sbyte DistanceAttnFuncIndex { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="Curves"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{FogAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the distance attenuation function to use.
        /// </summary>
        public string DistanceAttnFuncName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FogAnimData"/> instance storing initial fog parameters.
        /// </summary>
        public FogAnimData BaseData { get; set; }

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
                Switch.FogAnimParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                Flags = loader.ReadEnum<FogAnimFlags>(true);
                FrameCount = loader.ReadInt32();
                byte numCurve = loader.ReadByte();
                DistanceAttnFuncIndex = loader.ReadSByte();
                ushort numUserData = loader.ReadUInt16();
                BakedSize = loader.ReadUInt32();
                Name = loader.LoadString();
                DistanceAttnFuncName = loader.LoadString();
                Curves = loader.LoadList<AnimCurve>(numCurve);
                BaseData = loader.LoadCustom(() => new FogAnimData(loader));
                UserData = loader.LoadDict<UserData>();
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);

            if (saver.IsSwitch)
                Switch.FogAnimParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.Write(Flags, true);
                saver.Write(FrameCount);
                saver.Write((byte)Curves.Count);
                saver.Write(DistanceAttnFuncIndex);
                saver.Write((ushort)UserData.Count);
                saver.Write(BakedSize);
                saver.SaveString(Name);
                saver.SaveString(DistanceAttnFuncName);
                saver.SaveList(Curves);
                saver.SaveCustom(BaseData, () => BaseData.Save(saver));
                saver.SaveDict(UserData);
            }
        }

        internal long PosCurveArrayOffset;
        internal long PosBaseDataOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum FogAnimFlags : ushort
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