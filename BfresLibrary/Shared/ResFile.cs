using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Syroot.BinaryData;
using BfresLibrary.Core;
using System.ComponentModel;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a NintendoWare for Cafe (NW4F) graphics data archive file.
    /// </summary>
    [DebuggerDisplay(nameof(ResFile) + " {" + nameof(Name) + "}")]
    public class ResFile : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FRES";

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class.
        /// </summary>
        public ResFile()
        {
            Name = "";
            DataAlignment = 8192;

            VersionMajor = 3;
            VersionMajor2 = 4;
            VersionMinor = 0;
            VersionMinor2 = 4;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public ResFile(Stream stream, bool leaveOpen = false) : base()
        {
            if (IsSwitchBinary(stream))
            {
                using (ResFileLoader loader = new Switch.Core.ResFileSwitchLoader(this, stream)) {
                    loader.Execute();
                }
            }
            else
            {
                using (ResFileLoader loader = new WiiU.Core.ResFileWiiULoader(this, stream)) {
                    loader.Execute();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public ResFile(string fileName) : base()
        {
            if (IsSwitchBinary(fileName))
            {
                using (ResFileLoader loader = new Switch.Core.ResFileSwitchLoader(this, fileName)) {
                    loader.Execute();
                }
            }
            else
            {
                using (ResFileLoader loader = new WiiU.Core.ResFileWiiULoader(this, fileName)) {
                    loader.Execute();
                }
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Save(Stream stream, bool leaveOpen = false)
        {
            if (IsPlatformSwitch) {
                using (ResFileSaver saver = new Switch.Core.ResFileSwitchSaver(this, stream, leaveOpen)) {
                    saver.Execute();
                }
            }
            else
            {
                using (ResFileSaver saver = new WiiU.Core.ResFileWiiUSaver(this, stream, leaveOpen)) {
                    saver.Execute();
                }
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Save(string fileName)
        {
            if (IsPlatformSwitch) {
                using (ResFileSaver saver = new Switch.Core.ResFileSwitchSaver(this, fileName)) {
                    saver.Execute();
                }
            }
            else
            {
                using (ResFileSaver saver = new WiiU.Core.ResFileWiiUSaver(this, fileName)) {
                    saver.Execute();
                }
            }
        }

        internal uint SaveVersion()
        {
            return VersionMajor << 24 | VersionMajor2 << 16 | VersionMinor << 8 | VersionMinor2;
        }


        public static bool IsSwitchBinary(string fileName) {
            return IsSwitchBinary(File.OpenRead(fileName));
        }

        public static bool IsSwitchBinary(Stream stream)
        {
            using (var reader = new BinaryDataReader(stream, true)) {
                reader.ByteOrder = ByteOrder.LittleEndian;

                reader.Seek(4, SeekOrigin.Begin);
                uint paddingCheck = reader.ReadUInt32();
                reader.Position = 0;

                return paddingCheck == 0x20202020;
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public bool IsPlatformSwitch { get; set; }

        /// <summary>
        /// Gets or sets the alignment to use for raw data blocks in the file.
        /// </summary>
        [Browsable(true)]
        [Category("Binary Info")]
        [DisplayName("Alignment")]
        public uint Alignment { get; set; } = 0xC;

        public int DataAlignment
        {
            get
            {
                if (IsPlatformSwitch)
                    return (1 << (int)Alignment);
                else
                    return (int)Alignment;
            }
            set
            {
                if (IsPlatformSwitch)
                    Alignment = (uint)(value >> 7);
                else
                    Alignment = (uint)value;
            }
        }

        public int DataAlignmentOverride = 0;

        /// <summary>
        /// Gets or sets the target adress size to use for raw data blocks in the file.
        /// </summary>
        [Browsable(false)]
        public uint TargetAddressSize { get; set; }

        /// <summary>
        /// Gets or sets a name describing the contents.
        /// </summary>
        [Browsable(true)]
        [Category("Binary Info")]
        [DisplayName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the revision of the BFRES structure formats.
        /// </summary>
        internal uint Version { get; set; }

        /// <summary>
        /// Gets or sets the flag. Unknown purpose.
        /// </summary>
        internal uint Flag { get; set; }

        /// <summary>
        /// Gets or sets the BlockOffset. 
        /// </summary>
        internal uint BlockOffset { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="MemoryPool"/> instances. 
        /// </summary>
        internal MemoryPool MemoryPool { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="BufferInfo"/> instances.
        /// </summary>
        internal BufferInfo BufferInfo { get; set; }

        internal StringTable StringTable { get; set; }

        /// <summary>
        /// Combination of all the material animations into one.
        /// This is used for switch material animations
        /// </summary>
        internal ResDict<MaterialAnim> MaterialAnims { get; set; } = new ResDict<MaterialAnim>();

        /// <summary>
        /// Gets or sets the major revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Version")]
        [DisplayName("Version Major")]
        public string VersioFull
        {
            get
            {
                return $"{VersionMajor},{VersionMajor2},{VersionMinor},{VersionMinor2}";
            }
        }

        /// <summary>
        /// Gets or sets the major revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Major")]
        public uint VersionMajor { get; set; }
        /// <summary>
        /// Gets or sets the second major revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Major 2")]
        public uint VersionMajor2 { get; set; }
        /// <summary>
        /// Gets or sets the minor revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Minor")]
        public uint VersionMinor { get; set; }
        /// <summary>
        /// Gets or sets the second minor revision of the BFRES structure formats.
        /// </summary>
        [Browsable(true)]
        [Category("Version")]
        [DisplayName("Version Minor 2")]
        public uint VersionMinor2 { get; set; }

        /// <summary>
        /// Gets the byte order in which data is stored. Must be the endianness of the target platform.
        /// </summary>
        [Browsable(false)]
        public ByteOrder ByteOrder { get; set; } = ByteOrder.BigEndian;

        /// <summary>
        /// Gets or sets the stored <see cref="Model"/> (FMDL) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<Model> Models { get; set; } = new ResDict<Model>();

        /// <summary>
        /// Gets or sets the stored <see cref="Texture"/> (FTEX) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<TextureShared> Textures { get; set; } = new ResDict<TextureShared>();

        /// <summary>
        /// Gets or sets the stored <see cref="SkeletalAnim"/> (FSKA) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<SkeletalAnim> SkeletalAnims { get; set; } = new ResDict<SkeletalAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="ShaderParamAnim"/> (FSHU) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<MaterialAnim> ShaderParamAnims { get; set; } = new ResDict<MaterialAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="ShaderParamAnim"/> (FSHU) instances for color animations.
        /// </summary>
        [Browsable(false)]
        public ResDict<MaterialAnim> ColorAnims { get; set; } = new ResDict<MaterialAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="ShaderParamAnim"/> (FSHU) instances for texture SRT animations.
        /// </summary>
        [Browsable(false)]
        public ResDict<MaterialAnim> TexSrtAnims { get; set; } = new ResDict<MaterialAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="TexPatternAnim"/> (FTXP) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<MaterialAnim> TexPatternAnims { get; set; } = new ResDict<MaterialAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="VisibilityAnim"/> (FVIS) instances for bone visibility animations.
        /// </summary>
        [Browsable(false)]
        public ResDict<VisibilityAnim> BoneVisibilityAnims { get; set; } = new ResDict<VisibilityAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="VisibilityAnim"/> (FVIS) instances for material visibility animations.
        /// </summary>
        [Browsable(false)]
        public ResDict<MaterialAnim> MatVisibilityAnims { get; set; } = new ResDict<MaterialAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="ShapeAnim"/> (FSHA) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<ShapeAnim> ShapeAnims { get; set; } = new ResDict<ShapeAnim>();

        /// <summary>
        /// Gets or sets the stored <see cref="SceneAnim"/> (FSCN) instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<SceneAnim> SceneAnims { get; set; } = new ResDict<SceneAnim>();

        /// <summary>
        /// Gets or sets attached <see cref="ExternalFile"/> instances. The key of the dictionary typically represents
        /// the name of the file they were originally created from.
        /// </summary>
        [Browsable(false)]
        public ResDict<ExternalFile> ExternalFiles { get; set; } = new ResDict<ExternalFile>();

        public ExternalFlags ExternalFlag;

        // ---- METHODS (INTERNAL) ---------------------------------------------------------------------------------------

        internal void SetVersionInfo(uint Version)
        {
            VersionMajor = Version >> 24;
            VersionMajor2 = Version >> 16 & 0xFF;
            VersionMinor = Version >> 8 & 0xFF;
            VersionMinor2 = Version & 0xFF;
        }

        // ---- ENUMS ------------------------------------------------------------------------------------------------

        //Flags thanks to watertoon
        public enum ExternalFlags : byte
        {
            IsExternalModelUninitalized = 1 << 0,
            HasExternalString = 1 << 1,
            HoldsExternalStrings = 1 << 2,
            HasExternalGPU = 1 << 3,

            MeshCodecResave = 1 << 7,
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public void ChangePlatform(bool isSwitch, int alignment,
            byte versionA, byte versionB, byte versionC, byte versionD, PlatformConverters.ConverterHandle handle)
        {
            if (IsPlatformSwitch && isSwitch || (!IsPlatformSwitch && !isSwitch))
                return;

            //Shaders cannot be converted, remove them
            for (int i = 0; i < ExternalFiles.Count; i++)
            {
                if (ExternalFiles.Keys.ElementAt(i).Contains(".bfsha"))
                    ExternalFiles.RemoveAt(i);
            }

            if (!IsPlatformSwitch && isSwitch) {
                ConvertTexturesToBntx(Textures.Values.ToList());
            }
            else
            {
                List<TextureShared> textures = new List<TextureShared>();
                foreach (var tex in this.Textures.Values)
                {
                    var textureU = new WiiU.Texture();
                    textureU.FromSwitch((Switch.SwitchTexture)tex);
                    textures.Add(textureU);
                }
                Textures.Clear();
                foreach (var tex in textures)
                    Textures.Add(tex.Name, tex);

                foreach (var mdl in Models.Values) {
                    foreach (var mat in mdl.Materials.Values) {
                        mat.RenderState = new RenderState();
                    }
                }

                for (int i = 0; i < ExternalFiles.Count; i++)
                {
                    if (ExternalFiles.Keys.ElementAt(i).Contains(".bntx"))
                        ExternalFiles.RemoveAt(i);
                }
            }

            //Order to read the existing data
            ByteOrder byteOrder = IsPlatformSwitch ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
            //Order to set the target data
            ByteOrder targetOrder = isSwitch ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

            IsPlatformSwitch = isSwitch;
            DataAlignment = alignment;
            VersionMajor = versionA;
            VersionMajor2 = versionB;
            VersionMinor = versionC;
            VersionMinor2 = versionD;
            this.ByteOrder = targetOrder;

            foreach (var model in Models.Values)
            {
                UpdateVertexBufferByteOrder(model, byteOrder, targetOrder);
                foreach (var shp in model.Shapes.Values) {
                    foreach (var mesh in shp.Meshes) {
                        mesh.UpdateIndexBufferByteOrder(targetOrder);
                    }
                }
                foreach (var mat in model.Materials.Values)
                {
                    if (IsPlatformSwitch)
                        PlatformConverters.MaterialConverter.ConvertToSwitchMaterial(mat, handle);
                    else
                        PlatformConverters.MaterialConverter.ConvertToWiiUMaterial(mat, handle);
                }
            }

            if (IsPlatformSwitch)
            {
                foreach (var anim in ShaderParamAnims.Values) {
                    anim.Name = $"{anim.Name}_fsp";
                }

                foreach (var anim in TexSrtAnims.Values) {
                    anim.Name = $"{anim.Name}_fts";
                }

                foreach (var anim in ColorAnims.Values) {
                    anim.Name = $"{anim.Name}_fcl";
                }

                foreach (var anim in TexPatternAnims.Values) {
                    anim.Name = $"{anim.Name}_ftp";
                }

                foreach (var anim in MatVisibilityAnims.Values) {
                    anim.Name = $"{anim.Name}_fvs";
                }
            }
            else
            {
                this.TexPatternAnims = new ResDict<MaterialAnim>();
                this.ShaderParamAnims = new ResDict<MaterialAnim>();
                this.ColorAnims = new ResDict<MaterialAnim>();
                this.TexSrtAnims = new ResDict<MaterialAnim>();
                this.MatVisibilityAnims = new ResDict<MaterialAnim>();
            }
        }

        void IResData.Load(ResFileLoader loader)
        {
            IsPlatformSwitch = loader.IsSwitch;
            if (loader.IsSwitch)
                 Switch.ResFileParser.Load((Switch.Core.ResFileSwitchLoader)loader, this);
            else
                WiiU.ResFileParser.Load((WiiU.Core.ResFileWiiULoader)loader, this);
            //Custom external file loading
            foreach (var file in ExternalFiles)
            {
                if (file.Key.EndsWith(".brtcamera"))
                    file.Value.LoadedFileData = new Brtcamera(new MemoryStream(file.Value.Data), !IsPlatformSwitch);
            }
        }

        void IResData.Save(ResFileSaver saver) {
            PreSave();
            if (saver.IsSwitch)
                Switch.ResFileParser.Save((Switch.Core.ResFileSwitchSaver)saver, this);
            else
                WiiU.ResFileParser.Save((WiiU.Core.ResFileWiiUSaver)saver, this);
        }

        internal void PreSave()
        {
            Version = SaveVersion();
            bool calculateBakeSizes = true;

            if (MatVisibilityAnims == null)
                MatVisibilityAnims = new ResDict<MaterialAnim>();

            for (int i = 0; i < Models.Count; i++) {
                for (int s = 0; s < Models[i].Shapes.Count; s++) {

                    Models[i].Shapes[s].VertexBuffer = Models[i].VertexBuffers[Models[i].Shapes[s].VertexBufferIndex];

                    //Link texture sections for wii u texture refs
                    if (Textures != null)
                    {
                        foreach (var texRef in Models[i].Materials[Models[i].Shapes[s].MaterialIndex].TextureRefs)
                        {
                            if (Textures.ContainsKey(texRef.Name))
                                texRef.Texture = Textures[texRef.Name];
                        }
                    }
                }
            }

            for (int i = 0; i < SkeletalAnims.Count; i++)
            {
                int curveIndex = 0;
                if (calculateBakeSizes)
                SkeletalAnims[i].BakedSize = 0;
                for (int s = 0; s < SkeletalAnims[i].BoneAnims.Count; s++)
                {
                    SkeletalAnims[i].BoneAnims[s].BeginCurve = curveIndex;
                    curveIndex += SkeletalAnims[i].BoneAnims[s].Curves.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in SkeletalAnims[i].BoneAnims[s].Curves)
                            SkeletalAnims[i].BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            // Update ShapeAnim instances.
            foreach (ShapeAnim anim in ShapeAnims.Values)
            {
                int curveIndex = 0;
                int infoIndex = 0;
                if (calculateBakeSizes)
                    anim.BakedSize = 0;
                foreach (VertexShapeAnim subAnim in anim.VertexShapeAnims)
                {
                    subAnim.BeginCurve = curveIndex;
                    subAnim.BeginKeyShapeAnim = infoIndex;
                    curveIndex += subAnim.Curves.Count;
                    infoIndex += subAnim.KeyShapeAnimInfos.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in subAnim.Curves)
                            anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in TexPatternAnims.Values)
            {
                anim.signature = IsPlatformSwitch ? "FMAA" : "FTXP";
                int curveIndex = 0;
                int infoIndex = 0;
                if (calculateBakeSizes)
                    anim.BakedSize = 0;
                foreach (var subAnim in anim.MaterialAnimDataList)
                {
                    if (subAnim.Curves.Count > 0)
                        subAnim.TexturePatternCurveIndex = curveIndex;
                    subAnim.InfoIndex = infoIndex;
                    curveIndex += subAnim.Curves.Count;
                    infoIndex += subAnim.PatternAnimInfos.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in subAnim.Curves)
                            anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in TexSrtAnims.Values)
            {
                anim.signature = IsPlatformSwitch ? "FMAA" : "FSHU";
                int curveIndex = 0;
                int infoIndex = 0;
                if (calculateBakeSizes)
                    anim.BakedSize = 0;
                foreach (var subAnim in anim.MaterialAnimDataList)
                {
                    if (subAnim.Curves.Count > 0)
                        subAnim.TexturePatternCurveIndex = curveIndex;
                    subAnim.InfoIndex = infoIndex;
                    curveIndex += subAnim.Curves.Count;
                    infoIndex += subAnim.PatternAnimInfos.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in subAnim.Curves)
                            anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in ColorAnims.Values)
            {
                anim.signature = IsPlatformSwitch ? "FMAA" : "FSHU";
                int curveIndex = 0;
                int infoIndex = 0;
                if (calculateBakeSizes)
                    anim.BakedSize = 0;
                foreach (var subAnim in anim.MaterialAnimDataList)
                {
                    if (subAnim.Curves.Count > 0)
                        subAnim.TexturePatternCurveIndex = curveIndex;
                    subAnim.InfoIndex = infoIndex;
                    curveIndex += subAnim.Curves.Count;
                    infoIndex += subAnim.PatternAnimInfos.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in subAnim.Curves)
                            anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in ShaderParamAnims.Values)
            {
                anim.signature = IsPlatformSwitch ? "FMAA" : "FSHU";
                int curveIndex = 0;
                int infoIndex = 0;
                if (calculateBakeSizes)
                    anim.BakedSize = 0;
                foreach (var subAnim in anim.MaterialAnimDataList)
                {
                    if (subAnim.Curves.Count > 0)
                        subAnim.ShaderParamCurveIndex = curveIndex;
                    subAnim.InfoIndex = infoIndex;
                    curveIndex += subAnim.Curves.Count;
                    infoIndex += subAnim.ParamAnimInfos.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in subAnim.Curves)
                            anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in MatVisibilityAnims.Values)
            {
                int curveIndex = 0;
                int infoIndex = 0;
                if (calculateBakeSizes)
                    anim.BakedSize = 0;
                foreach (var subAnim in anim.MaterialAnimDataList)
                {
                    if (subAnim.Curves.Count > 0)
                        subAnim.VisalCurveIndex = curveIndex;
                    curveIndex += subAnim.Curves.Count;

                    if (calculateBakeSizes)
                    {
                        foreach (var curve in subAnim.Curves)
                            anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in SceneAnims.Values)
            {
                foreach (var camAnim in anim.CameraAnims.Values)
                {
                    if (calculateBakeSizes)
                    {
                        foreach (var curve in camAnim.Curves)
                            camAnim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
                    }
                }
            }

            foreach (var anim in BoneVisibilityAnims.Values)
            {
                anim.BakedSize = 0;
                foreach (var curve in anim.Curves)
                    anim.BakedSize += curve.CalculateBakeSize(IsPlatformSwitch);
            }

            if (IsPlatformSwitch)
            {
                MaterialAnims.Clear();
                foreach (var anim in ShaderParamAnims.Values)
                    MaterialAnims.Add(anim.Name, anim);

                foreach (var anim in TexSrtAnims.Values)
                    MaterialAnims.Add(anim.Name, anim);

                foreach (var anim in ColorAnims.Values)
                    MaterialAnims.Add(anim.Name, anim);

                foreach (var anim in TexPatternAnims.Values)
                    MaterialAnims.Add(anim.Name, anim);

                foreach (var anim in MatVisibilityAnims.Values)
                    MaterialAnims.Add(anim.Name, anim);

                for (int i = 0; i < MaterialAnims.Count; i++)
                {
                    MaterialAnims[i].signature = "FMAA";
                }
            }

            //Custom external file loading
            foreach (var file in ExternalFiles)
            {
                if (file.Value.LoadedFileData is Brtcamera)
                {
                    var cam = file.Value.LoadedFileData as Brtcamera;
                    cam.IsBigEndian = !IsPlatformSwitch;
                    var mem = new MemoryStream();
                    cam.Save(mem);
                    file.Value.Data = mem.ToArray();
                }
            }
        }

        private void UpdateVertexBufferByteOrder(Model model, ByteOrder byteOrder, ByteOrder target)
        {
            foreach (var buffer in model.VertexBuffers)
                buffer.UpdateVertexBufferByteOrder(byteOrder, target);
        }

        private void ConvertTexturesToBntx(List<TextureShared> textures)
        {
            if (textures.Count == 0) return;

            var bntx = PlatformConverters.TextureConverter.CreateBNTX(textures);
            var mem = new MemoryStream();
            bntx.Save(mem);

            bntx = new Syroot.NintenTools.NSW.Bntx.BntxFile(new MemoryStream(mem.ToArray()));

            mem = new MemoryStream();
            bntx.Save(mem);

            ExternalFiles.Add("textures.bntx", new ExternalFile()
            {
                Data = mem.ToArray(),
            });
        }

        //Reserved for saving offsets 
        internal long ModelOffset = 0;
        internal long SkeletonAnimationOffset = 0;
        internal long MaterialAnimationOffset = 0;
        internal long ShapeAnimationOffset = 0;
        internal long BoneVisAnimationOffset = 0;
        internal long SceneAnimationOffset = 0;
        internal long ExternalFileOffset = 0;

        internal long ModelDictOffset = 0;
        internal long SkeletonAnimationDictOffset = 0;
        internal long MaterialAnimationnDictOffset = 0;
        internal long ShapeAnimationDictOffset = 0;
        internal long BoneVisAnimationDictOffset = 0;
        internal long SceneAnimationDictOffset = 0;
        internal long ExternalFileDictOffset = 0;

        internal long BufferInfoOffset = 0;
    }
}
