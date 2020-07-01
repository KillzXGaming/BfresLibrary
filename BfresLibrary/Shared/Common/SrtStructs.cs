using Syroot.Maths;

namespace Syroot.NintenTools.Bfres
{
    /// <summary>
    /// Represents a 2D transformation.
    /// </summary>
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
        public Vector2F Scaling;

        /// <summary>
        /// The rotation angle of the transformation.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector2F Translation;
    }

    /// <summary>
    /// Represents a 3D transformation.
    /// </summary>
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
        public Vector3F Scaling;

        /// <summary>
        /// The rotation amount of the transformation.
        /// </summary>
        public Vector3F Rotation;

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector3F Translation;
    }

    /// <summary>
    /// Represents a 2D texture transformation.
    /// </summary>
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
        public TexSrtMode Mode;

        /// <summary>
        /// The scaling amount of the transformation.
        /// </summary>
        public Vector2F Scaling;

        /// <summary>
        /// The rotation angle of the transformation.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector2F Translation;

        /// <summary>
        /// Unkown space used in some games
        /// </summary>
        public byte[] unkown;
    }

    /// <summary>
    /// Represents a 2D texture transformation which is multiplied by a 3x4 matrix referenced at runtime by the
    /// <see cref="MatrixPointer"/>.
    /// </summary>
    public struct TexSrtEx
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of this structure.
        /// </summary>
        public const int SizeInBytes = TexSrt.SizeInBytes + sizeof(uint);

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The <see cref="TexSrtMode"/> with which the transformation is applied.
        /// </summary>
        public TexSrtMode Mode;

        /// <summary>
        /// The scaling amount of the transformation.
        /// </summary>
        public Vector2F Scaling;

        /// <summary>
        /// The rotation angle of the transformation.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The translation amount of the transformation.
        /// </summary>
        public Vector2F Translation;

        /// <summary>
        /// A pointer to a 3x4 matrix to multiply the transformation with. Set at runtime.
        /// </summary>
        public uint MatrixPointer;
    }

    /// <summary>
    /// Represents the texture transformation mode used in <see cref="TexSrt"/> and <see cref="TexSrtEx"/>.
    /// </summary>
    public enum TexSrtMode : uint
    {
        ModeMaya,
        Mode3dsMax,
        ModeSoftimage
    }
}
