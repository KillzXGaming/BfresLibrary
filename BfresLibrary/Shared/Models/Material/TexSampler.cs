using Syroot.Maths;
using BfresLibrary.Core;
using System.ComponentModel;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents a GX2 texture sampler controlling how a texture is samples and drawn onto a surface.
    /// </summary>
    public class TexSampler
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _clampXBit = 0, _clampXBits = 3;
        private const int _clampYBit = 3, _clampYBits = 3;
        private const int _clampZBit = 6, _clampZBits = 3;
        private const int _xyMagFilterBit = 9, _xyMagFilterBits = 2;
        private const int _xyMinFilterBit = 12, _xyMinFilterBits = 2;
        private const int _zFilterBit = 15, _zFilterBits = 2;
        private const int _mipFilterBit = 17, _mipFilterBits = 2;
        private const int _maxAnisotropicRatioBit = 19, _maxAnisotropicRatioBits = 3;
        private const int _borderTypeBit = 22, _borderTypeBits = 2;
        private const int _depthCompareFuncBit = 26, _depthCompareFuncBits = 3;

        private const int _minLodBit = 0, _minLodBits = 10;
        private const int _maxLodBit = 10, _maxLodBits = 10;
        private const int _lodBiasBit = 20, _lodBiasBits = 12;

        private const int _depthCompareBit = 30;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TexSampler"/> instance.
        /// </summary>
        public TexSampler()
        {
            Values = new uint[3]
            {
                33559049,
                851968,
                2147483648,
            };

            ClampX = GX2TexClamp.Wrap;
            ClampY = GX2TexClamp.Wrap;
            ClampZ = GX2TexClamp.Clamp;
            MagFilter = GX2TexXYFilterType.Bilinear;
            MinFilter = GX2TexXYFilterType.Bilinear;
            ZFilter = GX2TexZFilterType.Linear;
            MipFilter = GX2TexMipFilterType.Linear;
            MaxAnisotropicRatio = GX2TexAnisoRatio.Ratio_1_1;
            BorderType = GX2TexBorderType.ClearBlack;
            DepthCompareFunc = GX2CompareFunction.Never;
            LodBias = 0;
            MinLod = 0;
            MaxLod = 13;
        }

        internal TexSampler(uint[] values)
        {
            Values = values;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the texture repetition mode on the X axis.
        /// </summary>
        [Description("The texture repetition mode on the X axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap X")]
        public GX2TexClamp ClampX
        {
            get { return (GX2TexClamp)Values[0].Decode(_clampXBit, _clampXBits); }
            set { Values[0] = Values[0].Encode((uint)value, _clampXBit, _clampXBits); }
        }

        /// <summary>
        /// Gets or sets the texture repetition mode on the Y axis.
        /// </summary>
        [Description("The texture repetition mode on the Y axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap Y")]
        public GX2TexClamp ClampY
        {
            get { return (GX2TexClamp)Values[0].Decode(_clampYBit, _clampYBits); }
            set { Values[0] = Values[0].Encode((uint)value, _clampYBit, _clampYBits); }
        }

        /// <summary>
        /// Gets or sets the texture repetition mode on the Z axis.
        /// </summary>
        [Description("The texture repetition mode on the Z axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap Z")]
        public GX2TexClamp ClampZ
        {
            get { return (GX2TexClamp)Values[0].Decode(_clampZBit, _clampZBits); }
            set { Values[0] = Values[0].Encode((uint)value, _clampZBit, _clampZBits); }
        }

        /// <summary>
        /// Gets or sets the texture filtering on the X and Y axes when the texture is drawn larger than the actual
        /// texture's resolution.
        /// </summary>
        [Description("The texture filtering on the X and Y axes when the texture is drawn larger than the actual texture's resolution.")]
        [Category("Filter")]
        [DisplayName("Expand XY")]
        public GX2TexXYFilterType MagFilter
        {
            get { return (GX2TexXYFilterType)Values[0].Decode(_xyMagFilterBit, _xyMagFilterBits); }
            set { Values[0] = Values[0].Encode((uint)value, _xyMagFilterBit, _xyMagFilterBits); }
        }

        /// <summary>
        /// Gets or sets the texture filtering on the X and Y axes when the texture is drawn smaller than the actual
        /// texture's resolution.
        /// </summary>
        [Description("The texture filtering on the X and Y axes when the texture is drawn smaller than the actual texture's resolution.")]
        [Category("Filter")]
        [DisplayName("Shrink XY")]
        public GX2TexXYFilterType MinFilter
        {
            get { return (GX2TexXYFilterType)Values[0].Decode(_xyMinFilterBit, _xyMinFilterBits); }
            set { Values[0] = Values[0].Encode((uint)value, _xyMinFilterBit, _xyMinFilterBits); }
        }

        /// <summary>
        /// Gets or sets the texture filtering on the Z axis.
        /// </summary>
        [Description("The texture filtering on the Z axis.")]
        [Category("Filter")]
        [DisplayName("Texture W")]
        public GX2TexZFilterType ZFilter
        {
            get { return (GX2TexZFilterType)Values[0].Decode(_zFilterBit, _zFilterBits); }
            set { Values[0] = Values[0].Encode((uint)value, _zFilterBit, _zFilterBits); }
        }

        /// <summary>
        /// Gets or sets the texture filtering for mipmaps.
        /// </summary>
        [Description("The texture filtering for mipmaps.")]
        [Category("Filter")]
        [DisplayName("MipMap")]
        public GX2TexMipFilterType MipFilter
        {
            get { return (GX2TexMipFilterType)Values[0].Decode(_mipFilterBit, _mipFilterBits); }
            set { Values[0] = Values[0].Encode((uint)value, _mipFilterBit, _mipFilterBits); }
        }

        /// <summary>
        /// Gets or sets the maximum anisotropic filtering level to use.
        /// </summary>
        [Description("The maximum anisotropic filtering level to use.")]
        [Category("Filter")]
        [DisplayName("Anisotropic Ratio")]
        public GX2TexAnisoRatio MaxAnisotropicRatio
        {
            get { return (GX2TexAnisoRatio)Values[0].Decode(_maxAnisotropicRatioBit, _maxAnisotropicRatioBits); }
            set { Values[0] = Values[0].Encode((uint)value, _maxAnisotropicRatioBit, _maxAnisotropicRatioBits); }
        }

        /// <summary>
        /// Gets or sets what color to draw at places not reached by a texture if the clamp mode does not repeat it.
        /// </summary>
        [Description("Color to draw at places not reached by a texture if the clamp mode does not repeat it.")]
        [Category("Filter")]
        [DisplayName("Border Type")]
        public GX2TexBorderType BorderType
        {
            get { return (GX2TexBorderType)Values[0].Decode(_borderTypeBit, _borderTypeBits); }
            set { Values[0] = Values[0].Encode((uint)value, _borderTypeBit, _borderTypeBits); }
        }

        /// <summary>
        /// Gets or sets the depth comparison function.
        /// </summary>
        [Description("The depth comparison function.")]
        [Category("Depth")]
        [DisplayName("Function")]
        public GX2CompareFunction DepthCompareFunc
        {
            get { return (GX2CompareFunction)Values[0].Decode(_depthCompareFuncBit, _depthCompareFuncBits); }
            set { Values[0] = Values[0].Encode((uint)value, _depthCompareFuncBit, _depthCompareFuncBits); }
        }

        /// <summary>
        /// Gets or sets the minimum LoD level.
        /// </summary>
        /// 
        [Description("The minimum LoD level.")]
        [Category("LOD")]
        [DisplayName("Min")]
        public float MinLod
        {
            get { return USingle4x6ToSingle((ushort)Values[1].Decode(_minLodBit, _minLodBits)); }
            set { Values[1] = Values[1].Encode(SingleToUSingle4x6(value), _minLodBit, _minLodBits); }
        }

        /// <summary>
        /// Gets or sets the maximum LoD level.
        /// </summary>
        [Description("The maximum LoD level.")]
        [Category("LOD")]
        [DisplayName("Max")]
        public float MaxLod
        {
            get { return USingle4x6ToSingle((ushort)Values[1].Decode(_maxLodBit, _maxLodBits)); }
            set { Values[1] = Values[1].Encode(SingleToUSingle4x6(value), _maxLodBit, _maxLodBits); }
        }

        /// <summary>
        /// Gets or sets the LoD bias.
        /// </summary>
        [Description("The LoD bias.")]
        [Category("LOD")]
        [DisplayName("Bias")]
        public float LodBias
        {
            get { return Single5x6ToSingle(Values[1].Decode(_lodBiasBit, _lodBiasBits)); }
            set { Values[1] = Values[1].Encode(SingleToSingle5x6(value), _lodBiasBit, _lodBiasBits); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether depth comparison is enabled (never set for a real console).
        /// </summary>
        [Description("Indicates whether depth comparison is enabled (never set for a real console.")]
        [Category("Depth")]
        [DisplayName("Enabled")]
        public bool DepthCompareEnabled
        {
            get { return Values[2].GetBit(_depthCompareBit); }
            set { Values[2] = Values[2].SetBit(_depthCompareBit, value); }
        }

        internal ushort _filterFlags;

        internal uint[] Values { get; set; }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private float Single5x6ToSingle(uint value)
        {
            // Use a signed value to get arithmetic right shifts to receive correct negative numbers.
            int signed = (int)(value << 20);
            return (float)(signed >> 20) / 64;
        }

        private float USingle4x6ToSingle(ushort value)
        {
            return value / 64f;
        }

        private uint SingleToSingle5x6(float value)
        {
            return (uint)(Algebra.Clamp(value, -32, 31.984375f) * 64);
        }

        private uint SingleToUSingle4x6(float value)
        {
            return (uint)(Algebra.Clamp(value, 0, 13) * 64);
        }
    }
}
