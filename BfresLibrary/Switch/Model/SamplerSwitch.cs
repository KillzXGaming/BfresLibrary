using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using BfresLibrary.Core;
using System.ComponentModel;
using BfresLibrary.GX2;

namespace BfresLibrary
{
    internal class SamplerSwitch : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const ushort _flagsShrinkMask = 0b00000000_00110000;
        private const ushort _flagsExpandMask = 0b00000000_00001100;
        private const ushort _flagsMipmapMask = 0b00000000_00000011;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerSwitch"/> class.
        /// </summary>
        public SamplerSwitch()
        {
            WrapModeU = TexClamp.Repeat;
            WrapModeV = TexClamp.Repeat;
            WrapModeW = TexClamp.Clamp;
            CompareFunc = CompareFunction.Never;
            BorderColorType = TexBorderType.White;
            Anisotropic = MaxAnisotropic.Ratio_1_1;
            LODBias = 0;
            MinLOD = 0;
            MaxLOD = 13;
            _filterFlags = 42;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the UV wrap mode in the U direction
        /// </summary>
        [Description("The texture repetition mode on the X axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap X")]
        public TexClamp WrapModeU { get; set; }

        /// <summary>
        /// Gets or sets the UV wrap mode in the V direction
        /// </summary>
        [Description("The texture repetition mode on the Y axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap Y")]
        public TexClamp WrapModeV { get; set; }

        /// <summary>
        /// Gets or sets the UV wrap mode in the W direction
        /// </summary>
        [Description("The texture repetition mode on the Z axis.")]
        [Category("Wrap")]
        [DisplayName("Wrap Z")]
        public TexClamp WrapModeW { get; set; }

        /// <summary>
        /// Gets or sets the compare function
        /// </summary>
        [Description("The depth comparison function.")]
        [Category("Depth")]
        [DisplayName("Function")]
        public CompareFunction CompareFunc { get; set; }

        /// <summary>
        /// Gets or sets the border color
        /// </summary>
        [Description("Color to draw at places not reached by a texture if the clamp mode does not repeat it.")]
        [Category("Filter")]
        [DisplayName("Border Type")]
        public TexBorderType BorderColorType { get; set; }

        /// <summary>
        /// Gets or sets the max anisotropic filtering value
        /// </summary>
        [Description("The maximum anisotropic filtering level to use.")]
        [Category("Filter")]
        [DisplayName("Anisotropic Ratio")]
        public MaxAnisotropic Anisotropic { get; set; }

        private ushort _filterFlags;

        [Description("The texture filtering on the X and Y axes when the texture is drawn smaller than the actual texture's resolution.")]
        [Category("Filter")]
        [DisplayName("Shrink XY")]
        public ShrinkFilterModes ShrinkXY
        {
            get { return (ShrinkFilterModes)(_filterFlags & _flagsShrinkMask); }
            set { _filterFlags = (ushort)(_filterFlags & ~_flagsShrinkMask | (ushort)value); }
        }

        [Description("The texture filtering on the X and Y axes when the texture is drawn larger than the actual texture's resolution.")]
        [Category("Filter")]
        [DisplayName("Exapnd XY")]
        public ExpandFilterModes ExpandXY
        {
            get { return (ExpandFilterModes)(_filterFlags & _flagsExpandMask); }
            set { _filterFlags = (ushort)(_filterFlags & ~_flagsExpandMask | (ushort)value); }

        }

        [Description("The texture filtering for mipmaps.")]
        [Category("Filter")]
        [DisplayName("MipMap")]
        public MipFilterModes Mipmap
        {
            get { return (MipFilterModes)(_filterFlags & _flagsMipmapMask); }
            set { _filterFlags = (ushort)(_filterFlags & ~_flagsMipmapMask | (ushort)value); }
        }

        [Description("The minimum LoD level.")]
        [Category("LOD")]
        [DisplayName("Min")]
        public float MinLOD { get; set; }

        [Description("The maximum LoD level.")]
        [Category("LOD")]
        [DisplayName("Max")]
        public float MaxLOD { get; set; }

        [Description("The LoD bias.")]
        [Category("LOD")]
        [DisplayName("Max")]
        public float LODBias { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public TexSampler ToTexSampler()
        {
            TexSampler sampler = new TexSampler();
            sampler._filterFlags = _filterFlags;

            if (ClampModes.ContainsKey(WrapModeU)) sampler.ClampX = ClampModes[WrapModeU];
            if (ClampModes.ContainsKey(WrapModeV)) sampler.ClampY = ClampModes[WrapModeV];
            if (ClampModes.ContainsKey(WrapModeW)) sampler.ClampZ = ClampModes[WrapModeW];
            if (AnisotropicModes.ContainsKey(Anisotropic)) sampler.MaxAnisotropicRatio = AnisotropicModes[Anisotropic];
            if (CompareModes.ContainsKey(CompareFunc)) sampler.DepthCompareFunc = CompareModes[CompareFunc];
            if (BorderModes.ContainsKey(BorderColorType)) sampler.BorderType = BorderModes[BorderColorType];

            if (MipFilters.ContainsKey(Mipmap)) sampler.MipFilter = MipFilters[Mipmap];
            if (ExpandFilters.ContainsKey(ExpandXY)) sampler.MagFilter = ExpandFilters[ExpandXY];
            if (ShrinkFilters.ContainsKey(ShrinkXY)) sampler.MinFilter = ShrinkFilters[ShrinkXY];

            sampler.MaxLod = MaxLOD;
            sampler.MinLod = MinLOD;
            sampler.LodBias = LODBias;

            return sampler;
        }

        public void SaveTexSampler(ResFileSaver saver, TexSampler sampler)
        {
            _filterFlags = sampler._filterFlags;

            WrapModeU = ClampModes.FirstOrDefault(x => x.Value == sampler.ClampX).Key;
            WrapModeV = ClampModes.FirstOrDefault(x => x.Value == sampler.ClampY).Key;
            WrapModeW = ClampModes.FirstOrDefault(x => x.Value == sampler.ClampZ).Key;
            CompareFunc = CompareModes.FirstOrDefault(x => x.Value == sampler.DepthCompareFunc).Key;
            BorderColorType = BorderModes.FirstOrDefault(x => x.Value == sampler.BorderType).Key;
            Anisotropic = AnisotropicModes.FirstOrDefault(x => x.Value == sampler.MaxAnisotropicRatio).Key;
            Mipmap = MipFilters.FirstOrDefault(x => x.Value == sampler.MipFilter).Key;
            ExpandXY = ExpandFilters.FirstOrDefault(x => x.Value == sampler.MagFilter).Key;
            ShrinkXY = ShrinkFilters.FirstOrDefault(x => x.Value == sampler.MinFilter).Key;

         /*   _filterFlags = 0x0029;

            ShrinkXY = ShrinkFilterModes.Linear;
            ExpandXY = ExpandFilterModes.Linear;
            Mipmap = MipFilterModes.Points;
            BorderColorType = TexBorderType.White;*/

            MinLOD = sampler.MinLod;
            MaxLOD = sampler.MaxLod;
            LODBias = sampler.LodBias;

            ((IResData)this).Save(saver);
        }

        void IResData.Load(ResFileLoader loader)
        {
            WrapModeU = (TexClamp)loader.ReadByte();
            WrapModeV = (TexClamp)loader.ReadByte();
            WrapModeW = (TexClamp)loader.ReadByte();
            CompareFunc = (CompareFunction)loader.ReadByte();
            BorderColorType = (TexBorderType)loader.ReadByte();
            Anisotropic = (MaxAnisotropic)loader.ReadByte();
            _filterFlags = loader.ReadUInt16();
            MinLOD = loader.ReadSingle();
            MaxLOD = loader.ReadSingle();
            LODBias = loader.ReadSingle();
            loader.Seek(12);
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.Write((byte)WrapModeU);
            saver.Write((byte)WrapModeV);
            saver.Write((byte)WrapModeW);
            saver.Write((byte)CompareFunc);
            saver.Write((byte)BorderColorType);
            saver.Write((byte)Anisotropic);
            saver.Write(_filterFlags);
            saver.Write((float)MinLOD);
            saver.Write((float)MaxLOD);
            saver.Write((float)LODBias);
            saver.Seek(12);
        }

        Dictionary<ExpandFilterModes, GX2TexXYFilterType> ExpandFilters = new Dictionary<ExpandFilterModes, GX2TexXYFilterType>()
        {
            { ExpandFilterModes.Linear, GX2TexXYFilterType.Bilinear },
            { ExpandFilterModes.Points, GX2TexXYFilterType.Point },
        };

        Dictionary<ShrinkFilterModes, GX2TexXYFilterType> ShrinkFilters = new Dictionary<ShrinkFilterModes, GX2TexXYFilterType>()
        {
            { ShrinkFilterModes.Linear, GX2TexXYFilterType.Bilinear },
            { ShrinkFilterModes.Points, GX2TexXYFilterType.Point },
        };

        Dictionary<MipFilterModes, GX2TexMipFilterType> MipFilters = new Dictionary<MipFilterModes, GX2TexMipFilterType>()
        {
            { MipFilterModes.Linear, GX2TexMipFilterType.Linear },
            { MipFilterModes.Points, GX2TexMipFilterType.Point },
            { MipFilterModes.None, GX2TexMipFilterType.NoMip },
        };

        Dictionary<TexBorderType, GX2TexBorderType> BorderModes = new Dictionary<TexBorderType, GX2TexBorderType>()
        {
            { TexBorderType.Opaque, GX2TexBorderType.SolidBlack },
            { TexBorderType.White, GX2TexBorderType.SolidWhite },
            { TexBorderType.Transparent, GX2TexBorderType.ClearBlack },
        };

        Dictionary<CompareFunction, GX2CompareFunction> CompareModes = new Dictionary<CompareFunction, GX2CompareFunction>()
        {
               { CompareFunction.Always, GX2CompareFunction.Always },
               { CompareFunction.Equal, GX2CompareFunction.Equal },
               { CompareFunction.Greater, GX2CompareFunction.Greater },
               { CompareFunction.GreaterOrEqual, GX2CompareFunction.GreaterOrEqual },
               { CompareFunction.Less, GX2CompareFunction.Less },
               { CompareFunction.LessOrEqual, GX2CompareFunction.LessOrEqual },
               { CompareFunction.Never, GX2CompareFunction.Never },
               { CompareFunction.NotEqual, GX2CompareFunction.NotEqual },
        };

        Dictionary<TexClamp, GX2TexClamp> ClampModes = new Dictionary<TexClamp, GX2TexClamp>()
        {
            { TexClamp.Repeat, GX2TexClamp.Wrap },
            { TexClamp.Mirror, GX2TexClamp.Mirror },
            { TexClamp.MirrorOnce, GX2TexClamp.MirrorOnce },
            { TexClamp.MirrorOnceClampToEdge, GX2TexClamp.MirrorOnceBorder },
            { TexClamp.Clamp, GX2TexClamp.Clamp },
            { TexClamp.ClampToEdge, GX2TexClamp.ClampBorder },
        };

        Dictionary<MaxAnisotropic, GX2TexAnisoRatio> AnisotropicModes = new Dictionary<MaxAnisotropic, GX2TexAnisoRatio>()
        {
            { MaxAnisotropic.Ratio_1_1, GX2TexAnisoRatio.Ratio_1_1 },
            { MaxAnisotropic.Ratio_2_1, GX2TexAnisoRatio.Ratio_2_1 },
            { MaxAnisotropic.Ratio_4_1, GX2TexAnisoRatio.Ratio_4_1 },
            { MaxAnisotropic.Ratio_8_1, GX2TexAnisoRatio.Ratio_8_1 },
            { MaxAnisotropic.Ratio_16_1, GX2TexAnisoRatio.Ratio_16_1 },
        };

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