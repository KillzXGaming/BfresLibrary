using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BfresLibrary.Core;
using BfresLibrary.GX2;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System.ComponentModel;
using BfresLibrary.Swizzling;

namespace BfresLibrary.WiiU
{
    /// <summary>
    /// Represents an FTEX subfile in a <see cref="ResFile"/>, storing multi-dimensional texture data.
    /// </summary>
    [DebuggerDisplay(nameof(Texture) + " {" + nameof(Name) + "}")]
    public class Texture : TextureShared, IResData
    {
        public const uint SwizzleMask = 0xFF00FF;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class.
        /// </summary>
        public Texture()
        {
            CompSelR = GX2CompSel.ChannelR;
            CompSelG = GX2CompSel.ChannelG;
            CompSelB = GX2CompSel.ChannelB;
            CompSelA = GX2CompSel.ChannelA;

            Name = "";
            Path = "";
            Width = 0;
            Height = 0;
            Depth = 1;
            Swizzle = 0;
            Alignment = 0x1000;
            ArrayLength = 1;
            Pitch = 32;
            TileMode = GX2TileMode.Mode2dTiledThin1;
            AAMode = GX2AAMode.Mode1X;
            Dim = GX2SurfaceDim.Dim2D;
            Format = GX2SurfaceFormat.T_BC1_SRGB;

            Data = new byte[0];
            MipData = new byte[0];
            MipOffsets = new uint[13];
            Regs = new uint[5];

            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FTEX";

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the source channel to map to the R (red) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the R (red) channel.")]
        [Category("Channels")]
        [DisplayName("Red Channel")]
        public GX2CompSel CompSelR { get; set; }

        /// <summary>
        /// Gets or sets the source channel to map to the G (green) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the G (green) channel.")]
        [Category("Channels")]
        [DisplayName("Green Channel")]
        public GX2CompSel CompSelG { get; set; }

        /// <summary>
        /// Gets or sets the source channel to map to the B (blue) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the B (blue) channel.")]
        [Category("Channels")]
        [DisplayName("Blue Channel")]
        public GX2CompSel CompSelB { get; set; }

        /// <summary>
        /// Gets or sets the source channel to map to the A (alpha) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the A (alpha) channel.")]
        [Category("Channels")]
        [DisplayName("Alpha Channel")]
        public GX2CompSel CompSelA { get; set; }

        /// <summary>
        /// Gets or sets the swizzling value.
        /// </summary>
        [Browsable(false)]
        public uint Swizzle { get; set; }

        [DisplayName("Swizzle Pattern")]
        public uint SwizzlePattern
        {
            get { return (Swizzle >> 8) & 7; }
        }

        /// <summary>
        /// Gets or sets the swizzling alignment.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Alignment")]
        [DisplayName("Alignment")]
        public uint Alignment { get; set; }

        /// <summary>
        /// Gets or sets the pixel swizzling stride.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("The pixel swizzling stride")]
        [DisplayName("Pitch")]
        public uint Pitch { get; set; }

        /// <summary>
        /// Gets or sets the desired texture data buffer format.
        /// </summary>
        [Browsable(true)]
        [Description("Enables SRGB if the format supports it")]
        [Category("Image Info")]
        [DisplayName("Use SRGB")]
        public bool UseSRGB
        {
            get
            {
                switch (Format)
                {
                    case GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB:
                    case GX2SurfaceFormat.T_BC1_SRGB:
                    case GX2SurfaceFormat.T_BC2_SRGB:
                    case GX2SurfaceFormat.T_BC3_SRGB:
                        return true;
                }
                return false;
            }
            set
            {
                if (value == true)
                {
                    switch (Format)
                    {
                        case GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNorm:
                            Format = GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB; break;
                        case GX2SurfaceFormat.T_BC1_UNorm:
                            Format = GX2SurfaceFormat.T_BC1_SRGB; break;
                        case GX2SurfaceFormat.T_BC2_UNorm:
                            Format = GX2SurfaceFormat.T_BC2_SRGB; break;
                        case GX2SurfaceFormat.T_BC3_UNorm:
                            Format = GX2SurfaceFormat.T_BC3_SRGB; break;
                    }
                }
                else
                {
                    switch (Format)
                    {
                        case GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB:
                            Format = GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNorm; break;
                        case GX2SurfaceFormat.T_BC1_SRGB:
                            Format = GX2SurfaceFormat.T_BC1_UNorm; break;
                        case GX2SurfaceFormat.T_BC2_SRGB:
                            Format = GX2SurfaceFormat.T_BC2_UNorm; break;
                        case GX2SurfaceFormat.T_BC3_SRGB:
                            Format = GX2SurfaceFormat.T_BC3_UNorm; break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired texture data buffer format.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Format")]
        [Category("Image Info")]
        [DisplayName("Format")]
        public GX2SurfaceFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the shape of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Dims of the texture")]
        [DisplayName("Dims")]
        public GX2SurfaceDim Dim { get; set; }

        /// <summary>
        /// Gets or sets the number of samples for the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Anti Alias Mode")]
        [DisplayName("Anti Alias Mode")]
        public GX2AAMode AAMode { get; set; }

        /// <summary>
        /// Gets or sets the texture data usage hint.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("The way the surface is used")]
        [DisplayName("Use")]
        public GX2SurfaceUse Use { get; set; }

        /// <summary>
        /// Gets or sets the tiling mode.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Tiling mode")]
        [DisplayName("Tile Mode")]
        public GX2TileMode TileMode { get; set; }

        /// <summary>
        /// Gets or sets the offsets in the <see cref="MipData"/> array to the data of the mipmap level corresponding
        /// to the array index.
        /// </summary>
        [Browsable(false)]
        public uint[] MipOffsets { get; set; }

        [Browsable(false)]
        public uint ViewMipFirst { get; set; }

        [Browsable(false)]
        public uint ViewMipCount { get; set; }

        [Browsable(false)]
        public uint ViewSliceFirst { get; set; }

        [Browsable(false)]
        public uint ViewSliceCount { get; set; }

        [Browsable(false)]
        public uint[] Regs { get; set; }

        /// <summary>
        /// Gets or sets the raw texture data bytes.
        /// </summary>
        [Browsable(false)]
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the raw mipmap level data bytes for all levels.
        /// </summary>
        [Browsable(false)]
        public byte[] MipData { get; set; }

        public uint MipSwizzle { get; set; }

        public override byte[] GetSwizzledData() {
            return ByteUtils.CombineArray(Data, MipData);
        }

        public override byte[] GetDeswizzledData(int arrayLevel, int mipLevel)
        {
            Swizzling.GX2.GX2Surface surf = new Swizzling.GX2.GX2Surface();
            surf.height = Height;
            surf.width = Width;
            surf.depth = ArrayLength;
            surf.alignment = Alignment;
            surf.aa = (uint)AAMode;
            surf.dim = (uint)Dim;
            surf.format = (uint)Format;
            surf.use = (uint)Use;
            surf.pitch = Pitch;
            surf.data = Data;
            surf.mipData = MipData != null ? MipData : Data;
            surf.mipOffset = MipOffsets != null ? MipOffsets : new uint[0];
            surf.numMips = MipCount;
            surf.numArray = ArrayLength;
            surf.tileMode = (uint)TileMode;
            surf.swizzle = Swizzle;

            return Swizzling.GX2.Decode(surf, arrayLevel, mipLevel);
        }

        /// <summary>
        /// Converts a Wii U texture instance to a switch texture.
        /// </summary>
        /// <param name="texture"></param>
        public void FromSwitch(Switch.SwitchTexture textureNX)
        {
            Width = textureNX.Width;
            Height = textureNX.Height;
            MipCount = textureNX.MipCount;
            Depth = 1;
            ArrayLength = textureNX.ArrayLength;
            TileMode = GX2TileMode.Mode2dTiledThin1;
            Dim = GX2SurfaceDim.Dim2D;
            Use = GX2SurfaceUse.Texture;
            Format = PlatformConverters.TextureConverter.FormatList.FirstOrDefault(
                x => x.Value == textureNX.Format).Key;
            Name = textureNX.Name;

            //Save arrays and mips into a list for swizzling back
            for (int i = 0; i < textureNX.ArrayLength; i++)
            {
                List<byte[]> mipData = new List<byte[]>();
                for (int j = 0; j < textureNX.MipCount; j++)
                    mipData.Add(textureNX.GetDeswizzledData(i, j));

                //Swizzle the current mip data into a switch swizzled image
                var surface = SwizzleSurfaceMipMaps(ByteUtils.CombineArray(mipData.ToArray()));
                Data = surface.data;
                MipData = surface.mipData;
                TileMode = (GX2TileMode)surface.tileMode;
                MipOffsets = surface.mipOffset;
                MipCount = surface.numMips;
                Alignment = surface.alignment;
                Pitch = surface.pitch;
                Swizzle = surface.swizzle;
                Regs = surface.texRegs;
            }

            CompSelR = ConvertChannelSelector(textureNX.Texture.ChannelRed);
            CompSelG = ConvertChannelSelector(textureNX.Texture.ChannelGreen);
            CompSelB = ConvertChannelSelector(textureNX.Texture.ChannelBlue);
            CompSelA = ConvertChannelSelector(textureNX.Texture.ChannelAlpha);

       /*     //Convert user data. BNTX doesn't share the same user data library atm so it needs manual conversion.
            UserData = new ResDict<UserData>();
            foreach (var userData in textureNX.UserData)
            {

            }*/
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            Dim = loader.ReadEnum<GX2SurfaceDim>(true);
            Width = loader.ReadUInt32();
            Height = loader.ReadUInt32();
            Depth = loader.ReadUInt32();
            MipCount = loader.ReadUInt32();
            Format = loader.ReadEnum<GX2SurfaceFormat>(true);
            AAMode = loader.ReadEnum<GX2AAMode>(true);
            Use = loader.ReadEnum<GX2SurfaceUse>(true);
            uint sizData = loader.ReadUInt32();
            uint imagePointer = loader.ReadUInt32();
            uint sizMipData = loader.ReadUInt32();
            uint mipPointer = loader.ReadUInt32();
            TileMode = loader.ReadEnum<GX2TileMode>(true);
            Swizzle = loader.ReadUInt32();
            Alignment = loader.ReadUInt32();
            Pitch = loader.ReadUInt32();
            MipOffsets = loader.ReadUInt32s(13);
            ViewMipFirst = loader.ReadUInt32();
            ViewMipCount = loader.ReadUInt32();
            ViewSliceFirst = loader.ReadUInt32();
            ViewSliceCount = loader.ReadUInt32();
            CompSelR = loader.ReadEnum<GX2CompSel>(true);
            CompSelG = loader.ReadEnum<GX2CompSel>(true);
            CompSelB = loader.ReadEnum<GX2CompSel>(true);
            CompSelA = loader.ReadEnum<GX2CompSel>(true);
            Regs = loader.ReadUInt32s(5);
            uint handle = loader.ReadUInt32();
            ArrayLength = loader.ReadByte(); // Possibly just a byte.
            loader.Seek(3, System.IO.SeekOrigin.Current);
            Name = loader.LoadString();
            Path = loader.LoadString();

            // Load texture data.
            bool? isMainTextureFile
                = loader.ResFile.Name.Contains(".Tex1") ? new bool?(true)
                : loader.ResFile.Name.Contains(".Tex2") ? new bool?(false)
                : null;

            switch (isMainTextureFile)
            {
                case true:
                    Data = loader.LoadCustom(() => loader.ReadBytes((int)sizData));
                    loader.ReadOffset(); // MipData not used.
                    break;
                case false:
                    MipData = loader.LoadCustom(() => loader.ReadBytes((int)sizMipData));
                    loader.ReadOffset(); // Data not used.
                    break;
                default:
                    Data = loader.LoadCustom(() => loader.ReadBytes((int)sizData));
                    MipData = loader.LoadCustom(() => loader.ReadBytes((int)sizMipData));
                    break;
            }

            UserData = loader.LoadDict<UserData>();
            ushort numUserData = loader.ReadUInt16();
            loader.Seek(2);
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            saver.Write(Dim, true);
            saver.Write(Width);
            saver.Write(Height);
            saver.Write(Depth);
            saver.Write(MipCount);
            saver.Write(Format, true);
            saver.Write(AAMode, true);
            saver.Write(Use, true);

            bool? isMainTextureFile
         = saver.ResFile.Name.Contains(".Tex1") ? new bool?(true)
         : saver.ResFile.Name.Contains(".Tex2") ? new bool?(false)
         : null;

            switch (isMainTextureFile)
            {
                case false:
                    saver.Write(0);
                    saver.Write(0); // ImagePointer
                    saver.Write(MipData == null ? 0 : MipData.Length);
                    saver.Write(0); // MipPointer
                    break;
                default:
                    saver.Write(Data == null ? 0 : Data.Length);
                    saver.Write(0); // ImagePointer
                    saver.Write(MipData == null ? 0 : MipData.Length);
                    saver.Write(0); // MipPointer
                    break;
            }


            saver.Write(TileMode, true);
            saver.Write(Swizzle);
            saver.Write(Alignment);
            saver.Write(Pitch);
            for (int i = 0; i < 13; i++)
                saver.Write(i < MipOffsets.Length ? MipOffsets[i] : 0);
            saver.Write(ViewMipFirst);
            saver.Write(ViewMipCount);
            saver.Write(ViewSliceFirst);
            saver.Write(ViewSliceCount);
            saver.Write(CompSelR, true);
            saver.Write(CompSelG, true);
            saver.Write(CompSelB, true);
            saver.Write(CompSelA, true);
            if (Regs.Length != 5)
                RegenerateRegisters();

            saver.Write(Regs);
            saver.Write(0); // Handle
            saver.Write((byte)ArrayLength);
            saver.Seek(3);
            saver.SaveString(Name);
            saver.SaveString(Path);

            uint alignment = saver.ResFile.Alignment;

            switch (isMainTextureFile)
            {
                case true:
                    saver.SaveBlock(Data, alignment, () => saver.Write(Data));
                    saver.Write(0); // MipData not used.
                    break;
                case false:
                    saver.SaveBlock(MipData, alignment, () => saver.Write(MipData));
                    saver.Write(0);  // Data not used.
                    break;
                default:
                    saver.SaveBlock(Data, alignment, () => saver.Write(Data));
                    saver.SaveBlock(MipData, alignment, () => saver.Write(MipData));
                    break;
            }
            saver.SaveDict(UserData);
            saver.Write((ushort)UserData.Count);
            saver.Seek(2);
        }

        private void RegenerateRegisters()
        {
            Regs = GX2TexRegisters.CreateTexRegs(Width, Height, MipCount,
                    (uint)Format, (uint)TileMode, Pitch, new byte[4]
                    {
                        (byte)CompSelR,
                        (byte)CompSelG,
                        (byte)CompSelB,
                        (byte)CompSelA,
                    });
        }

        private GX2.GX2CompSel ConvertChannelSelector(ChannelType type)
        {
            switch (type)
            {
                case ChannelType.Red: return GX2.GX2CompSel.ChannelR;
                case ChannelType.Green: return GX2.GX2CompSel.ChannelG;
                case ChannelType.Blue: return GX2.GX2CompSel.ChannelB;
                case ChannelType.Alpha: return GX2.GX2CompSel.ChannelA;
                case ChannelType.Zero: return GX2.GX2CompSel.Always0;
                default: return GX2.GX2CompSel.Always1;
            }
        }

        Swizzling.GX2.GX2Surface SwizzleSurfaceMipMaps(byte[] data)
        {
            return Swizzling.GX2.CreateGx2Texture(data,
               Name,
               (uint)TileMode,
               (uint)AAMode,
               Width,
               Height,
               Depth,
               (uint)Format,
               SwizzlePattern,
               (uint)Dim,
               MipCount);
        }
    }
}