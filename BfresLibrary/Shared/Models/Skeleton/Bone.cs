using System;
using System.Diagnostics;
using Syroot.Maths;
using System.Collections.Generic;
using BfresLibrary.Core;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a single bone in a <see cref="Skeleton"/> section, storing its initial transform and transformation
    /// effects.
    /// </summary>
    [DebuggerDisplay(nameof(Bone) + " {" + nameof(Name) + "}")]
    public class Bone : IResData, INamed, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bone"/> class.
        /// </summary>
        public Bone()
        {
            Name = "";
            UserData = new ResDict<UserData>();
            ParentIndex = -1;
            SmoothMatrixIndex = -1;
            RigidMatrixIndex = -1;
            BillboardIndex = -1;

            Scale = new Vector3F(1, 1, 1);
            Rotation = new Vector4F(0, 0, 0, 0);
            Position = new Vector3F(0, 0, 0);

            Flags = BoneFlags.Visible;
            FlagsRotation = BoneFlagsRotation.EulerXYZ;
            FlagsBillboard = BoneFlagsBillboard.None;
            FlagsTransform = BoneFlagsTransform.None;
            FlagsTransformCumulative = BoneFlagsTransformCumulative.None;
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const uint _flagsMask = 0b00000000_00000000_00000000_00000001;
        private const uint _flagsMaskRotate = 0b00000000_00000000_01110000_00000000;
        private const uint _flagsMaskBillboard = 0b00000000_00000111_00000000_00000000;
        private const uint _flagsMaskTransform = 0b00001111_00000000_00000000_00000000;
        private const uint _flagsMaskTransformCumulative = 0b11110000_00000000_00000000_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Bone}"/>
        /// instances.
        /// </summary>
        [Category("Name")]
        [Browsable(true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the index of the parent <see cref="Bone"/> this instance is a child of.
        /// </summary>
        [Category("Tree")]
        [Browsable(true)]
        public short ParentIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of a matrix used for smooth skinning.
        /// </summary>
        [Category("Matrices")]
        [Browsable(true)]
        public short SmoothMatrixIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of a matrix used for rigid skinning.
        /// </summary>
        [Category("Matrices")]
        [Browsable(true)]
        public short RigidMatrixIndex { get; set; }

        [Category("Billboard")]
        [DisplayName("Billboard ID")]
        [Browsable(true)]
        public short BillboardIndex { get; set; }

        [Category("Flags")]
        [Browsable(true)]
        public bool Visible
        {
            get { return Flags.HasFlag(BoneFlags.Visible); }
            set
            {
                if (value)
                    Flags |= BoneFlags.Visible;
                else
                    Flags &= BoneFlags.Visible;
            }
        }

        /// <summary>
        /// Gets or sets flags controlling bone behavior.
        /// </summary>
        [Browsable(false)]
        public BoneFlags Flags
        {
            get { return (BoneFlags)(_flags & _flagsMask); }
            set { _flags = _flags & ~_flagsMask | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the rotation method used to store bone rotations in <see cref="Rotation"/>.
        /// </summary>
        [Category("Transform Mode")]
        [DisplayName("Rotation Mode")]
        [Browsable(true)]
        public BoneFlagsRotation FlagsRotation
        {
            get { return (BoneFlagsRotation)(_flags & _flagsMaskRotate); }
            set { _flags = _flags & ~_flagsMaskRotate | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the billboard transformation applied to the bone.
        /// </summary>
        [Category("Billboard")]
        [DisplayName("Billboard Type")]
        [Browsable(true)]
        public BoneFlagsBillboard FlagsBillboard
        {
            get { return (BoneFlagsBillboard)(_flags & _flagsMaskBillboard); }
            set { _flags = _flags & ~_flagsMaskBillboard | (uint)value; }
        }

        [Category("Transform Mode")]
        [Browsable(true)]
        public BoneFlagsTransform FlagsTransform
        {
            get { return (BoneFlagsTransform)(_flags & _flagsMaskTransform); }
            set { _flags = _flags & ~_flagsMaskTransform | (uint)value; }
        }

        [Category("Transform Mode")]
        [Browsable(true)]
        public BoneFlagsTransformCumulative FlagsTransformCumulative
        {
            get { return (BoneFlagsTransformCumulative)(_flags & _flagsMaskTransformCumulative); }
            set { _flags = _flags & ~_flagsMaskTransformCumulative | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the spatial scale of the bone.
        /// </summary>
        [Category("Transformation")]
        [Browsable(true)]
        public Vector3F Scale { get; set; }

        /// <summary>
        /// Gets or sets the spatial rotation of the bone. If <see cref="BoneFlagsRotation.EulerXYZ"/> is used, the
        /// fourth component is always <c>1.0f</c>.
        /// </summary>
        [Category("Transformation")]
        [Browsable(true)]
        public Vector4F Rotation { get; set; }

        /// <summary>
        /// Gets or sets the spatial position of the bone.
        /// </summary>
        [Category("Transformation")]
        [Browsable(true)]
        public Vector3F Position { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(true)]
        public ResDict<UserData> UserData { get; set; }

        public List<UserData> UserDataList
        {
            get
            {
                return UserData.Values.ToList();
            }
        }

        /// <summary>
        /// Gets or sets the inverse matrix (Only used in bfres verson v3.3.X.X and below)
        /// </summary>
        [Browsable(false)]
        public Matrix3x4 InverseMatrix { get; set; }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformIdentity
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.Identity); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.Identity;
                else
                    FlagsTransform &= ~BoneFlagsTransform.Identity;
                UpdateTransformFlagProperties();
            }
        }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformRotateTranslateZero
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.RotateTranslateZero); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.RotateTranslateZero;
                else
                    FlagsTransform &= ~BoneFlagsTransform.RotateTranslateZero;
                UpdateTransformFlagProperties();
            }
        }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformRotateZero
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.RotateZero); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.RotateZero;
                else
                    FlagsTransform &= ~BoneFlagsTransform.RotateZero;
                UpdateTransformFlagProperties();
            }
        }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformScaleOne
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.ScaleOne); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.ScaleOne;
                else
                    FlagsTransform &= ~BoneFlagsTransform.ScaleOne;
                UpdateTransformFlagProperties();
            }
        }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformScaleUniform
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.ScaleUniform); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.ScaleUniform;
                else
                    FlagsTransform &= ~BoneFlagsTransform.ScaleUniform;
                UpdateTransformFlagProperties();
            }
        }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformScaleVolumeOne
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.ScaleVolumeOne); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.ScaleVolumeOne;
                else
                    FlagsTransform &= ~BoneFlagsTransform.ScaleVolumeOne;
                UpdateTransformFlagProperties();
            }
        }

        [Category("Transform Mode")]
        [Browsable(false)]
        public bool TransformTranslateZero
        {
            get { return FlagsTransform.HasFlag(BoneFlagsTransform.TranslateZero); }
            set
            {
                if (value == true)
                    FlagsTransform |= BoneFlagsTransform.TranslateZero;
                else
                    FlagsTransform &= ~BoneFlagsTransform.TranslateZero;
                UpdateTransformFlagProperties();
            }
        }

        private void UpdateTransformFlagProperties()
        {
            NotifyPropertyChanged(nameof(TransformScaleOne));
            NotifyPropertyChanged(nameof(TransformScaleUniform));
            NotifyPropertyChanged(nameof(TransformScaleVolumeOne));
            NotifyPropertyChanged(nameof(TransformRotateZero));
            NotifyPropertyChanged(nameof(TransformRotateTranslateZero));
            NotifyPropertyChanged(nameof(TransformTranslateZero));
            NotifyPropertyChanged(nameof(TransformIdentity));
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeIdentity
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.Identity); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.Identity;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.Identity;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeRotateTranslateZero
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.RotateTranslateZero); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.RotateTranslateZero;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.RotateTranslateZero;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeRotateZero
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.RotateZero); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.RotateZero;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.RotateZero;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeScaleOne
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.ScaleOne); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.ScaleOne;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.ScaleOne;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeScaleUniform
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.ScaleUniform); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.ScaleUniform;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.ScaleUniform;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeScaleVolumeOne
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.ScaleVolumeOne); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.ScaleVolumeOne;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.ScaleVolumeOne;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        [Category("Transform Mode Cumulative")]
        [Browsable(false)]
        public bool TransformCumulativeTranslateZero
        {
            get { return FlagsTransformCumulative.HasFlag(BoneFlagsTransformCumulative.TranslateZero); }
            set
            {
                if (value == true)
                    FlagsTransformCumulative |= BoneFlagsTransformCumulative.TranslateZero;
                else
                    FlagsTransformCumulative &= ~BoneFlagsTransformCumulative.TranslateZero;
                UpdateCumulativeTransformFlagProperties();
            }
        }

        private void UpdateCumulativeTransformFlagProperties()
        {
            NotifyPropertyChanged(nameof(TransformCumulativeScaleOne));
            NotifyPropertyChanged(nameof(TransformCumulativeScaleUniform));
            NotifyPropertyChanged(nameof(TransformCumulativeScaleVolumeOne));
            NotifyPropertyChanged(nameof(TransformCumulativeRotateZero));
            NotifyPropertyChanged(nameof(TransformCumulativeRotateTranslateZero));
            NotifyPropertyChanged(nameof(TransformCumulativeTranslateZero));
            NotifyPropertyChanged(nameof(TransformCumulativeIdentity));
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                Name = loader.LoadString();
                UserData = loader.LoadDictValues<UserData>();
                if (loader.ResFile.VersionMajor2 > 9)
                    loader.Seek(8);
                if (loader.ResFile.VersionMajor2 == 8 || loader.ResFile.VersionMajor2 == 9)
                    loader.Seek(16);

                ushort idx = loader.ReadUInt16();
                ParentIndex = loader.ReadInt16();
                SmoothMatrixIndex = loader.ReadInt16();
                RigidMatrixIndex = loader.ReadInt16();
                BillboardIndex = loader.ReadInt16();
                ushort numUserData = loader.ReadUInt16();
                _flags = loader.ReadUInt32();
                Scale = loader.ReadVector3F();
                Rotation = loader.ReadVector4F();
                Position = loader.ReadVector3F();
            }
            else
            {
                Name = loader.LoadString();
                ushort idx = loader.ReadUInt16();
                ParentIndex = loader.ReadInt16();
                SmoothMatrixIndex = loader.ReadInt16();
                RigidMatrixIndex = loader.ReadInt16();
                BillboardIndex = loader.ReadInt16();
                ushort numUserData = loader.ReadUInt16();
                _flags = loader.ReadUInt32();
                Scale = loader.ReadVector3F();
                Rotation = loader.ReadVector4F();
                Position = loader.ReadVector3F();
                UserData = loader.LoadDict<UserData>();

                if (loader.ResFile.Version < 0x03040000) {
                    InverseMatrix = loader.ReadMatrix3x4();
                }
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                saver.SaveString(Name);
                PosUserDataOffset = saver.SaveOffset();
                PosUserDataDictOffset = saver.SaveOffset();
                if (saver.ResFile.VersionMajor2 > 9)
                    saver.Seek(8);
                else if (saver.ResFile.VersionMajor2 == 8 || saver.ResFile.VersionMajor2 == 9)
                    saver.Seek(16);

                saver.Write((ushort)saver.CurrentIndex);
                saver.Write(ParentIndex);
                saver.Write(SmoothMatrixIndex);
                saver.Write(RigidMatrixIndex);
                saver.Write(BillboardIndex);
                saver.Write((ushort)UserData.Count);
                saver.Write(_flags);
                saver.Write(Scale);
                saver.Write(Rotation);
                saver.Write(Position);
            }
            else
            {
                if (InverseMatrix == null)
                    InverseMatrix = new Syroot.Maths.Matrix3x4();

                saver.SaveString(Name);
                saver.Write((ushort)saver.CurrentIndex);
                saver.Write(ParentIndex);
                saver.Write(SmoothMatrixIndex);
                saver.Write(RigidMatrixIndex);
                saver.Write(BillboardIndex);
                saver.Write((ushort)UserData.Count);
                saver.Write(_flags);
                saver.Write(Scale);
                saver.Write(Rotation);
                saver.Write(Position);
                saver.SaveDict(UserData);

                if (saver.ResFile.Version < 0x03040000)
                {
                    saver.Write(InverseMatrix);
                }   
            }
        }

        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Represents flags controlling bone behavior.
    /// </summary>
    public enum BoneFlags : uint
    {
        /// <summary>
        /// Set when the bone is visible.
        /// </summary>
        Visible = 1 << 0
    }

    /// <summary>
    /// Represents the rotation method used to store bone rotations.
    /// </summary>
    public enum BoneFlagsRotation : uint
    {
        /// <summary>
        /// A quaternion represents the rotation.
        /// </summary>
        Quaternion,

        /// <summary>
        /// A <see cref="Vector3F"/> represents the Euler rotation in XYZ order.
        /// </summary>
        EulerXYZ = 1 << 12
    }

    /// <summary>
    /// Represents the possible transformations for bones to handle them as billboards.
    /// </summary>
    public enum BoneFlagsBillboard : uint
    {
        /// <summary>
        /// No transformation is applied.
        /// </summary>
        None,

        /// <summary>
        /// Transforms of the child are applied.
        /// </summary>
        Child = 1 << 16,

        /// <summary>
        /// Transforms the Z axis parallel to the camera.
        /// </summary>
        WorldViewVector = 2 << 16,

        /// <summary>
        /// Transforms the Z axis parallel to the direction of the camera.
        /// </summary>
        WorldViewPoint = 3 << 16,

        /// <summary>
        /// Transforms the Y axis parallel to the camera up vector, and the Z parallel to the camera up-vector.
        /// </summary>
        ScreenViewVector = 4 << 16,

        /// <summary>
        /// Transforms the Y axis parallel to the camera up vector, and the Z axis parallel to the direction of the
        /// camera.
        /// </summary>
        ScreenViewPoint = 5 << 16,

        /// <summary>
        /// Transforms the Z axis parallel to the camera by rotating only the Y axis.
        /// </summary>
        YAxisViewVector = 6 << 16,

        /// <summary>
        /// Transforms the Z axis parallel to the direction of the camera by rotating only the Y axis.
        /// </summary>
        YAxisViewPoint = 7 << 16
    }

    [Flags]
    public enum BoneFlagsTransform : uint
    {
        None,
        ScaleUniform = 1 << 24,
        ScaleVolumeOne = 1 << 25,
        RotateZero = 1 << 26,
        TranslateZero = 1 << 27,
        ScaleOne = ScaleUniform | ScaleVolumeOne,
        RotateTranslateZero = RotateZero | TranslateZero,
        Identity = ScaleOne | RotateZero | TranslateZero
    }

    [Flags]
    public enum BoneFlagsTransformCumulative : uint
    {
        None,
        ScaleUniform = 1 << 28,
        ScaleVolumeOne = 1 << 29,
        RotateZero = 1 << 30,
        TranslateZero = 1u << 31,
        ScaleOne = ScaleVolumeOne | ScaleUniform,
        RotateTranslateZero = RotateZero | TranslateZero,
        Identity = ScaleOne | RotateZero | TranslateZero
    }
}