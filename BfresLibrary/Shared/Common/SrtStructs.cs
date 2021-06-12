using System;
using Syroot.Maths;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a 2D transformation.
    /// </summary>
    [Serializable]
    public struct Srt2D
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of this structure.
        /// </summary>
        public const int SizeInBytes = Vector2F.SizeInBytes + sizeof(float) + Vector2F.SizeInBytes;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The scaling amount of the transformation.
        /// </summary>
        public Vector2F Scaling { get; set; }

        /// <summary>
        /// The rotation angle of the transformation.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector2F Translation { get; set; }
    }

    /// <summary>
    /// Represents a 3D transformation.
    /// </summary>
    [Serializable]
    public struct Srt3D
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of this structure.
        /// </summary>
        public const int SizeInBytes = Vector3F.SizeInBytes + Vector3F.SizeInBytes + Vector3F.SizeInBytes;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The scaling amount of the transformation.
        /// </summary>
        public Vector3F Scaling { get; set; }

        /// <summary>
        /// The rotation amount of the transformation.
        /// </summary>
        public Vector3F Rotation { get; set; }

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector3F Translation { get; set; }
    }

    /// <summary>
    /// Represents a 2D texture transformation.
    /// </summary>
    [Serializable]
    public struct TexSrt
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of this structure.
        /// </summary>
        public const int SizeInBytes = sizeof(TexSrtMode) + Vector2F.SizeInBytes + sizeof(float) + Vector2F.SizeInBytes;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The <see cref="TexSrtMode"/> with which the transformation is applied.
        /// </summary>
        public TexSrtMode Mode { get; set; }

        /// <summary>
        /// The scaling amount of the transformation.
        /// </summary>
        public Vector2F Scaling { get; set; }

        /// <summary>
        /// The rotation angle of the transformation.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector2F Translation { get; set; }
    }

    /// <summary>
    /// Represents the texture transformation mode used in <see cref="TexSrt"/> />.
    /// </summary>
    public enum TexSrtMode : uint
    {
        ModeMaya,
        Mode3dsMax,
        ModeSoftimage
    }
}
