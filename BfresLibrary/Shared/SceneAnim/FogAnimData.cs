using System.Runtime.InteropServices;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents the animatable data of scene fog.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FogAnimData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The distance attenuation of the fog depth.
        /// </summary>
        public Vector2F DistanceAttn;

        /// <summary>
        /// The color of the fog.
        /// </summary>
        public Vector3F Color;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        internal FogAnimData(ResFileLoader loader)
        {
            DistanceAttn = loader.ReadVector2F();
            Color = loader.ReadVector3F();
        }

        internal void Save(ResFileSaver saver)
        {
            saver.Write(DistanceAttn);
            saver.Write(Color);
        }
    }

    /// <summary>
    /// Gets the <see cref="AnimCurve.AnimDataOffset"/> for <see cref="FogAnimData"/> instances.
    /// </summary>
    public enum FogAnimDataOffset : uint
    {
        /// <summary>
        /// Animates the X component of <see cref="FogAnimData.DistanceAttn"/>.
        /// </summary>
        DistanceAttnX = 0x00,

        /// <summary>
        /// Animates the Y component of <see cref="FogAnimData.DistanceAttn"/>.
        /// </summary>
        DistanceAttnY = 0x04,

        /// <summary>
        /// Animates the X (red) component of <see cref="FogAnimData.Color"/>.
        /// </summary>
        ColorR = 0x08,

        /// <summary>
        /// Animates the Y (green) component of <see cref="FogAnimData.Color"/>.
        /// </summary>
        ColorG = 0x0C,

        /// <summary>
        /// Animates the Z (blue) component of <see cref="FogAnimData.Color"/>.
        /// </summary>
        ColorB = 0x10
    }
}
