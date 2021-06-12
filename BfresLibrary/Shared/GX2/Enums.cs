#pragma warning disable 1591 // Document enum members only when necessary.

using System;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents the AA modes (number of samples) for a surface.
    /// </summary>
    public enum GX2AAMode : uint
    {
        Mode1X,
        Mode2X,
        Mode4X,
        Mode8X
    }

    /// <summary>
    /// Represents the format of a vertex attribute entry. Possible type conversions:
    /// <para/>UNorm: attrib unsigned integer is converted to/from [0.0, 1.0] in shader.
    /// <para/>UInt: attrib unsigned integer is copied to/from shader as unsigned int.
    /// <para/>SNorm: attrib signed integer is converted to/from [-1.0, 1.0] in shader.
    /// <para/>SInt: attrib signed integer is copied to/from shader as signed int.
    /// <para/>Single: attrib single is copied to/from shader as Single.
    /// <para/>UIntToSingle: attrib unsigned integer is converted Single in shader.
    /// <para/>SIntToSingle: attrib signed integer is converted Single in shader.
    /// </summary>
    public enum GX2AttribFormat : uint
    {
        // 8 bits (8 x 1)
        Format_8_UNorm = 0x00000000,
        Format_8_UInt = 0x00000100,
        Format_8_SNorm = 0x00000200,
        Format_8_SInt = 0x00000300,
        Format_8_UIntToSingle = 0x00000800,
        Format_8_SIntToSingle = 0x00000A00,
        // 8 bits (4 x 2)
        Format_4_4_UNorm = 0x00000001,
        // 16 bits (16 x 1)
        Format_16_UNorm = 0x00000002,
        Format_16_UInt = 0x00000102,
        Format_16_SNorm = 0x00000202,
        Format_16_SInt = 0x00000302,
        Format_16_Single = 0x00000803,
        Format_16_UIntToSingle = 0x00000802,
        Format_16_SIntToSingle = 0x00000A02,
        // 16 bits (8 x 2)
        Format_8_8_UNorm = 0x00000004,
        Format_8_8_UInt = 0x00000104,
        Format_8_8_SNorm = 0x00000204,
        Format_8_8_SInt = 0x00000304,
        Format_8_8_UIntToSingle = 0x00000804,
        Format_8_8_SIntToSingle = 0x00000A04,
        // 32 bits (32 x 1)
        Format_32_UInt = 0x00000105,
        Format_32_SInt = 0x00000305,
        Format_32_Single = 0x00000806,
        // 32 bits (16 x 2)
        Format_16_16_UNorm = 0x00000007,
        Format_16_16_UInt = 0x00000107,
        Format_16_16_SNorm = 0x00000207,
        Format_16_16_SInt = 0x00000307,
        Format_16_16_Single = 0x00000808,
        Format_16_16_UIntToSingle = 0x00000807,
        Format_16_16_SIntToSingle = 0x00000A07,
        // 32 bits (10/11 x 3)
        Format_10_11_11_Single = 0x00000809,
        // 32 bits (8 x 4)
        Format_8_8_8_8_UNorm = 0x0000000A,
        Format_8_8_8_8_UInt = 0x0000010A,
        Format_8_8_8_8_SNorm = 0x0000020A,
        Format_8_8_8_8_SInt = 0x0000030A,
        Format_8_8_8_8_UIntToSingle = 0x0000080A,
        Format_8_8_8_8_SIntToSingle = 0x00000A0A,
        // 32 bits (10 x 3 + 2)
        Format_10_10_10_2_UNorm = 0x0000000B,
        Format_10_10_10_2_UInt = 0x0000010B,
        Format_10_10_10_2_SNorm = 0x0000020B, // High 2 bits are UNorm
        Format_10_10_10_2_SInt = 0x0000030B,
        // 64 bits (32 x 2)
        Format_32_32_UInt = 0x0000010C,
        Format_32_32_SInt = 0x0000030C,
        Format_32_32_Single = 0x0000080D,
        // 64 bits (16 x 4)
        Format_16_16_16_16_UNorm = 0x0000000E,
        Format_16_16_16_16_UInt = 0x0000010E,
        Format_16_16_16_16_SNorm = 0x0000020E,
        Format_16_16_16_16_SInt = 0x0000030E,
        Format_16_16_16_16_Single = 0x0000080F,
        Format_16_16_16_16_UIntToSingle = 0x0000080E,
        Format_16_16_16_16_SIntToSingle = 0x00000A0E,
        // 96 bits (32 x 3)
        Format_32_32_32_UInt = 0x00000110,
        Format_32_32_32_SInt = 0x00000310,
        Format_32_32_32_Single = 0x00000811,
        // 128 bits (32 x 4)
        Format_32_32_32_32_UInt = 0x00000112,
        Format_32_32_32_32_SInt = 0x00000312,
        Format_32_32_32_32_Single = 0x00000813
    }

    /// <summary>
    /// Represents how the terms of the blend function are combined.
    /// </summary>
    public enum GX2BlendCombine : uint
    {
        Add,
        SourceMinusDestination,
        Minimum,
        Maximum,
        DestinationMinusSource
    }

    /// <summary>
    /// Represents the factors used in the blend function.
    /// </summary>
    public enum GX2BlendFunction : uint
    {
        Zero = 0,
        One = 1,
        SourceColor = 2,
        OneMinusSourceColor = 3,
        SourceAlpha = 4,
        OneMinusSourceAlpha = 5,
        DestinationAlpha = 6,
        OneMinusDestinationAlpha = 7,
        DestinationColor = 8,
        OneMinusDestinationColor = 9,
        SourceAlphaSaturate = 10,
        ConstantColor = 13,
        OneMinusConstantColor = 14,
        Source1Color = 15,
        OneMinusSource1Color = 16,
        Source1Alpha = 17,
        OneMinusSource1Alpha = 18,
        ConstantAlpha = 19,
        OneMinusConstantAlpha = 20,
    }

    /// <summary>
    /// Represents compare functions used for depth and stencil tests.
    /// </summary>
    public enum GX2CompareFunction : uint
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
    /// Represents the source channels to map to a color channel in textures.
    /// </summary>
    public enum GX2CompSel : byte
    {
        ChannelR,
        ChannelG,
        ChannelB,
        ChannelA,
        Always0,
        Always1
    }

    /// <summary>
    /// Represents the vertex order of front-facing polygons.
    /// </summary>
    public enum GX2FrontFaceMode : uint
    {
        CounterClockwise,
        Clockwise
    }

    /// <summary>
    /// Represents the type in which vertex indices are stored.
    /// </summary>
    public enum GX2IndexFormat : uint
    {
        UInt16LittleEndian = 0,
        UInt32LittleEndian = 1,
        UInt16 = 4,
        UInt32 = 9,
    }

    /// <summary>
    /// Represents the logic op function to perform.
    /// </summary>
    public enum GX2LogicOp : uint
    {
        /// <summary>
        /// Black
        /// </summary>
        Clear = 0x00,
        /// <summary>
        /// White
        /// </summary>
        Set = 0xFF,
        /// <summary>
        /// Source (Default)
        /// </summary>
        Copy = 0xCC,
        /// <summary>
        /// ~Source
        /// </summary>
        InverseCopy = 0x33,
        /// <summary>
        /// Destination
        /// </summary>
        NoOperation = 0xAA,
        /// <summary>
        /// ~Destination
        /// </summary>
        Inverse = 0x55,
        /// <summary>
        /// Source &amp; Destination
        /// </summary>
        And = 0x88,
        /// <summary>
        /// ~(Source &amp; Destination)
        /// </summary>
        NAnd = 0x77,
        /// <summary>
        /// Source | Destination
        /// </summary>
        Or = 0xEE,
        /// <summary>
        /// ~(Source | Destination)
        /// </summary>
        NOr = 0x11,
        /// <summary>
        /// Source ^ Destination
        /// </summary>
        XOr = 0x66,
        /// <summary>
        ///  ~(Source ^ Destination)
        /// </summary>
        Equivalent = 0x99,
        /// <summary>
        /// Source &amp; ~Destination
        /// </summary>
        ReverseAnd = 0x44,
        /// <summary>
        /// ~Source &amp; Destination
        /// </summary>
        InverseAnd = 0x22,
        /// <summary>
        /// Source | ~Destination
        /// </summary>
        ReverseOr = 0xDD,
        /// <summary>
        /// ~Source | Destination
        /// </summary>
        InverseOr = 0xBB,
    }

    /// <summary>
    /// Represents the base primitive used to draw each side of the polygon when dual-sided polygon mode is enabled.
    /// </summary>
    public enum GX2PolygonMode : uint
    {
        Point,
        Line,
        Triangle
    }

    /// <summary>
    /// Represents the type of primitives to draw.
    /// </summary>
    public enum GX2PrimitiveType : uint
    {
        /// <summary>
        /// Requires at least 1 element and 1 more to draw another primitive.
        /// </summary>
        Points = 0x01,

        /// <summary>
        /// Requires at least 2 elements and 2 more to draw another primitive.
        /// </summary>
        Lines = 0x02,

        /// <summary>
        /// Requires at least 2 elements and 1 more to draw another primitive.
        /// </summary>
        LineStrip = 0x03,

        /// <summary>
        /// Requires at least 3 elements and 3 more to draw another primitive.
        /// </summary>
        Triangles = 0x04,

        /// <summary>
        /// Requires at least 3 elements and 1 more to draw another primitive.
        /// </summary>
        TriangleFan = 0x05,

        /// <summary>
        /// Requires at least 3 elements and 1 more to draw another primitive.
        /// </summary>
        TriangleStrip = 0x06,

        /// <summary>
        /// Requires at least 4 elements and 4 more to draw another primitive.
        /// </summary>
        LinesAdjacency = 0x0A,

        /// <summary>
        /// Requires at least 4 elements and 1 more to draw another primitive.
        /// </summary>
        LineStripAdjacency = 0x0B,

        /// <summary>
        /// Requires at least 6 elements and 6 more to draw another primitive.
        /// </summary>
        TrianglesAdjacency = 0x0C,

        /// <summary>
        /// Requires at least 6 elements and 2 more to draw another primitive.
        /// </summary>
        TriangleStripAdjacency = 0x0D,

        /// <summary>
        /// Requires at least 3 elements and 3 more to draw another primitive.
        /// </summary>
        Rects = 0x11,

        /// <summary>
        /// Requires at least 2 elements and 1 more to draw another primitive.
        /// </summary>
        LineLoop = 0x12,

        /// <summary>
        /// Requires at least 4 elements and 4 more to draw another primitive.
        /// </summary>
        Quads = 0x13,

        /// <summary>
        /// Requires at least 4 elements and 2 more to draw another primitive.
        /// </summary>
        QuadStrip = 0x14,

        /// <summary>
        /// Requires at least 2 elements and 2 more to draw another primitive.
        /// </summary>
        TessellateLines = 0x82,

        /// <summary>
        /// Requires at least 2 elements and 1 more to draw another primitive.
        /// </summary>
        TessellateLineStrip = 0x83,

        /// <summary>
        /// Requires at least 3 elements and 3 more to draw another primitive.
        /// </summary>
        TessellateTriangles = 0x84,

        /// <summary>
        /// Requires at least 3 elements and 1 more to draw another primitive.
        /// </summary>
        TessellateTriangleStrip = 0x86,

        /// <summary>
        /// Requires at least 4 elements and 4 more to draw another primitive.
        /// </summary>
        TessellateQuads = 0x93,

        /// <summary>
        /// Requires at least 4 elements and 2 more to draw another primitive.
        /// </summary>
        TessellateQuadStrip = 0x94
    }

    /// <summary>
    /// Represents the stencil function to be performed if stencil tests pass.
    /// </summary>
    public enum GX2StencilFunction : uint
    {
        Keep,
        Zero,
        Replace,
        Increment,
        Decrement,
        Invert,
        IncrementWrap,
        DecrementWrap
    }

    /// <summary>
    /// Represents shapes of a given surface or texture.
    /// </summary>
    public enum GX2SurfaceDim : uint
    {
        Dim1D,
        Dim2D,
        Dim3D,
        DimCube,
        Dim1DArray,
        Dim2DArray,
        Dim2DMsaa,
        Dim2DMsaaArray
    }

    /// <summary>
    /// Represents desired texture, color-buffer, depth-buffer, or scan-buffer formats.
    /// </summary>
    public enum GX2SurfaceFormat : uint
    {
        Invalid = 0x00000000,
        TC_R8_UNorm = 0x00000001,
        TC_R8_UInt = 0x00000101,
        TC_R8_SNorm = 0x00000201,
        TC_R8_SInt = 0x00000301,
        T_R4_G4_UNorm = 0x00000002,
        TCD_R16_UNorm = 0x00000005,
        TC_R16_UInt = 0x00000105,
        TC_R16_SNorm = 0x00000205,
        TC_R16_SInt = 0x00000305,
        TC_R16_Float = 0x00000806,
        TC_R8_G8_UNorm = 0x00000007,
        TC_R8_G8_UInt = 0x00000107,
        TC_R8_G8_SNorm = 0x00000207,
        TC_R8_G8_SInt = 0x00000307,
        TCS_R5_G6_B5_UNorm = 0x00000008,
        TC_R5_G5_B5_A1_UNorm = 0x0000000A,
        TC_R4_G4_B4_A4_UNorm = 0x0000000B,
        TC_A1_B5_G5_R5_UNorm = 0x0000000C,
        TC_R32_UInt = 0x0000010D,
        TC_R32_SInt = 0x0000030D,
        TCD_R32_Float = 0x0000080E,
        TC_R16_G16_UNorm = 0x0000000F,
        TC_R16_G16_UInt = 0x0000010F,
        TC_R16_G16_SNorm = 0x0000020F,
        TC_R16_G16_SInt = 0x0000030F,
        TC_R16_G16_Float = 0x00000810,
        D_D24_S8_UNorm = 0x00000011,
        T_R24_UNorm_X8 = 0x00000011,
        T_X24_G8_UInt = 0x00000111,
        D_D24_S8_Float = 0x00000811,
        TC_R11_G11_B10_Float = 0x00000816,
        TCS_R10_G10_B10_A2_UNorm = 0x00000019,
        TC_R10_G10_B10_A2_UInt = 0x00000119,
        TC_R10_G10_B10_A2_SNorm = 0x00000219,
        TC_R10_G10_B10_A2_SInt = 0x00000319,
        TCS_R8_G8_B8_A8_UNorm = 0x0000001A,
        TC_R8_G8_B8_A8_UInt = 0x0000011A,
        TC_R8_G8_B8_A8_SNorm = 0x0000021A,
        TC_R8_G8_B8_A8_SInt = 0x0000031A,
        TCS_R8_G8_B8_A8_SRGB = 0x0000041A,
        TCS_A2_B10_G10_R10_UNorm = 0x0000001B,
        TC_A2_B10_G10_R10_UInt = 0x0000011B,
        D_D32_Float_S8_UInt_X24 = 0x0000081C,
        T_R32_Float_X8_X24 = 0x0000081C,
        T_X32_G8_UInt_X24 = 0x0000011C,
        TC_R32_G32_UInt = 0x0000011D,
        TC_R32_G32_SInt = 0x0000031D,
        TC_R32_G32_Float = 0x0000081E,
        TC_R16_G16_B16_A16_UNorm = 0x0000001F,
        TC_R16_G16_B16_A16_UInt = 0x0000011F,
        TC_R16_G16_B16_A16_SNorm = 0x0000021F,
        TC_R16_G16_B16_A16_SInt = 0x0000031F,
        TC_R16_G16_B16_A16_Float = 0x00000820,
        TC_R32_G32_B32_A32_UInt = 0x00000122,
        TC_R32_G32_B32_A32_SInt = 0x00000322,
        TC_R32_G32_B32_A32_Float = 0x00000823,
        T_BC1_UNorm = 0x00000031,
        T_BC1_SRGB = 0x00000431,
        T_BC2_UNorm = 0x00000032,
        T_BC2_SRGB = 0x00000432,
        T_BC3_UNorm = 0x00000033,
        T_BC3_SRGB = 0x00000433,
        T_BC4_UNorm = 0x00000034,
        T_BC4_SNorm = 0x00000234,
        T_BC5_UNorm = 0x00000035,
        T_BC5_SNorm = 0x00000235,
        T_NV12_UNorm = 0x00000081
    }

    /// <summary>
    /// Represents Indicates how a given surface may be used. A final TV render target is one that will be copied to a
    /// TV scan buffer. It needs to be designated to handle certain display corner cases (when a HD surface must be
    /// scaled down to display in NTSC/PAL).
    /// </summary>
    [Flags]
    public enum GX2SurfaceUse : uint
    {
        Texture = 1 << 0,
        ColorBuffer = 1 << 1,
        DepthBuffer = 1 << 2,
        ScanBuffer = 1 << 3,
        FinalTV = 1u << 31,
        ColorBufferTexture = Texture | ColorBuffer,
        DepthBufferTexture = Texture | DepthBuffer,
        ColorBufferFinalTV = FinalTV | ColorBuffer,
        ColorBufferTextureFinalTV = FinalTV | ColorBufferTexture
    }

    /// <summary>
    /// Represents maximum desired anisotropic filter ratios. Higher ratios give better image quality, but slower
    /// performance.
    /// </summary>
    public enum GX2TexAnisoRatio : uint
    {
        Ratio_1_1,
        Ratio_2_1,
        Ratio_4_1,
        Ratio_8_1,
        Ratio_16_1,
    }

    /// <summary>
    /// Represents type of border color to use.
    /// </summary>
    public enum GX2TexBorderType : uint
    {
        ClearBlack,
        SolidBlack,
        SolidWhite,
        UseRegister
    }

    /// <summary>
    /// Represents how to treat texture coordinates outside of the normalized coordinate texture range.
    /// </summary>
    public enum GX2TexClamp : uint
    {
        Wrap,
        Mirror,
        Clamp,
        MirrorOnce,
        ClampHalfBorder,
        MirrorOnceHalfBorder,
        ClampBorder,
        MirrorOnceBorder
    }

    /// <summary>
    /// Represents desired texture filter options between mip levels.
    /// </summary>
    public enum GX2TexMipFilterType : uint
    {
        NoMip,
        Point,
        Linear
    }

    /// <summary>
    /// Represents desired texture filter options within a plane.
    /// </summary>
    public enum GX2TexXYFilterType : uint
    {
        Point,
        Bilinear
    }

    /// <summary>
    /// Represents desired texture filter options between Z planes.
    /// </summary>
    public enum GX2TexZFilterType : uint
    {
        UseXY,
        Point,
        Linear
    }

    /// <summary>
    /// Represents the desired tiling modes for a surface.
    /// </summary>
    public enum GX2TileMode : uint
    {
        Default,
        LinearAligned,
        Mode1dTiledThin1,
        Mode1dTiledThick,
        Mode2dTiledThin1,
        Mode2dTiledThin2,
        Mode2dTiledThin4,
        Mode2dTiledThick,
        Mode2bTiledThin1,
        Mode2bTiledThin2,
        Mode2bTiledThin4,
        Mode2bTiledThick,
        Mode3dTiledThin1,
        Mode3dTiledThick,
        Mode3bTiledThin1,
        Mode3bTiledThick,
        LinearSpecial
    }
}

#pragma warning restore 1591