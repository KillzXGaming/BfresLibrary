using System.Collections.Generic;
using Syroot.Maths;
using BfresLibrary.Core;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FSKL section in a <see cref="Model"/> subfile, storing armature data.
    /// </summary>
    public class Skeleton : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skeleton"/> class.
        /// </summary>
        public Skeleton()
        {
            MatrixToBoneList = new List<ushort>();
            InverseModelMatrices = new List<Matrix3x4>();
            Bones = new ResDict<Bone>();
            FlagsRotation = SkeletonFlagsRotation.EulerXYZ;
            FlagsScaling = SkeletonFlagsScaling.Maya;
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSKL";

        private const uint _flagsScalingMask = 0b00000000_00000000_00000011_00000000;
        private const uint _flagsRotationMask = 0b00000000_00000000_01110000_00000000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        [Category("Bones")]
        [DisplayName("Scale Mode")]
        public SkeletonFlagsScaling FlagsScaling
        {
            get { return (SkeletonFlagsScaling)(_flags & _flagsScalingMask); }
            set { _flags = _flags & ~_flagsScalingMask | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the rotation method used to store bone rotations.
        /// </summary>
        [Category("Bones")]
        [DisplayName("Rotation Mode")]
        public SkeletonFlagsRotation FlagsRotation
        {
            get { return (SkeletonFlagsRotation)(_flags & _flagsRotationMask); }
            set { _flags = _flags & ~_flagsRotationMask | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Bone"/> instances forming the skeleton.
        /// </summary>
        [Browsable(false)]
        public ResDict<Bone> Bones { get; set; }

        public List<Bone> BoneList
        {
            get
            {
                return Bones.Values.ToList();
            }
        }

        [Browsable(false)]
        public IList<ushort> MatrixToBoneList { get; set; }

        [Category("Matrices")]
        [DisplayName("Inverse Matrices")]
        public List<Matrix3x4> InverseModelMatrices { get; set; }

        public IList<ushort> GetSmoothIndices()
        {
            List<ushort> indices = new List<ushort>();
            foreach (Bone bone in Bones.Values)
            {
                if (bone.SmoothMatrixIndex != -1)
                    indices.Add((ushort)bone.SmoothMatrixIndex);
            }
            return indices;
        }

        public IList<ushort> GetRigidIndices()
        {
            List<ushort> indices = new List<ushort>();
            foreach (Bone bone in Bones.Values)
            {
                if (bone.RigidMatrixIndex != -1)
                    indices.Add((ushort)bone.RigidMatrixIndex);
            }
            return indices;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.IsSwitch)
            {
                if (loader.ResFile.VersionMajor2 >= 9)
                    _flags = loader.ReadUInt32();
                else
                    ((Switch.Core.ResFileSwitchLoader)loader).LoadHeaderBlock();

                long BoneDictOffset = loader.ReadOffset();
                long BoneArrayOffset = loader.ReadOffset();
                Bones = loader.LoadDictValues<Bone>(BoneDictOffset, BoneArrayOffset);

                uint MatrixToBoneListOffset = loader.ReadOffset();
                uint InverseModelMatricesOffset = loader.ReadOffset();

                if (loader.ResFile.VersionMajor2 == 8)
                    loader.Seek(16);
                if (loader.ResFile.VersionMajor2 >= 9)
                    loader.Seek(8);

                long userPointer = loader.ReadInt64();
                if (loader.ResFile.VersionMajor2 < 9)
                    _flags = loader.ReadUInt32();
                ushort numBone = loader.ReadUInt16();
                ushort numSmoothMatrix = loader.ReadUInt16();
                ushort numRigidMatrix = loader.ReadUInt16();
                loader.Seek(6);

                MatrixToBoneList = loader.LoadCustom(() => loader.ReadUInt16s((numSmoothMatrix + numRigidMatrix)), MatrixToBoneListOffset);
                InverseModelMatrices = loader.LoadCustom(() => loader.ReadMatrix3x4s(numSmoothMatrix), InverseModelMatricesOffset)?.ToList();
            }
            else
            {
                _flags = loader.ReadUInt32();
                ushort numBone = loader.ReadUInt16();
                ushort numSmoothMatrix = loader.ReadUInt16();
                ushort numRigidMatrix = loader.ReadUInt16();
                loader.Seek(2);
                Bones = loader.LoadDict<Bone>();
                uint ofsBoneList = loader.ReadOffset(); // Only load dict.
                MatrixToBoneList = loader.LoadCustom(() => loader.ReadUInt16s((numSmoothMatrix + numRigidMatrix)));
                if (loader.ResFile.Version >= 0x03040000)
                    InverseModelMatrices = loader.LoadCustom(() => loader.ReadMatrix3x4s(numSmoothMatrix))?.ToList();
                uint userPointer = loader.ReadUInt32();
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (InverseModelMatrices == null)
                InverseModelMatrices = new List<Matrix3x4>();
            if (MatrixToBoneList == null)
                MatrixToBoneList = new List<ushort>();

            saver.WriteSignature(_signature);
            if (saver.IsSwitch)
            {
                if (saver.ResFile.VersionMajor2 >= 9)
                    saver.Write(_flags);
                else
                    ((Switch.Core.ResFileSwitchSaver)saver).SaveHeaderBlock();

                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 4, 1, 0, Switch.Core.ResFileSwitchSaver.Section1, "FSKL"); 
                PosBoneDictOffset = saver.SaveOffset();
                PosBoneArrayOffset = saver.SaveOffset();
                PosMatrixToBoneListOffset = saver.SaveOffset();
                PosInverseModelMatricesOffset = saver.SaveOffset();
                if (saver.ResFile.VersionMajor2 == 8)
                    saver.Seek(16);
                if (saver.ResFile.VersionMajor2 >= 9)
                    saver.Seek(8);

                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 1, 1, 0, Switch.Core.ResFileSwitchSaver.Section1, "FSKL UserPointer");
                saver.Write(0L); // UserPointer

                if (saver.ResFile.VersionMajor2 < 9)
                    saver.Write(_flags);

                saver.Write((ushort)Bones.Count);
                saver.Write((ushort)InverseModelMatrices.Count); // NumSmoothMatrix
                saver.Write((ushort)(MatrixToBoneList.Count - InverseModelMatrices.Count)); // NumRigidMatrix

                if (saver.ResFile.VersionMajor2 >= 9)
                    saver.Seek(2);
                else
                    saver.Seek(6);
            }
            else
            {
                saver.Write(_flags);
                saver.Write((ushort)Bones.Count);
                if (saver.ResFile.Version >= 0x03040000)
                {
                    if (InverseModelMatrices == null)
                        InverseModelMatrices = new List<Matrix3x4>();

                    saver.Write((ushort)InverseModelMatrices.Count); // NumSmoothMatrix
                    saver.Write((ushort)(MatrixToBoneList.Count - InverseModelMatrices.Count)); // NumRigidMatrix
                }
                else
                {
                    int numRididMatrix = 0;
                    foreach (Bone bn in Bones.Values)
                    {
                        if (bn.RigidMatrixIndex != -1)
                            numRididMatrix++;
                    }

                    saver.Write((ushort)(MatrixToBoneList.Count - numRididMatrix)); // NumRigidMatrix
                    saver.Write((ushort)(numRididMatrix)); // NumRigidMatrix
                }
                saver.Seek(2);
                saver.SaveDict(Bones);
                saver.SaveList(Bones.Values);
                saver.SaveCustom(MatrixToBoneList, () => saver.Write(MatrixToBoneList));
                if (saver.ResFile.Version >= 0x03040000)
                    saver.SaveCustom(InverseModelMatrices, () => saver.Write(InverseModelMatrices));
                saver.Write(0); // UserPointer

            }
        }

        internal long PosBoneDictOffset;
        internal long PosBoneArrayOffset;
        internal long PosMatrixToBoneListOffset;
        internal long PosInverseModelMatricesOffset;
    }

    public enum SkeletonFlagsScaling : uint
    {
        None,
        Standard = 1 << 8,
        Maya = 2 << 8,
        Softimage = 3 << 8
    }

    /// <summary>
    /// Represents the rotation method used to store bone rotations.
    /// </summary>
    public enum SkeletonFlagsRotation : uint
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
}
