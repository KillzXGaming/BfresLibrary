using System.Runtime.InteropServices;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents the animatable data of a <see cref="Bone"/> instance.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoneAnimData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        public uint Flags;

        /// <summary>
        /// The scaling of the bone.
        /// </summary>
        public Vector3F Scale;

        /// <summary>
        /// The translation of the bone.
        /// </summary>
        public Vector3F Translate;

        /// <summary>
        /// An unused field.
        /// </summary>
        public uint Padding;

        /// <summary>
        /// The rotation of the bone.
        /// </summary>
        public Vector4F Rotate;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        internal BoneAnimData(ResFileLoader loader, BoneAnimFlagsBase flags)
        {
            Flags = 0; // Never in files.
            Scale = flags.HasFlag(BoneAnimFlagsBase.Scale) ? loader.ReadVector3F() : Vector3F.Zero;
            Rotate = flags.HasFlag(BoneAnimFlagsBase.Rotate) ? loader.ReadVector4F() : Vector4F.Zero;
            Padding = 0; // Never in files.
            Translate = flags.HasFlag(BoneAnimFlagsBase.Translate) ? loader.ReadVector3F() : Vector3F.Zero;
        }

        internal void Save(ResFileSaver saver, BoneAnimFlagsBase flags)
        {
            if (flags.HasFlag(BoneAnimFlagsBase.Scale)) saver.Write(Scale);
            if (flags.HasFlag(BoneAnimFlagsBase.Rotate)) saver.Write(Rotate);
            if (flags.HasFlag(BoneAnimFlagsBase.Translate)) saver.Write(Translate);
        }
    }

    /// <summary>
    /// Gets the <see cref="AnimCurve.AnimDataOffset"/> for <see cref="BoneAnimData"/> instances.
    /// </summary>
    public enum BoneAnimDataOffset : uint
    {
        /// <summary>
        /// Animates <see cref="BoneAnimData.Flags"/> (never seen in files).
        /// </summary>
        Flags = 0x00,

        /// <summary>
        /// Animates the X component of <see cref="BoneAnimData.Scale"/>.
        /// </summary>
        ScaleX = 0x04,

        /// <summary>
        /// Animates the Y component of <see cref="BoneAnimData.Scale"/>.
        /// </summary>
        ScaleY = 0x08,

        /// <summary>
        /// Animates the Z component of <see cref="BoneAnimData.Scale"/>.
        /// </summary>
        ScaleZ = 0x0C,

        /// <summary>
        /// Animates the X component of <see cref="BoneAnimData.Translate"/>.
        /// </summary>
        TranslateX = 0x10,

        /// <summary>
        /// Animates the Y component of <see cref="BoneAnimData.Translate"/>.
        /// </summary>
        TranslateY = 0x14,

        /// <summary>
        /// Animates the Z component of <see cref="BoneAnimData.Translate"/>.
        /// </summary>
        TranslateZ = 0x18,

        ///// <summary>
        ///// Animates <see cref="BoneAnimData.Padding"/>.
        ///// </summary>
        // Padding = 0x1C,

        /// <summary>
        /// Animates the X component of <see cref="BoneAnimData.Rotate"/>.
        /// </summary>
        RotateX = 0x20,

        /// <summary>
        /// Animates the Y component of <see cref="BoneAnimData.Rotate"/>.
        /// </summary>
        RotateY = 0x24,

        /// <summary>
        /// Animates the Z component of <see cref="BoneAnimData.Rotate"/>.
        /// </summary>
        RotateZ = 0x28,

        /// <summary>
        /// Animates the W component of <see cref="BoneAnimData.Rotate"/>.
        /// </summary>
        RotateW = 0x2C
    }
}
