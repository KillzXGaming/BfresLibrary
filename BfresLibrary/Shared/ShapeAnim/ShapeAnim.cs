using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FSHA subfile in a <see cref="ResFile"/>, storing shape animations of a <see cref="Model"/>
    /// instance.
    /// </summary>
    [DebuggerDisplay(nameof(ShapeAnim) + " {" + nameof(Name) + "}")]
    public class ShapeAnim : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeAnim"/> class.
        /// </summary>
        public ShapeAnim()
        {
            Name = "";
            Path = "";
            Flags = 0;
            BindModel = new Model();
            BindIndices = new ushort[0];
            VertexShapeAnims = new List<VertexShapeAnim>();
            FrameCount = 0;
            BakedSize = 0;
            BindIndices = new ushort[0];
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSHA";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{ShapeAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public ShapeAnimFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="AnimCurve"/> instances of all
        /// <see cref="VertexShapeAnims"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets or sets the indices of the <see cref="Shape"/> instances in the <see cref="Model.Shapes"/> dictionary
        /// to bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VertexShapeAnim"/> instances creating the animation.
        /// </summary>
        public IList<VertexShapeAnim> VertexShapeAnims { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict<UserData> UserData { get; set; }

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
                Switch.ShapeAnimParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                Name = loader.LoadString();
                Path = loader.LoadString();
                Flags = loader.ReadEnum<ShapeAnimFlags>(true);

                ushort numUserData;
                ushort numVertexShapeAnim;
                ushort numKeyShapeAnim;
                ushort numCurve;
                if (loader.ResFile.Version >= 0x03040000)
                {
                    numUserData = loader.ReadUInt16();
                    FrameCount = loader.ReadInt32();
                    numVertexShapeAnim = loader.ReadUInt16();
                    numKeyShapeAnim = loader.ReadUInt16();
                    numCurve = loader.ReadUInt16();
                    loader.Seek(2);
                    BakedSize = loader.ReadUInt32();
                }
                else
                {
                    FrameCount = loader.ReadUInt16();
                    numVertexShapeAnim = loader.ReadUInt16();
                    numKeyShapeAnim = loader.ReadUInt16();
                    numUserData = loader.ReadUInt16();
                    numCurve = loader.ReadUInt16();
                    BakedSize = loader.ReadUInt32();
                }

                BindModel = loader.Load<Model>();
                BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numVertexShapeAnim));
                VertexShapeAnims = loader.LoadList<VertexShapeAnim>(numVertexShapeAnim);
                UserData = loader.LoadDict<UserData>();
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.IsSwitch)
                Switch.ShapeAnimParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Write(Flags, true);

                if (saver.ResFile.Version >= 0x03040000)
                {
                    saver.Write((ushort)UserData.Count);
                    saver.Write(FrameCount);
                    saver.Write((ushort)VertexShapeAnims.Count);
                    saver.Write((ushort)VertexShapeAnims.Sum((x) => x.KeyShapeAnimInfos.Count));
                    saver.Write((ushort)VertexShapeAnims.Sum((x) => x.Curves.Count));
                    saver.Seek(2);
                    saver.Write(BakedSize);
                }
                else
                {
                    saver.Write((ushort)FrameCount);
                    saver.Write((ushort)VertexShapeAnims.Count);
                    saver.Write((ushort)VertexShapeAnims.Sum((x) => x.KeyShapeAnimInfos.Count));
                    saver.Write((ushort)UserData.Count);
                    saver.Write((ushort)VertexShapeAnims.Sum((x) => x.Curves.Count));
                    saver.Write(BakedSize);
                }
                saver.Save(BindModel);
                saver.SaveCustom(BindIndices, () => saver.Write(BindIndices));
                saver.SaveList(VertexShapeAnims);
                saver.SaveDict(UserData);
            }
        }

        internal long PosBindModelOffset;
        internal long PosBindIndicesOffset;
        internal long PosVertexShapeAnimsOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
    }

    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum ShapeAnimFlags : ushort
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