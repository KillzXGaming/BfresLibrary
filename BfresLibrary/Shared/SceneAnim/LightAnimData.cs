using System.Runtime.InteropServices;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents the animatable data of scene lighting.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LightAnimData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Enables or disables the light in total.
        /// </summary>
        public int Enable;

        /// <summary>
        /// The spatial origin of the light source for point or spot lights.
        /// </summary>
        public Vector3F Position;

        /// <summary>
        /// The spatial rotation of the light source.
        /// </summary>
        public Vector3F Rotation;

        /// <summary>
        /// The distance attenuation of the light.
        /// </summary>
        public Vector2F DistanceAttn;

        /// <summary>
        /// The angle attenuation of the light in degrees.
        /// </summary>
        public Vector2F AngleAttn;

        /// <summary>
        /// The first light source color.
        /// </summary>
        public Vector3F Color0;

        /// <summary>
        /// The second light source color.
        /// </summary>
        public Vector3F Color1;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        internal LightAnimData(ResFileLoader loader, LightAnimField flags)
        {
            Enable = flags.HasFlag(LightAnimField.Enable) ? loader.ReadInt32() : 0;
            Position = flags.HasFlag(LightAnimField.Position) ? loader.ReadVector3F() : Vector3F.Zero;
            Rotation = flags.HasFlag(LightAnimField.Rotation) ? loader.ReadVector3F() : Vector3F.Zero;
            DistanceAttn = flags.HasFlag(LightAnimField.DistanceAttn) ? loader.ReadVector2F() : Vector2F.Zero;
            AngleAttn = flags.HasFlag(LightAnimField.AngleAttn) ? loader.ReadVector2F() : Vector2F.Zero;
            Color0 = flags.HasFlag(LightAnimField.Color0) ? loader.ReadVector3F() : Vector3F.Zero;
            Color1 = flags.HasFlag(LightAnimField.Color1) ? loader.ReadVector3F() : Vector3F.Zero;
        }

        internal void Save(ResFileSaver saver, LightAnimField flags)
        {
            if (flags.HasFlag(LightAnimField.Enable)) saver.Write(Enable);
            if (flags.HasFlag(LightAnimField.Position)) saver.Write(Position);
            if (flags.HasFlag(LightAnimField.Rotation)) saver.Write(Rotation);
            if (flags.HasFlag(LightAnimField.DistanceAttn)) saver.Write(DistanceAttn);
            if (flags.HasFlag(LightAnimField.AngleAttn)) saver.Write(AngleAttn);
            if (flags.HasFlag(LightAnimField.Color0)) saver.Write(Color0);
            if (flags.HasFlag(LightAnimField.Color1)) saver.Write(Color1);
        }
    }

    /// <summary>
    /// Gets the <see cref="AnimCurve.AnimDataOffset"/> for <see cref="LightAnimData"/> instances.
    /// </summary>
    public enum LightAnimDataOffset : uint
    {
        /// <summary>
        /// Animates <see cref="LightAnimData.Enable"/>.
        /// </summary>
        Enable = 0x00,

        /// <summary>
        /// Animates the X component of <see cref="LightAnimData.Position"/>.
        /// </summary>
        PositionX = 0x04,

        /// <summary>
        /// Animates the Y component of <see cref="LightAnimData.Position"/>.
        /// </summary>
        PositionY = 0x08,

        /// <summary>
        /// Animates the Z component of <see cref="LightAnimData.Position"/>.
        /// </summary>
        PositionZ = 0x0C,

        /// <summary>
        /// Animates the X component of <see cref="LightAnimData.Rotation"/>.
        /// </summary>
        RotationX = 0x10,

        /// <summary>
        /// Animates the Y component of <see cref="LightAnimData.Rotation"/>.
        /// </summary>
        RotationY = 0x14,

        /// <summary>
        /// Animates the Z component of <see cref="LightAnimData.Rotation"/>.
        /// </summary>
        RotationZ = 0x18,

        /// <summary>
        /// Animates the X component of <see cref="LightAnimData.DistanceAttn"/>.
        /// </summary>
        DistanceAttnX = 0x1C,

        /// <summary>
        /// Animates the Y component of <see cref="LightAnimData.DistanceAttn"/>.
        /// </summary>
        DistanceAttnY = 0x20,

        /// <summary>
        /// Animates the X component of <see cref="LightAnimData.AngleAttn"/>.
        /// </summary>
        AngleAttnX = 0x24,

        /// <summary>
        /// Animates the Y component of <see cref="LightAnimData.AngleAttn"/>.
        /// </summary>
        AngleAttnY = 0x28,

        /// <summary>
        /// Animates the X (red) component of <see cref="LightAnimData.Color0"/>.
        /// </summary>
        Color0R = 0x2C,

        /// <summary>
        /// Animates the Y (green) component of <see cref="LightAnimData.Color0"/>.
        /// </summary>
        Color0G = 0x30,

        /// <summary>
        /// Animates the Z (blue) component of <see cref="LightAnimData.Color0"/>.
        /// </summary>
        Color0B = 0x34,

        /// <summary>
        /// Animates the X (red) component of <see cref="LightAnimData.Color1"/>.
        /// </summary>
        Color1R = 0x38,

        /// <summary>
        /// Animates the Y (green) component of <see cref="LightAnimData.Color1"/>.
        /// </summary>
        Color1G = 0x3C,

        /// <summary>
        /// Animates the Z (blue) component of <see cref="LightAnimData.Color1"/>.
        /// </summary>
        Color1B = 0x40
    }
}
