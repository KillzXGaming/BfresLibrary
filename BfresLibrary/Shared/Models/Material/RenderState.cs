using Syroot.Maths;
using BfresLibrary.Core;
using BfresLibrary.GX2;
using System.ComponentModel;

namespace BfresLibrary
{
    /// <summary>
    /// Represents GX2 GPU configuration to determine how polygons are rendered.
    /// </summary>
    public class RenderState : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderState"/> class.
        /// </summary>
        public RenderState()
        {
            FlagsMode = RenderStateFlagsMode.Opaque;
            FlagsBlendMode = RenderStateFlagsBlendMode.None;

            PolygonControl = new PolygonControl()
            {
                CullBack = false,
                CullFront = false,
                FrontFace = GX2FrontFaceMode.CounterClockwise,
                PolygonModeBack = GX2PolygonMode.Triangle,
                PolygonModeFront = GX2PolygonMode.Triangle,
                PolygonOffsetBackEnabled = false,
                PolygonOffsetFrontEnabled = false,
                PolygonLineOffsetEnabled = false,
                PolygonModeEnabled = true,
            };

            AlphaControl = new AlphaControl()
            {
                AlphaFunc = GX2CompareFunction.GreaterOrEqual,
            };

            DepthControl = new DepthControl()
            {
                BackStencilEnabled = false,
                BackStencilFail = GX2StencilFunction.Replace,
                BackStencilFunc = GX2CompareFunction.Always,
                BackStencilZFail = GX2StencilFunction.Replace,
                BackStencilZPass = GX2StencilFunction.Replace,
                DepthFunc = GX2CompareFunction.LessOrEqual,
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                FrontStencilFail = GX2StencilFunction.Replace,
                FrontStencilFunc = GX2CompareFunction.Always,
                FrontStencilZFail = GX2StencilFunction.Replace,
                FrontStencilZPass = GX2StencilFunction.Replace,
                StencilTestEnabled = false,
            };

            AlphaRefValue = 0.5F;


            BlendControl = new BlendControl()
            {
                AlphaCombine = GX2BlendCombine.Add,
                AlphaDestinationBlend = GX2BlendFunction.Zero,
                AlphaSourceBlend = GX2BlendFunction.Zero,
                ColorCombine = GX2BlendCombine.Add,
                ColorDestinationBlend = GX2BlendFunction.OneMinusSourceAlpha,
                ColorSourceBlend = GX2BlendFunction.SourceAlpha,
                SeparateAlphaBlend = true,
            };

            BlendColor = new Vector4F(0, 0, 0, 0);
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const uint _flagsMaskMode = 0b00000000_00000000_00000000_00000011;
        private const uint _flagsMaskBlendMode = 0b00000000_00000000_00000000_00110000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        [Category("Alpha Control")]
        [DisplayName("Render State")]
        public RenderStateFlagsMode FlagsMode
        {
            get { return (RenderStateFlagsMode)(_flags & _flagsMaskMode); }
            set { _flags = _flags & ~_flagsMaskMode | (uint)value; }
        }

        [DisplayName("Blend Mode")]
        [Category("Blend Control")]
        public RenderStateFlagsBlendMode FlagsBlendMode
        {
            get { return (RenderStateFlagsBlendMode)(_flags & _flagsMaskBlendMode); }
            set { _flags = _flags & ~_flagsMaskBlendMode | (uint)value; }
        }

        private AlphaControl _alphaControl;
        private BlendControl _blendControl;
        private PolygonControl _polygonControl;
        private ColorControl _colorControl;
        private DepthControl _depthControl;

        //A class that can display all the properties
        class Properties
        {

            #region AlphaControl Properties

            private AlphaControl _alphaControl;

            [Browsable(true)]
            [Description("Indicates whether alpha testing is enabled at all.")]
            [DisplayName("AlphaTestEnabled")]
            [Category("Alpha Control")]
            public bool AlphaTestEnabled
            {
                get { return _alphaControl.AlphaTestEnabled; }
                set { _alphaControl.AlphaTestEnabled = value; }
            }

            [Browsable(true)]
            [Description("The comparison functions to use for alpha testing.")]
            [DisplayName("AlphaFunc")]
            [Category("Alpha Control")]
            public GX2CompareFunction AlphaFunc
            {
                get { return _alphaControl.AlphaFunc; }
                set { _alphaControl.AlphaFunc = value; }
            }

            #endregion

            #region BlendControl Properties

            private BlendControl _blendControl;

            [Browsable(true)]
            [Description("The color source blend operation.")]
            [DisplayName("ColorSourceBlend")]
            [Category("Blend Control")]
            public GX2BlendFunction ColorSourceBlend
            {
                get { return _blendControl.ColorSourceBlend; }
                set { _blendControl.ColorSourceBlend = value; }
            }

            [Browsable(true)]
            [Description("The color combine operation.")]
            [DisplayName("ColorCombine")]
            [Category("Blend Control")]
            public GX2BlendCombine ColorCombine
            {
                get { return _blendControl.ColorCombine; }
                set { _blendControl.ColorCombine = value; }
            }

            [Browsable(true)]
            [Description("The color destination blend operation.")]
            [DisplayName("ColorDestinationBlend")]
            [Category("Blend Control")]
            public GX2BlendFunction ColorDestinationBlend
            {
                get { return _blendControl.ColorDestinationBlend; }
                set { _blendControl.ColorDestinationBlend = value; }
            }

            [Browsable(true)]
            [Description("The alpha source blend operation.")]
            [DisplayName("AlphaSourceBlend")]
            [Category("Blend Control")]
            public GX2BlendFunction AlphaSourceBlend
            {
                get { return _blendControl.AlphaSourceBlend; }
                set { _blendControl.AlphaSourceBlend = value; }
            }

            [Browsable(true)]
            [Description("The alpha combine operation.")]
            [DisplayName("AlphaCombine")]
            [Category("Blend Control")]
            public GX2BlendCombine AlphaCombine
            {
                get { return _blendControl.AlphaCombine; }
                set { _blendControl.AlphaCombine = value; }
            }

            [Browsable(true)]
            [Description("The alpha destination blend operation.")]
            [DisplayName("AlphaDestinationBlend")]
            [Category("Blend Control")]
            public GX2BlendFunction AlphaDestinationBlend
            {
                get { return _blendControl.AlphaDestinationBlend; }
                set { _blendControl.AlphaDestinationBlend = value; }
            }

            [Browsable(true)]
            [Description("Indicates whether alpha blending is separated from color blending.")]
            [DisplayName("SeparateAlphaBlend")]
            [Category("Blend Control")]
            public bool SeparateAlphaBlend
            {
                get { return _blendControl.SeparateAlphaBlend; }
                set { _blendControl.SeparateAlphaBlend = value; }
            }

            #endregion

            #region PolygonControl Properties

            private PolygonControl _polygonControl;

            [Browsable(true)]
            [Description("indicates whether front-facing polygons are culled.")]
            [DisplayName("CullFront")]
            [Category("Polygon Control")]
            public bool CullFront
            {
                get { return _polygonControl.CullFront; }
                set { _polygonControl.CullFront = value; }
            }

            [Browsable(true)]
            [Description("indicates whether back-facing polygons are culled.")]
            [DisplayName("CullBack")]
            [Category("Polygon Control")]
            public bool CullBack
            {
                get { return _polygonControl.CullBack; }
                set { _polygonControl.CullBack = value; }
            }

            [Browsable(true)]
            [Description("The order in which vertices have to form the triangle to be handled as a front- rather than back-face.")]
            [DisplayName("FrontFace")]
            [Category("Polygon Control")]
            public GX2FrontFaceMode FrontFace
            {
                get { return _polygonControl.FrontFace; }
                set { _polygonControl.FrontFace = value; }
            }

            [Browsable(true)]
            [Description("Indicates whether polygons are drawn at all.")]
            [DisplayName("PolygonModeEnabled")]
            [Category("Polygon Control")]
            public bool PolygonModeEnabled
            {
                get { return _polygonControl.PolygonModeEnabled; }
                set { _polygonControl.PolygonModeEnabled = value; }
            }

            [Browsable(true)]
            [Description("How front facing polygons are drawn.")]
            [DisplayName("PolygonModeFront")]
            [Category("Polygon Control")]
            public GX2PolygonMode PolygonModeFront
            {
                get { return _polygonControl.PolygonModeFront; }
                set { _polygonControl.PolygonModeFront = value; }
            }

            [Browsable(true)]
            [Description("How back facing polygons are drawn.")]
            [DisplayName("PolygonModeBack")]
            [Category("Polygon Control")]
            public GX2PolygonMode PolygonModeBack
            {
                get { return _polygonControl.PolygonModeBack; }
                set { _polygonControl.PolygonModeBack = value; }
            }

            [Browsable(true)]
            [Description("Whether front-facing polygons are drawn offset(useful for decals to combat Z fighting).")]
            [DisplayName("PolygonOffsetFrontEnabled")]
            [Category("Polygon Control")]
            public bool PolygonOffsetFrontEnabled
            {
                get { return _polygonControl.PolygonOffsetFrontEnabled; }
                set { _polygonControl.PolygonOffsetFrontEnabled = value; }
            }

            [Browsable(true)]
            [Description("Whether back-facing polygons are drawn offset (useful for decals to combat Z fighting")]
            [DisplayName("PolygonOffsetBackEnabled")]
            [Category("Polygon Control")]
            public bool PolygonOffsetBackEnabled
            {
                get { return _polygonControl.PolygonOffsetBackEnabled; }
                set { _polygonControl.PolygonOffsetBackEnabled = value; }
            }

            [Browsable(true)]
            [Description("Whether lines are drawn offset (useful for decals to combat Z fighting")]
            [DisplayName("PolygonOffsetBackEnabled")]
            [Category("Polygon Control")]
            public bool PolygonLineOffsetEnabled
            {
                get { return _polygonControl.PolygonLineOffsetEnabled; }
                set { _polygonControl.PolygonLineOffsetEnabled = value; }
            }

            #endregion

            #region ColorControl Properties

            private ColorControl _colorControl;

            [Browsable(true)]
            [Description("indicates whether multi writes are enabled.")]
            [DisplayName("MultiWriteEnabled")]
            [Category("Color Control")]
            public bool MultiWriteEnabled
            {
                get { return _colorControl.MultiWriteEnabled; }
                set { _colorControl.MultiWriteEnabled = value; }
            }

            [Browsable(true)]
            [Description("indicates whether the color buffer is enabled.")]
            [DisplayName("ColorBufferEnabled")]
            [Category("Color Control")]
            public bool ColorBufferEnabled
            {
                get { return _colorControl.ColorBufferEnabled; }
                set { _colorControl.ColorBufferEnabled = value; }
            }

            [Browsable(true)]
            [Description("The bitmask used for blending.")]
            [DisplayName("BlendEnableMask")]
            [Category("Color Control")]
            public byte BlendEnableMask
            {
                get { return _colorControl.BlendEnableMask; }
                set { _colorControl.BlendEnableMask = value; }
            }

            [Browsable(true)]
            [Description("The ROP3 logic operation.")]
            [DisplayName("LogicOp")]
            [Category("Color Control")]
            public GX2LogicOp LogicOp
            {
                get { return _colorControl.LogicOp; }
                set { _colorControl.LogicOp = value; }
            }

            #endregion

            #region DepthControl Properties

            private DepthControl _depthControl;

            [Browsable(true)]
            [Description("Indicates whether writing to the depth buffer is enabled..")]
            [DisplayName("DepthTestEnabled")]
            [Category("Depth Control")]
            public bool DepthTestEnabled
            {
                get { return _depthControl.DepthTestEnabled; }
                set { _depthControl.DepthTestEnabled = value; }
            }

            [Browsable(true)]
            [Description("Depth buffer comparison function, controlling whether a new fragment is allowed to overwrite the old value in the depth buffer.")]
            [DisplayName("DepthWriteEnabled")]
            [Category("Depth Control")]
            public bool DepthWriteEnabled
            {
                get { return _depthControl.DepthWriteEnabled; }
                set { _depthControl.DepthWriteEnabled = value; }
            }

            [Browsable(true)]
            [Description("Depth buffer comparison function, controlling whether a new fragment is allowed to overwrite the old value in the depth buffer.")]
            [DisplayName("DepthFunc")]
            [Category("Depth Control")]
            public GX2CompareFunction DepthFunc
            {
                get { return _depthControl.DepthFunc; }
                set { _depthControl.DepthFunc = value; }
            }

            [Browsable(true)]
            [Description("Indicates whether stencil testing is enabled.")]
            [DisplayName("StencilTestEnabled")]
            [Category("Depth Control")]
            public bool StencilTestEnabled
            {
                get { return _depthControl.StencilTestEnabled; }
                set { _depthControl.StencilTestEnabled = value; }
            }

            [Browsable(true)]
            [Description("Indicates whether back-facing polygons are allowed to write to the stencil buffer or not")]
            [DisplayName("BackFaceStencilEnabled")]
            [Category("Depth Control")]
            public bool BackStencilEnabled
            {
                get { return _depthControl.BackStencilEnabled; }
                set { _depthControl.BackStencilEnabled = value; }
            }


            [Browsable(true)]
            [Description("The front-facing polygon stencil comparison function.")]
            [DisplayName("FrontFaceStencilFunc")]
            [Category("Depth Control")]
            public GX2CompareFunction FrontStencilFunc
            {
                get { return _depthControl.FrontStencilFunc; }
                set { _depthControl.FrontStencilFunc = value; }
            }

            [Browsable(true)]
            [Description("The stencil function configuring what to do with the existing stencil value when the stencil")]
            [DisplayName("FrontFaceStencilFail")]
            [Category("Depth Control")]
            public GX2StencilFunction FrontStencilFail
            {
                get { return _depthControl.FrontStencilFail; }
                set { _depthControl.FrontStencilFail = value; }
            }

            [Browsable(true)]
            [Description("The stencil function taking effect when the stencil test passes with the depth buffer for front-facing polygons")]
            [DisplayName("FrontFaceStencilZPass")]
            [Category("Depth Control")]
            public GX2StencilFunction FrontStencilZPass
            {
                get { return _depthControl.FrontStencilZPass; }
                set { _depthControl.FrontStencilZPass = value; }
            }

            [Browsable(true)]
            [Description("The function when the stencil test fails with the depth buffer for front-facing polygons")]
            [DisplayName("FrontFaceStencilZFail")]
            [Category("Depth Control")]
            public GX2StencilFunction FrontStencilZFail
            {
                get { return _depthControl.FrontStencilZFail; }
                set { _depthControl.FrontStencilZFail = value; }
            }

            [Browsable(true)]
            [Description("The back-facing polygon stencil comparison function")]
            [DisplayName("BackFaceStencilFunc")]
            [Category("Depth Control")]
            public GX2CompareFunction BackStencilFunc
            {
                get { return _depthControl.BackStencilFunc; }
                set { _depthControl.BackStencilFunc = value; }
            }

            [Browsable(true)]
            [Description("The stencil function configuring what to do with the existing stencil value when the stencil")]
            [DisplayName("BackFaceStencilZFail")]
            [Category("Depth Control")]
            public GX2StencilFunction BackStencilFail
            {
                get { return _depthControl.BackStencilFail; }
                set { _depthControl.BackStencilFail = value; }
            }

            [Browsable(true)]
            [Description("The stencil function when the stencil test passes with the depth buffer for back-facing polygons")]
            [DisplayName("BackFaceStencilZPass")]
            [Category("Depth Control")]
            public GX2StencilFunction BackStencilZPass
            {
                get { return _depthControl.BackStencilZPass; }
                set { _depthControl.BackStencilZPass = value; }
            }

            [Browsable(true)]
            [Description("The function when the stencil test fails with the depth buffer for back-facing")]
            [DisplayName("BackFaceStencilZFail")]
            [Category("Depth Control")]
            public GX2StencilFunction BackStencilZFail
            {
                get { return _depthControl.BackStencilZFail; }
                set { _depthControl.BackStencilZFail = value; }
            }

            #endregion
        }

        /// <summary>
        /// Gets or sets GX2 polygon drawing settings controlling if and how triangles are rendered.
        /// </summary>
        [Browsable(false)]
        public PolygonControl PolygonControl
        {
            get { return _polygonControl; }
            set { _polygonControl = value; }
        }

        /// <summary>
        /// Gets or sets GX2 settings controlling how depth and stencil buffer checks are performed and handled.
        /// </summary>
        [Browsable(false)]
        public DepthControl DepthControl
        {
            get { return _depthControl; }
            set { _depthControl = value; }
        }

        /// <summary>
        /// Gets or sets GX2 settings controlling additional alpha blending options.
        /// </summary>
        [Browsable(false)]
        public AlphaControl AlphaControl
        {
            get { return _alphaControl; }
            set { _alphaControl = value; }
        }

        /// <summary>
        /// Gets or sets the reference value used for alpha testing.
        /// </summary>
        [Category("Alpha Control")]
        [Description("The reference value used for alpha testing.")]
        [DisplayName("Alpha Ref")]
        public float AlphaRefValue { get; set; }

        /// <summary>
        /// Gets or sets GX2 settings controlling additional color blending options.
        /// </summary>
        [Browsable(false)]
        public ColorControl ColorControl
        {
            get { return _colorControl; }
            set { _colorControl = value; }
        }

        /// <summary>
        /// Gets or sets the blend target index.
        /// </summary>
        [Description("The blend target index.")]
        [DisplayName("Blend Target")]
        [Category("Blend Control")]
        public uint BlendTarget { get; set; }

        /// <summary>
        /// Gets or sets GX2 settings controlling color and alpha blending.
        /// </summary>
        [Browsable(false)]
        public BlendControl BlendControl
        {
            get { return _blendControl; }
            set { _blendControl = value; }
        }

        /// <summary>
        /// Gets or sets the blend color to perform blending with.
        /// </summary>
        [DisplayName("Blend Color")]
        [Category("Blend Control")]
        public Vector4F BlendColor { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            _flags = loader.ReadUInt32();
            PolygonControl = new PolygonControl() { Value = loader.ReadUInt32() };
            DepthControl = new DepthControl() { Value = loader.ReadUInt32() };
            AlphaControl = new AlphaControl() { Value = loader.ReadUInt32() };
            AlphaRefValue = loader.ReadSingle();
            ColorControl = new ColorControl() { Value = loader.ReadUInt32() };
            BlendTarget = loader.ReadUInt32();
            BlendControl = new BlendControl() { Value = loader.ReadUInt32() };
            BlendColor = loader.ReadVector4F();
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(_flags);
            saver.Write(PolygonControl.Value);
            saver.Write(DepthControl.Value);
            saver.Write(AlphaControl.Value);
            saver.Write(AlphaRefValue);
            saver.Write(ColorControl.Value);
            saver.Write(BlendTarget);
            saver.Write(BlendControl.Value);
            saver.Write(BlendColor);
        }
    }

    public enum RenderStateFlagsMode : uint
    {
        Custom,
        Opaque,
        AlphaMask,
        Translucent
    }

    public enum RenderStateFlagsBlendMode : uint
    {
        None,
        Color,
        Logical
    }
}