using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BfresLibrary.Core;
using System.ComponentModel;
using BfresLibrary.Switch;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ResFile"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(Model) + " {" + nameof(Name) + "}")]
    public class Model : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model()
        {
            Name = "";
            Path = "";
            Skeleton = new Skeleton();
            VertexBuffers = new List<VertexBuffer>();
            Shapes = new ResDict<Shape>();
            Materials = new ResDict<Material>();
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FMDL";

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        internal uint Flags;

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Model}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the <see cref="Skeleton"/> instance to deform the model with animations.
        /// </summary>
        [Browsable(false)]
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VertexBuffer"/> instances storing the vertex data used by the
        /// <see cref="Shapes"/>.
        /// </summary>
        [Browsable(false)]
        public IList<VertexBuffer> VertexBuffers { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Shape"/> instances forming the surface of the model.
        /// </summary>
        [Browsable(false)]
        public ResDict<Shape> Shapes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Material"/> instance applied on the <see cref="Shapes"/> to color their surface.
        /// </summary>
        [Browsable(false)]
        public ResDict<Material> Materials { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<UserData> UserData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<MaterialParserV10.ShaderAssignV10> ShaderAssign = new List<MaterialParserV10.ShaderAssignV10>();

        /// <summary>
        /// Gets the total number of vertices to process when drawing this model.
        /// </summary>
        /// <remarks>This excludes vertices which are not processed by any shader. However, the exact value does not
        /// seem to matter, so the total count of all vertices is taken to keep things trivial for now.</remarks>
        public uint TotalVertexCount
        {
            get
            {
                uint count = 0;
                foreach (VertexBuffer vertexBuffer in VertexBuffers)
                {
                    count += vertexBuffer.VertexCount;
                }
                return count;
            }
        }

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
                Switch.ModelParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                Name = loader.LoadString();
                Path = loader.LoadString();
                Skeleton = loader.Load<Skeleton>();
                uint ofsVertexBufferList = loader.ReadOffset();
                Shapes = loader.LoadDict<Shape>();
                Materials = loader.LoadDict<Material>();
                UserData = loader.LoadDict<UserData>();
                ushort numVertexBuffer = loader.ReadUInt16();
                ushort numShape = loader.ReadUInt16();
                ushort numMaterial = loader.ReadUInt16();
                ushort numUserData = loader.ReadUInt16();
                uint totalVertexCount = loader.ReadUInt32();

                if (loader.ResFile.Version >= 0x03030000)
                {
                    uint userPointer = loader.ReadUInt32();
                }

                VertexBuffers = loader.LoadList<VertexBuffer>(numVertexBuffer, ofsVertexBufferList);
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.IsSwitch)
                Switch.ModelParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.SaveString(Name);
                saver.SaveString(Path);
                saver.Save(Skeleton);
                saver.SaveList(VertexBuffers);
                saver.SaveDict(Shapes);
                saver.SaveDict(Materials);
                saver.SaveDict(UserData);
                saver.Write((ushort)VertexBuffers.Count);
                saver.Write((ushort)Shapes.Count);
                saver.Write((ushort)Materials.Count);
                saver.Write((ushort)UserData.Count);
                if (saver.ResFile.Version >= 0x03030000) {
                    saver.Write(TotalVertexCount);
                }
                saver.Write(0); // UserPointer
            }
        }

        ///reserve offsets for saving
        internal long SkeletonOffset;
        internal long VertexBufferOffset;
        internal long ShapeOffset;
        internal long ShapeDictOffset;
        internal long MaterialsOffset;
        internal long MaterialsDictOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
    }
}