using System.Collections.Generic;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a material animation in a <see cref="MaterialAnim"/> subfile, storing material animation data.
    /// </summary>
    public class MaterialAnimData : INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAnimData"/> class.
        /// </summary>
        public MaterialAnimData()
        {
            Name = "";

            ParamAnimInfos = new List<ParamAnimInfo>();
            PatternAnimInfos = new List<PatternAnimInfo>();
            Constants = new List<AnimConstant>();
            Curves = new List<AnimCurve>();

            ShaderParamCurveIndex = -1;
            TexturePatternCurveIndex = -1;
            BeginVisalConstantIndex = -1;
            VisalCurveIndex = -1;
            VisualConstantIndex = -1;
        }

        public MaterialAnimData(ResFileLoader loader, string signature) : base() {
            Load(loader, signature);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the list of <see cref="ParamAnimInfo"/> instances.
        /// </summary>
        public IList<ParamAnimInfo> ParamAnimInfos { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="TexturePatternAnimInfo"/> instances.
        /// </summary>
        public IList<PatternAnimInfo> PatternAnimInfos { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="AnimConstant"/> instances.
        /// </summary>
        public IList<AnimConstant> Constants { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AnimCurve"/> instances animating properties of objects stored in this section.
        /// </summary>
        public IList<AnimCurve> Curves { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{MaterialAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        public int ShaderParamCurveIndex { get; set; } = -1;
        public int TexturePatternCurveIndex { get; set; } = -1;
        public int BeginVisalConstantIndex { get; set; } = -1;
        public int VisalCurveIndex { get; set; } = -1;
        public int VisualConstantIndex { get; set; } = -1;
        public int InfoIndex { get; set; } = 0;

        public ushort[] BaseDataList { get; set; }

        public ushort VisibilyCount
        {
            get
            {
                return 0;
            }
        }

        public ushort TexturePatternCount
        {
            get {
                if (PatternAnimInfos.Count > 0)
                    return (ushort)(PatternAnimInfos.Count + (Constants != null ? Constants.Count : 0));
                else
                    return 0; 
            }
        }

        public ushort ParamCount
        {
            get {
                int paramsCount = 0;
                foreach (var paramInfo in ParamAnimInfos)
                    paramsCount += paramInfo.ConstantCount + paramInfo.FloatCurveCount + paramInfo.IntCurveCount;
                return (ushort)paramsCount; 
            }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        internal void Load(ResFileLoader loader, string signature)
        {
            ParamAnimInfos = new List<ParamAnimInfo>();
            PatternAnimInfos = new List<PatternAnimInfo>();
            Constants = new List<AnimConstant>();
            Curves = new List<AnimCurve>();

            if (signature == "FSHU")
            {
                ushort numAnimParam = loader.ReadUInt16();
                ushort numCurve = loader.ReadUInt16();
                ushort numConstant = loader.ReadUInt16();
                loader.Seek(2);
                ShaderParamCurveIndex = loader.ReadInt32();
                InfoIndex = loader.ReadInt32();
                Name = loader.LoadString();
                ParamAnimInfos = loader.LoadList<ParamAnimInfo>(numAnimParam);
                Curves = loader.LoadList<AnimCurve>(numCurve);
                Constants = loader.LoadCustom(() => loader.ReadAnimConstants(numConstant));
            }
            else if (signature == "FTXP")
            {
                ushort numPatAnim = loader.ReadUInt16();
                ushort numCurve = loader.ReadUInt16();
                TexturePatternCurveIndex = loader.ReadInt32();
                InfoIndex = loader.ReadInt32();
                Name = loader.LoadString();
                PatternAnimInfos = loader.LoadList<PatternAnimInfo>(numPatAnim);
                Curves = loader.LoadList<AnimCurve>(numCurve);
                BaseDataList = loader.LoadCustom(() => loader.ReadUInt16s(numPatAnim));
            }
            else if (signature == "FMAA")
            {
                Name = loader.LoadString();
                uint ShaderParamAnimOffset = loader.ReadOffset();
                uint TexturePatternAnimOffset = loader.ReadOffset();
                uint CurveOffset = loader.ReadOffset();
                uint ConstantAnimArrayOffset = loader.ReadOffset();
                ShaderParamCurveIndex = loader.ReadUInt16();
                TexturePatternCurveIndex = loader.ReadUInt16();
                VisualConstantIndex = loader.ReadUInt16();
                VisalCurveIndex = loader.ReadUInt16();
                BeginVisalConstantIndex = loader.ReadUInt16();
                ushort ShaderParamAnimCount = loader.ReadUInt16();
                ushort TexutrePatternAnimCount = loader.ReadUInt16();
                ushort ConstantAnimCount = loader.ReadUInt16();
                ushort CurveCount = loader.ReadUInt16();
                loader.Seek(6);

                Curves = loader.LoadList<AnimCurve>(CurveCount, CurveOffset);
                ParamAnimInfos = loader.LoadList<ParamAnimInfo>(ShaderParamAnimCount, ShaderParamAnimOffset);
                PatternAnimInfos = loader.LoadList<PatternAnimInfo>(TexutrePatternAnimCount, TexturePatternAnimOffset);
                Constants = loader.LoadCustom(() => loader.ReadAnimConstants(ConstantAnimCount), ConstantAnimArrayOffset);

                //Set base data list for texture patterns
                //Get the first value from either constants or curves
                BaseDataList = new ushort[PatternAnimInfos.Count];
                for (int i = 0; i < PatternAnimInfos.Count; i++)
                {
                    if (PatternAnimInfos[i].BeginConstant != ushort.MaxValue)
                        BaseDataList[i] = (ushort)((int)Constants[PatternAnimInfos[i].BeginConstant].Value);
                    else if (PatternAnimInfos[i].CurveIndex != -1)
                        BaseDataList[i] = (ushort)Curves[PatternAnimInfos[i].CurveIndex].Keys[0,0];
                }
            }
        }

        internal long PosParamInfoOffset;
        internal long PosTexPatInfoOffset;
        internal long PosCurvesOffset;
        internal long PosConstantsOffset;

        internal void Save(ResFileSaver saver, string signature)
        {
            if (signature == "FSHU")
            {
                saver.Write((ushort)ParamAnimInfos.Count);
                saver.Write((ushort)Curves.Count);
                if (Constants != null)
                    saver.Write((ushort)Constants.Count);
                else
                    saver.Write((ushort)0);
                saver.Seek(2);
                saver.Write(ShaderParamCurveIndex);
                saver.Write(InfoIndex);
                saver.SaveString(Name);
                PosParamInfoOffset = saver.SaveOffsetPos();
                PosCurvesOffset = saver.SaveOffsetPos();
                PosConstantsOffset = saver.SaveOffsetPos();
            }
            else if (signature == "FTXP")
            {
                saver.Write((ushort)PatternAnimInfos.Count);
                saver.Write((ushort)Curves.Count);
                saver.Write(TexturePatternCurveIndex);
                saver.Write(InfoIndex);
                saver.SaveString(Name);
                PosTexPatInfoOffset = saver.SaveOffsetPos();
                PosCurvesOffset = saver.SaveOffsetPos();
                PosConstantsOffset = saver.SaveOffsetPos();
            }
            else if (signature == "FMAA")
            {
                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 5, 1, 0, 1, "Material Animation Data");
                saver.SaveString(Name);
                PosParamInfoOffset = saver.SaveOffset();
                PosTexPatInfoOffset = saver.SaveOffset();
                PosCurvesOffset = saver.SaveOffset();
                PosConstantsOffset = saver.SaveOffset();
                saver.Write((ushort)ShaderParamCurveIndex);
                saver.Write((ushort)TexturePatternCurveIndex);
                saver.Write((ushort)VisualConstantIndex);
                saver.Write((ushort)VisalCurveIndex);
                saver.Write((ushort)BeginVisalConstantIndex);
                saver.Write((ushort)ParamAnimInfos.Count);
                saver.Write((ushort)PatternAnimInfos.Count);
                if (Constants != null)
                    saver.Write((ushort)Constants.Count);
                else
                    saver.Write((ushort)0);
                saver.Write((ushort)Curves.Count);
                saver.Seek(6);
            }
        }
    }
}
