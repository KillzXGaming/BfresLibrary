using System.Diagnostics;
using BfresLibrary.Core;
using BfresLibrary.GX2;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a <see cref="Texture"/> sampler in a <see cref="UserData"/> section, storing configuration on how to
    /// draw and interpolate textures.
    /// </summary>
    [DebuggerDisplay(nameof(Sampler) + " {" + nameof(Name) + "}")]
    public class Sampler : IResData, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sampler"/> class.
        /// </summary>
        public Sampler()
        {
    
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the internal representation of the sampler configuration.
        /// </summary>
        public TexSampler TexSampler { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Sampler}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        private SamplerSwitch sampler;

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                sampler = new SamplerSwitch();
                ((IResData)sampler).Load(loader);
                TexSampler = sampler.ToTexSampler();
            }
            else
            {
                TexSampler = new TexSampler(loader.ReadUInt32s(3));
                uint handle = loader.ReadUInt32();
                Name = loader.LoadString();
                byte idx = loader.ReadByte();
                loader.Seek(3);
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                sampler = new SamplerSwitch();
                sampler.SaveTexSampler(saver, TexSampler);
            }
            else
            {
                saver.Write(TexSampler.Values);
                saver.Write(0); // Handle
                saver.SaveString(Name);
                saver.Write((byte)saver.CurrentIndex);
                saver.Seek(3);
            }
        }

        public enum MaxAnisotropic : byte
        {
            Ratio_1_1 = 0x1,
            Ratio_2_1 = 0x2,
            Ratio_4_1 = 0x4,
            Ratio_8_1 = 0x8,
            Ratio_16_1 = 0x10,
        }

        public enum MipFilterModes : ushort
        {
            None = 0,
            Points = 1,
            Linear = 2,
        }

        public enum ExpandFilterModes : ushort
        {
            Points = 1 << 2,
            Linear = 2 << 2,
        }

        public enum ShrinkFilterModes : ushort
        {
            Points = 1 << 4,
            Linear = 2 << 4,
        }

        /// <summary>
        /// Represents compare functions used for depth and stencil tests.
        /// </summary>
        public enum CompareFunction : byte
        {
            Never,
            Less,
            Equal,
            LessOrEqual,
            Greater,
            NotEqual,
            GreaterOrEqual,
            Always
        }

        /// <summary>
        /// Represents type of border color to use.
        /// </summary>
        public enum TexBorderType : byte
        {
            White,
            Transparent,
            Opaque,
        }

        /// <summary>
        /// Represents how to treat texture coordinates outside of the normalized coordinate texture range.
        /// </summary>
        public enum TexClamp : sbyte
        {
            Repeat,
            Mirror,
            Clamp,
            ClampToEdge,
            MirrorOnce,
            MirrorOnceClampToEdge,
        }
    }
}