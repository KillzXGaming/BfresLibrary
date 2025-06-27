using System;
using System.Diagnostics;
using BfresLibrary.Core;
using System.IO;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FSCN subfile in a <see cref="ResFile"/>, storing scene animations controlling camera, light and
    /// fog settings.
    /// </summary>
    [DebuggerDisplay(nameof(SceneAnim) + " {" + nameof(Name) + "}")]
    public class SceneAnim : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneAnim"/> class.
        /// </summary>
        public SceneAnim()
        {
            Name = "";
            Path = "";

            CameraAnims = new ResDict<CameraAnim>();
            LightAnims = new ResDict<LightAnim>();
            FogAnims = new ResDict<FogAnim>();
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSCN";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{SceneAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CameraAnim"/> instances.
        /// </summary>
        public ResDict<CameraAnim> CameraAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LightAnim"/> instances.
        /// </summary>
        public ResDict<LightAnim> LightAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FogAnim"/> instances.
        /// </summary>
        public ResDict<FogAnim> FogAnims { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict<UserData> UserData { get; set; } = new ResDict<UserData>();

        internal uint Flags { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public void Import(string FileName, ResFile ResFile)
        {
            if (FileName.EndsWith(".json"))
            {
                TextConvert.SceneAnimConvert.FromJson(this, File.ReadAllText(FileName));
            }
            else
                ResFileLoader.ImportSection(FileName, this, ResFile);
        }

        public void Export(string FileName, ResFile ResFile)
        {
            if (FileName.EndsWith(".json"))
                File.WriteAllText(FileName, TextConvert.SceneAnimConvert.ToJson(this));
            else
                ResFileSaver.ExportSection(FileName, this, ResFile);
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.IsPlatformSwitch)
                Switch.SceneAnimParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                Name = loader.LoadString();
                Path = loader.LoadString();
                ushort numUserData = loader.ReadUInt16();
                ushort numCameraAnim = loader.ReadUInt16();
                ushort numLightAnim = loader.ReadUInt16();
                ushort numFogAnim = loader.ReadUInt16();
                CameraAnims = loader.LoadDict<CameraAnim>();
                LightAnims = loader.LoadDict<LightAnim>();
                FogAnims = loader.LoadDict<FogAnim>();
                UserData = loader.LoadDict<UserData>();
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.ResFile.IsPlatformSwitch)
                Switch.SceneAnimParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)CameraAnims.Count);
                saver.Write((ushort)LightAnims.Count);
                saver.Write((ushort)FogAnims.Count);
                saver.SaveDict(CameraAnims);
                saver.SaveDict(LightAnims);
                saver.SaveDict(FogAnims);
                saver.SaveDict(UserData);
            }
        }

        internal long PosCameraAnimArrayOffset;
        internal long PosCameraAnimDictOffset;
        internal long PosLightAnimArrayOffset;
        internal long PosLightAnimDictOffset;
        internal long PosFogAnimArrayOffset;
        internal long PosFogAnimDictOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
    }
}