using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using BfresLibrary.Core;
using System.ComponentModel;
using Syroot.Maths;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FSHP section in a <see cref="Model"/> subfile.
    /// </summary>
    [DebuggerDisplay(nameof(Shape) + " {" + nameof(Name) + "}")]
    public class Shape : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        public Shape()
        {
            Name = "";
            Flags = ShapeFlags.HasVertexBuffer;
            MaterialIndex = 0;
            BoneIndex = 0;
            VertexBufferIndex = 0;
            RadiusArray = new List<float>();
            VertexSkinCount = 0;
            TargetAttribCount = 0;
            Meshes = new List<Mesh>();
            SkinBoneIndices = new List<ushort>();
            KeyShapes = new ResDict<KeyShape>();
            SubMeshBoundings = new List<Bounding>();
            SubMeshBoundingNodes = new List<BoundingNode>();
            SubMeshBoundingIndices = new List<ushort>();
            VertexBuffer = new VertexBuffer();
        }

        public void CreateEmptyMesh()
        {
            uint[] faces = new uint[6];
            faces[0] = 0;
            faces[1] = 1;
            faces[2] = 2;
            faces[3] = 1;
            faces[4] = 3;
            faces[5] = 2;

            var mesh = new Mesh();
            mesh.SetIndices(faces, GX2.GX2IndexFormat.UInt16);
            mesh.SubMeshes.Add(new SubMesh() { Count = 6 });
            Meshes = new List<Mesh>();
            Meshes.Add(mesh);

            RadiusArray.Add(1.0f);

            //Set boundings for mesh
            SubMeshBoundings = new List<Bounding>();
            SubMeshBoundings.Add(new Bounding()
            {
                Center = new Vector3F(0, 0, 0),
                Extent = new Vector3F(1, 1, 1)
            });
            SubMeshBoundings.Add(new Bounding() //One more bounding for sub mesh
            {
                Center = new Vector3F(0, 0, 0),
                Extent = new Vector3F(1, 1, 1)
            });
            SubMeshBoundingIndices = new List<ushort>();
            SubMeshBoundingIndices.Add(0);
            SubMeshBoundingNodes = new List<BoundingNode>();
            SubMeshBoundingNodes.Add(new BoundingNode()
            {
                LeftChildIndex = 0,
                NextSibling = 0,
                SubMeshIndex = 0,
                RightChildIndex = 0,
                Unknown = 0,
                SubMeshCount = 1,
            });
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSHP";

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets flags determining which data is available for this instance.
        /// </summary>
        [Browsable(false)]
        public ShapeFlags Flags { get; set; }

        [Category("Flags")]
        public bool HasVertexBuffer
        {
            get { return Flags.HasFlag(ShapeFlags.HasVertexBuffer); }
            set
            {
                if (value)
                    Flags |= ShapeFlags.HasVertexBuffer;
                else
                    Flags &= ShapeFlags.HasVertexBuffer;
            }
        }

        [Category("Flags")]
        public bool SubMeshBoundaryConsistent
        {
            get { return Flags.HasFlag(ShapeFlags.SubMeshBoundaryConsistent); }
            set
            {
                if (value)
                    Flags |= ShapeFlags.SubMeshBoundaryConsistent;
                else
                    Flags &= ShapeFlags.SubMeshBoundaryConsistent;
            }
        }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Shape}"/>
        /// instances.
        /// </summary>
        [Category("Polygon")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the index of the material to apply to the shapes surface in the owning
        /// <see cref="Model.Materials"/> list.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Material Index")]
        public ushort MaterialIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="Bone"/> to which this instance is directly attached to. The bone
        /// must be part of the skeleton referenced by the owning <see cref="Model.Skeleton"/> instance.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Bone Index")]
        public ushort BoneIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="VertexBuffer"/> in the owning <see cref="Model.VertexBuffers"/>
        /// list.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Vertex Buffer Index")]
        public ushort VertexBufferIndex { get; set; }

        /// <summary>
        /// Gets or sets the bounding radius/radii spanning the shape. BOTW uses multiple per LOD mesh.
        /// </summary>
        [Category("Visibility")]
        [DisplayName("Bounding Radius")]
        public List<float> RadiusArray { get; set; }

        public List<Vector4F> BoundingRadiusList { get; set; } = new List<Vector4F>();

        /// <summary>
        /// Gets or sets the number of bones influencing the vertices stored in this buffer. 0 influences equal
        /// rigidbodies (no skinning), 1 equal rigid skinning and 2 or more smooth skinning.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Max Vertex Skin Influence")]
        public byte VertexSkinCount { get; set; }

        /// <summary>
        /// Gets or sets a value with unknown purpose.
        /// </summary>
        [Category("Morph Data")]
        [DisplayName("Target Attribiute Count")]
        public byte TargetAttribCount { get; set; }


        /// <summary>
        /// Gets or sets the list of <see cref="Meshes"/> which are used to represent different level of details of the
        /// shape.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("LOD Meshes")]
        public List<Mesh> Meshes { get; set; }

        [Category("Bones")]
        [DisplayName("Bone Indices")]
        public List<ushort> SkinBoneIndices { get; set; }

        [Category("Morph Data")]
        [DisplayName("Key Shapes")]
        public ResDict<KeyShape> KeyShapes { get; set; }

        [Category("Visibility")]
        [DisplayName("Bounding Boxes")]
        public List<Bounding> SubMeshBoundings { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BoundingNode"/> instances forming the bounding tree with which parts of a mesh
        /// are culled when not visible.
        /// </summary>
        [Category("Visibility")]
        [DisplayName("Visibility Tree")]
        public List<BoundingNode> SubMeshBoundingNodes { get; set; }

        [Category("Visibility")]
        [DisplayName("Visibility Indices")]
        public List<ushort> SubMeshBoundingIndices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VertexBuffer"/> instance storing the data which forms the shape's surface. Saved
        /// depending on <see cref="VertexBufferIndex"/>.
        /// </summary>
        internal VertexBuffer VertexBuffer { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public Shape ShallowCopy()
        {
            return (Shape)this.MemberwiseClone();
        }

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
                Switch.ShapeParser.Read((Switch.Core.ResFileSwitchLoader)loader, this);
            else
            {
                Name = loader.LoadString();
                Flags = loader.ReadEnum<ShapeFlags>(true);
                ushort idx = loader.ReadUInt16();
                MaterialIndex = loader.ReadUInt16();
                BoneIndex = loader.ReadUInt16();
                VertexBufferIndex = loader.ReadUInt16();
                ushort numSkinBoneIndex = loader.ReadUInt16();
                VertexSkinCount = loader.ReadByte();
                byte numMesh = loader.ReadByte();
                byte numKeyShape = loader.ReadByte();
                TargetAttribCount = loader.ReadByte();
                ushort numSubMeshBoundingNodes = loader.ReadUInt16(); // Padding in engine.

                if (loader.ResFile.Version >= 0x04050000)
                {
                    RadiusArray = loader.LoadCustom(() => loader.ReadSingles(numMesh))?.ToList();
                }
                else
                {
                    RadiusArray = loader.ReadSingles(1).ToList();
                }
                VertexBuffer = loader.Load<VertexBuffer>();
                Meshes = loader.LoadList<Mesh>(numMesh).ToList();
                SkinBoneIndices = loader.LoadCustom(() => loader.ReadUInt16s(numSkinBoneIndex))?.ToList();
                KeyShapes = loader.LoadDict<KeyShape>();

                // TODO: At least BotW has more data following the Boundings, or that are no boundings at all.
                if (numSubMeshBoundingNodes == 0)
                {
                    if (loader.ResFile.Version >= 0x04050000)
                        numSubMeshBoundingNodes = (ushort)(Meshes.Count + Meshes.Sum(x => x.SubMeshes.Count));
                    else
                        numSubMeshBoundingNodes = (ushort)(1 + Meshes[0].SubMeshes.Count + 1);
                    SubMeshBoundings = loader.LoadCustom(() => loader.ReadBoundings(numSubMeshBoundingNodes))?.ToList();
                }
                else
                {
                    SubMeshBoundingNodes = loader.LoadList<BoundingNode>(numSubMeshBoundingNodes)?.ToList();
                    SubMeshBoundings = loader.LoadCustom(() => loader.ReadBoundings(numSubMeshBoundingNodes))?.ToList();
                    SubMeshBoundingIndices = loader.LoadCustom(() => loader.ReadUInt16s(numSubMeshBoundingNodes))?.ToList();
                }

                if (SubMeshBoundingNodes == null) SubMeshBoundingNodes = new List<BoundingNode>();
                if (SubMeshBoundings == null) SubMeshBoundings = new List<Bounding>();
                if (SubMeshBoundingIndices == null) SubMeshBoundingIndices = new List<ushort>();

                uint userPointer = loader.ReadUInt32();
            }   
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.IsSwitch)
                Switch.ShapeParser.Write((Switch.Core.ResFileSwitchSaver)saver, this);
            else
            {
                saver.SaveString(Name);
                saver.Write(Flags, true);
                saver.Write((ushort)saver.CurrentIndex);
                saver.Write(MaterialIndex);
                saver.Write(BoneIndex);
                saver.Write(VertexBufferIndex);
                saver.Write((ushort)SkinBoneIndices.Count);
                saver.Write(VertexSkinCount);
                saver.Write((byte)Meshes.Count);
                saver.Write((byte)KeyShapes.Count);
                saver.Write(TargetAttribCount);
                saver.Write((ushort)SubMeshBoundingNodes?.Count);
                if (saver.ResFile.Version >= 0x04050000)
                {
                    saver.SaveCustom(RadiusArray, () => saver.Write(RadiusArray));
                }
                else
                {
                    if (RadiusArray.Count > 0)
                        saver.Write(RadiusArray[0]);
                    else
                        saver.Write(0);
                }
                saver.Save(VertexBuffer);
                saver.SaveList(Meshes);
                saver.SaveCustom(SkinBoneIndices, () => saver.Write(SkinBoneIndices));
                saver.SaveDict(KeyShapes);
                if (SubMeshBoundingNodes.Count == 0)
                {
                    saver.SaveCustom(SubMeshBoundings, () => saver.Write(SubMeshBoundings));
                }
                else
                {
                    saver.SaveList(SubMeshBoundingNodes);
                    saver.SaveCustom(SubMeshBoundings, () => saver.Write(SubMeshBoundings));
                    saver.SaveCustom(SubMeshBoundingIndices, () => saver.Write(SubMeshBoundingIndices));
                }
                saver.Write(0); // UserPointer
            }
        }

        internal void WriteBoudnings(ResFileSaver saver)
        {
            saver.Write(SubMeshBoundings);
        }

        //Reserve offsets for saving
        internal long PosMeshArrayOffset;
        internal long PosSkinBoneIndicesOffset;
        internal long PosKeyShapesOffset;
        internal long PosKeyShapeDictOffset;
        internal long PosSubMeshBoundingsOffset;
        internal long PosRadiusArrayOffset;
    }

    /// <summary>
    /// Represents flags determining which data is available for <see cref="Shape"/> instances.
    /// </summary>
    [Flags]
    public enum ShapeFlags : uint
    {
        /// <summary>
        /// The <see cref="Shape"/> instance references a <see cref="VertexBuffer"/>.
        /// </summary>
        HasVertexBuffer = 1 << 1,

        /// <summary>
        /// The boundings in all submeshes are consistent.
        /// </summary>
        SubMeshBoundaryConsistent = 1 << 2
    }
}