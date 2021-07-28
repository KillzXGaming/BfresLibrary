using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.NSW.Bntx;
using BfresLibrary.Core;
using BfresLibrary.Swizzling;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace BfresLibrary.Switch
{
    public class SwitchTexture : TextureShared, IResData
    {
        /// <summary>
        /// The attached BNTX texture instance used.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// The parent BNTX file instance
        /// </summary>
        public BntxFile BntxFile { get; set; }

        public SurfaceFormat Format
        {
            get { return Texture.Format; }
            set { Texture.Format = value; }
        }

        public override string Name
        {
            get => Texture.Name;
            set => Texture.Name = value;
        }

        public override string Path
        {
            get => Texture.Path;
            set => Texture.Path = value;
        }

        public override uint Width 
        { 
            get => Texture.Width; 
            set => Texture.Width = value;
        }

        public override uint Height
        {
            get => Texture.Height;
            set => Texture.Height = value;
        }

        public override uint Depth
        {
            get => Texture.Depth;
            set => Texture.Depth = value;
        }

        public override uint ArrayLength
        {
            get => Texture.ArrayLength;
            set => Texture.ArrayLength = value;
        }

        public override uint MipCount
        {
            get => Texture.MipCount;
            set => Texture.MipCount = value;
        }

        public SwitchTexture(BntxFile bntx, Texture texture) {
            BntxFile = bntx;
            Texture = texture;
        }

        public override void Import(string FileName, ResFile ResFile)
        {
            Texture.Import(FileName);
        }

        public override void Export(string FileName, ResFile ResFile)
        {
            Texture.Export(FileName, BntxFile);
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Converts a Wii U texture instance to a switch texture.
        /// </summary>
        /// <param name="texture"></param>
        public void FromWiiU(WiiU.Texture textureU)
        {
            Texture = new Texture();
            Texture.Width = textureU.Width;
            Texture.Height = textureU.Height;
            Texture.MipCount = textureU.MipCount;
            Texture.ArrayLength = textureU.ArrayLength;
            Texture.TileMode = TileMode.Default;
            Texture.Depth = 1;
            Texture.SurfaceDim = SurfaceDim.Dim2D;
            Texture.Dim = Dim.Dim2D;
            Texture.Format = PlatformConverters.TextureConverter.FormatList[textureU.Format];
            Texture.Name = textureU.Name;
            Texture.AccessFlags = AccessFlags.Texture;
            Texture.Swizzle = textureU.SwizzlePattern;

            //Save arrays and mips into a list for swizzling back
            Texture.TextureData = new List<List<byte[]>>();
            for (int i = 0; i < textureU.ArrayLength; i++)
            {
                List<byte[]> mipData = new List<byte[]>();
                for (int j = 0; j < textureU.MipCount; j++)
                    mipData.Add(textureU.GetDeswizzledData(i, j));

                //Swizzle the current mip data into a switch swizzled image
                List<byte[]> mipmaps = SwizzleSurfaceMipMaps(ByteUtils.CombineArray(mipData.ToArray()));
                Texture.TextureData.Add(mipmaps);

                //Combine mip map data
                byte[] combinedMips = ByteUtils.CombineArray(mipmaps.ToArray());
                Texture.TextureData[i][0] = combinedMips;
            }

            Texture.ChannelRed = ConvertChannelSelector(textureU.CompSelR);
            Texture.ChannelGreen = ConvertChannelSelector(textureU.CompSelG);
            Texture.ChannelBlue = ConvertChannelSelector(textureU.CompSelB);
            Texture.ChannelAlpha = ConvertChannelSelector(textureU.CompSelA);

            //Convert user data. BNTX doesn't share the same user data library atm so it needs manual conversion.
            Texture.UserData = new List<Syroot.NintenTools.NSW.Bntx.UserData>();
            foreach (var userData in textureU.UserData)
            {

            }
        }

        /// <summary>
        /// returns the raw swizzled data in bytes.
        /// </summary>
        public override byte[] GetSwizzledData() {
            //Combine all the arrays from lists
            List<byte[]> arrayList = new List<byte[]>();
            foreach (var arrayData in Texture.TextureData)
                arrayList.Add(ByteUtils.CombineArray(arrayData.ToArray()));
            return ByteUtils.CombineArray(arrayList.ToArray());
        }

        /// <summary>
        /// returns the deswizzled data in bytes. Data is still compressed/encoded by format if not rgba8.
        /// </summary>
        public override byte[] GetDeswizzledData(int arrayLevel, int mipLevel)
        {
            if (Texture.TextureData.Count < arrayLevel)
                throw new Exception($"Invalid array level! {arrayLevel}");
            if (Texture.TextureData[arrayLevel].Count < mipLevel)
                throw new Exception($"Invalid mip level! {mipLevel}");

            return TegraX1Swizzle.GetImageData(Texture,
              ByteUtils.CombineArray(Texture.TextureData[arrayLevel].ToArray()), arrayLevel, mipLevel, 0,
               Texture.BlockHeightLog2, 1, Texture.TileMode == TileMode.LinearAligned );
        }

        /// <summary>
        /// Swizzles the given array level and deswizzled image data.
        /// Method adjusts the image sizes, mip offsets, alignment, and block parameters.
        /// </summary>
        /// <param name="deswizzledImageData"></param>
        /// <param name="arrayLevel"></param>
        public void SwizzleImage(byte[] deswizzledImageData, int arrayLevel = 0) {
            List<byte[]> mipmaps = SwizzleSurfaceMipMaps(deswizzledImageData);

            byte[] combinedMips = ByteUtils.CombineArray(mipmaps.ToArray());
            Texture.TextureData[arrayLevel][0] = combinedMips;
        }

        void IResData.Load(ResFileLoader loader)
        {
        }

        void IResData.Save(ResFileSaver saver)
        {

        }

        private ChannelType ConvertChannelSelector(GX2.GX2CompSel type)
        {
            switch (type)
            {
                case GX2.GX2CompSel.ChannelR: return ChannelType.Red;
                case GX2.GX2CompSel.ChannelG: return ChannelType.Green;
                case GX2.GX2CompSel.ChannelB: return ChannelType.Blue;
                case GX2.GX2CompSel.ChannelA: return ChannelType.Alpha;
                case GX2.GX2CompSel.Always0: return ChannelType.Zero;
                default: return ChannelType.One;
            }
        }

        List<byte[]> SwizzleSurfaceMipMaps(byte[] data)
        {
            int blockHeightShift = 0;
            int target = 1;
            uint Pitch = 0;
            uint SurfaceSize = 0;
            uint blockHeight = 0;
            uint blkWidth = TegraX1Swizzle.GetBlockWidth(Texture);
            uint blkHeight = TegraX1Swizzle.GetBlockHeight(Texture);
            uint blkDepth = 1;
            uint bpp = TegraX1Swizzle.GetBytesPerPixel(Texture);
            Texture.MipOffsets = new long[MipCount];
            Texture.textureLayout2 = 0x010007;
            Texture.SampleCount = 1;

            uint linesPerBlockHeight = 0;

            if (Texture.TileMode == TileMode.LinearAligned)
            {
                blockHeight = 1;
                Texture.BlockHeightLog2 = 0;
                Texture.Alignment = 1;

                linesPerBlockHeight = 1;
                Texture.ReadTextureLayout = 0;
            }
            else
            {
                blockHeight = TegraX1Swizzle.GetBlockHeight(TegraX1Swizzle.DIV_ROUND_UP(Texture.Height, blkHeight));
                Texture.BlockHeightLog2 = (uint)Convert.ToString(blockHeight, 2).Length - 1;
                Texture.Alignment = 512;
                Texture.ReadTextureLayout = 1;

                linesPerBlockHeight = blockHeight * 8;
            }

            List<byte[]> mipmaps = new List<byte[]>();
            for (int mipLevel = 0; mipLevel < Texture.MipCount; mipLevel++)
            {
                var result = Swizzling.TextureHelper.GetCurrentMipSize(Texture.Width, Texture.Height, blkWidth, blkHeight, bpp, mipLevel);
                uint offset = result.Item1;
                uint size = result.Item2;

                byte[] data_ = ByteUtils.SubArray(data, offset, size);

                uint width_ = Math.Max(1, Texture.Width >> mipLevel);
                uint height_ = Math.Max(1, Texture.Height >> mipLevel);
                uint depth_ = Math.Max(1, Texture.Depth >> mipLevel);

                uint width__ = TegraX1Swizzle.DIV_ROUND_UP(width_, blkWidth);
                uint height__ = TegraX1Swizzle.DIV_ROUND_UP(height_, blkHeight);
                uint depth__ = TegraX1Swizzle.DIV_ROUND_UP(depth_, blkDepth);

                byte[] AlignedData = new byte[(TegraX1Swizzle.round_up(SurfaceSize, (uint)Texture.Alignment) - SurfaceSize)];
                SurfaceSize += (uint)AlignedData.Length;

                Texture.MipOffsets[mipLevel] = SurfaceSize;
                if (Texture.TileMode == TileMode.LinearAligned)
                {
                    Pitch = width__ * bpp;

                    if (target == 1)
                        Pitch = TegraX1Swizzle.round_up(width__ * bpp, 32);

                    SurfaceSize += Pitch * height__;
                }
                else
                {
                    if (TegraX1Swizzle.pow2_round_up(height__) < linesPerBlockHeight)
                        blockHeightShift += 1;

                    Pitch = TegraX1Swizzle.round_up(width__ * bpp, 64);
                    SurfaceSize += Pitch * TegraX1Swizzle.round_up(height__, Math.Max(1, blockHeight >> blockHeightShift) * 8);
                }

                byte[] SwizzledData = TegraX1Swizzle.swizzle(width_, height_, depth_, blkWidth, blkHeight,
                    blkDepth, target, bpp, (uint)Texture.TileMode, (int)Math.Max(0, Texture.BlockHeightLog2 - blockHeightShift), data_);
                mipmaps.Add(AlignedData.Concat(SwizzledData).ToArray());
            }
            Texture.ImageSize = SurfaceSize;
            return mipmaps;
        }
    }
}
