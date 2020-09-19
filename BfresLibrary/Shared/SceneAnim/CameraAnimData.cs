using System.Runtime.InteropServices;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents the animatable data of scene cameras.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraAnimData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The near clipping plane distance.
        /// </summary>
        public float ClipNear;

        /// <summary>
        /// The far clipping plane distance.
        /// </summary>
        public float ClipFar;

        /// <summary>
        /// The aspect ratio of the projected image.
        /// </summary>
        public float AspectRatio;

        /// <summary>
        /// The field of view of the projected image.
        /// </summary>
        public float FieldOfView;

        /// <summary>
        /// The spatial position of the camera.
        /// </summary>
        public Vector3F Position;

        /// <summary>
        /// The spatial rotation of the camera.
        /// </summary>
        public Vector3F Rotation;

        /// <summary>
        /// The spatial twist of the camera.
        /// </summary>
        public float Twist;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        internal CameraAnimData(ResFileLoader loader)
        {
            ClipNear = loader.ReadSingle();
            ClipFar = loader.ReadSingle();
            AspectRatio = loader.ReadSingle();
            FieldOfView = loader.ReadSingle();
            Position = loader.ReadVector3F();
            Rotation = loader.ReadVector3F();
            Twist = loader.ReadSingle();
        }

        internal void Save(ResFileSaver saver)
        {
            saver.Write(ClipNear);
            saver.Write(ClipFar);
            saver.Write(AspectRatio);
            saver.Write(FieldOfView);
            saver.Write(Position);
            saver.Write(Rotation);
            saver.Write(Twist);
        }
    }

    /// <summary>
    /// Gets the <see cref="AnimCurve.AnimDataOffset"/> for <see cref="CameraAnimData"/> instances.
    /// </summary>
    public enum CameraAnimDataOffset : uint
    {
        /// <summary>
        /// Animates <see cref="CameraAnimData.ClipNear"/>.
        /// </summary>
        ClipNear = 0x00,

        /// <summary>
        /// Animates <see cref="CameraAnimData.ClipFar"/>.
        /// </summary>
        ClipFar = 0x04,

        /// <summary>
        /// Animates <see cref="CameraAnimData.AspectRatio"/>.
        /// </summary>
        AspectRatio = 0x08,

        /// <summary>
        /// Animates <see cref="CameraAnimData.FieldOfView"/>.
        /// </summary>
        FieldOFView = 0x0C,

        /// <summary>
        /// Animates the X component of <see cref="CameraAnimData.Position"/>.
        /// </summary>
        PositionX = 0x10,

        /// <summary>
        /// Animates the Y component of <see cref="CameraAnimData.Position"/>.
        /// </summary>
        PositionY = 0x14,

        /// <summary>
        /// Animates the Z component of <see cref="CameraAnimData.Position"/>.
        /// </summary>
        PositionZ = 0x18,

        /// <summary>
        /// Animates the X component of <see cref="CameraAnimData.Rotation"/>.
        /// </summary>
        RotationX = 0x1C,

        /// <summary>
        /// Animates the Y component of <see cref="CameraAnimData.Rotation"/>.
        /// </summary>
        RotationY = 0x20,

        /// <summary>
        /// Animates the Z component of <see cref="CameraAnimData.Rotation"/>.
        /// </summary>
        RotationZ = 0x24,

        /// <summary>
        /// Animates <see cref="CameraAnimData.Twist"/>.
        /// </summary>
        Twist = 0x28
    }
}
