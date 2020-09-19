using System;
using System.Collections.Generic;
using Syroot.NintenTools.NSW.Bntx.GFX;
using Syroot.NintenTools.NSW.Bntx;

namespace BfresLibrary.Swizzling
{
    public class TegraX1Swizzle
    {
        public static byte[] GetImageData(Texture texture, byte[] ImageData, int ArrayLevel, int MipLevel, int DepthLevel, int target = 1, bool LinearTileMode = false)
        {
            uint blkHeight = GetBlockHeight(texture);
            uint blkDepth = 1;
            uint blockHeight = TegraX1Swizzle.GetBlockHeight(TegraX1Swizzle.DIV_ROUND_UP(texture.Height, blkHeight));
            uint BlockHeightLog2 = (uint)Convert.ToString(blockHeight, 2).Length - 1;
            return GetImageData(texture, ImageData, ArrayLevel, MipLevel, DepthLevel, BlockHeightLog2, target, LinearTileMode);
        }

        public static byte[] GetImageData(Texture texture, byte[] ImageData, int ArrayLevel, int MipLevel, int DepthLevel, uint BlockHeightLog2, int target = 1, bool LinearTileMode = false)
        {
            uint bpp = GetBytesPerPixel(texture);
            uint blkWidth = GetBlockWidth(texture);
            uint blkHeight = GetBlockHeight(texture);
            uint blkDepth = 1;
            uint blockHeight = TegraX1Swizzle.GetBlockHeight(TegraX1Swizzle.DIV_ROUND_UP(texture.Height, blkHeight));

            uint Pitch = 0;
            uint DataAlignment = 512;
            uint TileMode = 0;
            if (LinearTileMode)
                TileMode = 1;
            uint numDepth = 1;
            if (texture.Depth > 1)
                numDepth = texture.Depth;

            int linesPerBlockHeight = (1 << (int)BlockHeightLog2) * 8;

            uint ArrayOffset = 0;
            for (int depthLevel = 0; depthLevel < numDepth; depthLevel++)
            {
                for (int arrayLevel = 0; arrayLevel < texture.ArrayLength; arrayLevel++)
                {
                    uint SurfaceSize = 0;
                    int blockHeightShift = 0;

                    List<uint> MipOffsets = new List<uint>();

                    for (int mipLevel = 0; mipLevel < texture.MipCount; mipLevel++)
                    {
                        uint width = (uint)Math.Max(1, texture.Width >> mipLevel);
                        uint height = (uint)Math.Max(1, texture.Height >> mipLevel);
                        uint depth = (uint)Math.Max(1, texture.Depth >> mipLevel);

                        uint size = TegraX1Swizzle.DIV_ROUND_UP(width, blkWidth) * TegraX1Swizzle.DIV_ROUND_UP(height, blkHeight) * bpp;

                        if (TegraX1Swizzle.pow2_round_up(TegraX1Swizzle.DIV_ROUND_UP(height, blkWidth)) < linesPerBlockHeight)
                            blockHeightShift += 1;


                        uint width__ = TegraX1Swizzle.DIV_ROUND_UP(width, blkWidth);
                        uint height__ = TegraX1Swizzle.DIV_ROUND_UP(height, blkHeight);

                        //Calculate the mip size instead
                        byte[] AlignedData = new byte[(TegraX1Swizzle.round_up(SurfaceSize, DataAlignment) - SurfaceSize)];
                        SurfaceSize += (uint)AlignedData.Length;
                        MipOffsets.Add(SurfaceSize);

                        //Get the first mip offset and current one and the total image size
                        int msize = (int)((MipOffsets[0] + ImageData.Length - MipOffsets[mipLevel]) / texture.ArrayLength);

                        byte[] data_ = ByteUtils.SubArray(ImageData, ArrayOffset + MipOffsets[mipLevel], (uint)msize);

                        try
                        {
                            Pitch = TegraX1Swizzle.round_up(width__ * bpp, 64);
                            SurfaceSize += Pitch * TegraX1Swizzle.round_up(height__, Math.Max(1, blockHeight >> blockHeightShift) * 8);

                            byte[] result = TegraX1Swizzle.deswizzle(width, height, depth, blkWidth, blkHeight, blkDepth, target, bpp, TileMode, (int)Math.Max(0, BlockHeightLog2 - blockHeightShift), data_);
                            //Create a copy and use that to remove uneeded data
                            byte[] result_ = new byte[size];
                            Array.Copy(result, 0, result_, 0, size);
                            result = null;

                            if (ArrayLevel == arrayLevel && MipLevel == mipLevel && DepthLevel == depthLevel)
                                return result_;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to swizzle texture {texture.Name}!");
                            Console.WriteLine(e);

                            return new byte[0];
                        }
                    }
                    ArrayOffset += (uint)(ImageData.Length / texture.ArrayLength);
                }
            }
            return new byte[0];
        }

        internal static uint GetBytesPerPixel(Texture texture)
        {
            if (FormatList.ContainsKey(texture.Format))
                return FormatList[texture.Format].BytesPerPixel;
            return 0;
        }

        internal static uint GetBlockWidth(Texture texture)
        {
            if (FormatList.ContainsKey(texture.Format))
                return FormatList[texture.Format].BlockWidth;
            return 0;
        }

        internal static uint GetBlockHeight(Texture texture)
        {
            if (FormatList.ContainsKey(texture.Format))
                return FormatList[texture.Format].BlockHeight;
            return 0;
        }

        static Dictionary<SurfaceFormat, FormatInfo> FormatList = new Dictionary<SurfaceFormat, FormatInfo>()
        {
            { SurfaceFormat.R32_G32_B32_A32_UNORM,    new FormatInfo(16, 1,  1) },
            { SurfaceFormat.R16_G16_B16_A16_UNORM,    new FormatInfo(8, 1, 1) },
            { SurfaceFormat.R8_G8_B8_A8_UNORM,        new FormatInfo(4, 1, 1) },
            { SurfaceFormat.R8_G8_B8_A8_SRGB,         new FormatInfo(4, 1, 1) },
            { SurfaceFormat.R8_G8_B8_A8_SNORM,        new FormatInfo(4, 1, 1) },
            { SurfaceFormat.R4_G4_B4_A4_UNORM,        new FormatInfo(3, 1, 1) },
            { SurfaceFormat.R32_G32_B32_UNORM,        new FormatInfo(8, 1, 1) },
            { SurfaceFormat.R4_G4_UNORM,              new FormatInfo(1, 1, 1) },
            { SurfaceFormat.R32_UNORM,                new FormatInfo(4, 1, 1) },
            { SurfaceFormat.R16_UNORM,                new FormatInfo(2, 1, 1) },
            { SurfaceFormat.R16_UINT,                 new FormatInfo(2, 1, 1) },
            { SurfaceFormat.R8_UNORM,                 new FormatInfo(1, 1, 1) },
            { SurfaceFormat.R32_G8_X24_UNORM,         new FormatInfo(8, 1, 1) },
            { SurfaceFormat.B8_G8_R8_A8_UNORM,        new FormatInfo(4, 1, 1) },
            { SurfaceFormat.B8_G8_R8_A8_SRGB,         new FormatInfo(4, 1, 1) },
            { SurfaceFormat.R5_G5_B5_A1_UNORM,        new FormatInfo(2, 1, 1) },
            { SurfaceFormat.B5_G5_R5_A1_UNORM,        new FormatInfo(2, 1, 1) },
            { SurfaceFormat.R5_G6_B5_UNORM,           new FormatInfo(2, 1, 1) },
            { SurfaceFormat.R10_G10_B10_A2_UNORM,     new FormatInfo(4, 1, 1) },
            { SurfaceFormat.R11_G11_B10_UNORM,        new FormatInfo(4, 1, 1) },
            { SurfaceFormat.A4_B4_G4_R4_UNORM,        new FormatInfo(2, 1, 1) },
            { SurfaceFormat.B5_G6_R5_UNORM,           new FormatInfo(2, 1, 1) },

            { SurfaceFormat.BC1_UNORM,           new FormatInfo(8, 4, 4) },
            { SurfaceFormat.BC1_SRGB,            new FormatInfo(8, 4, 4) },
            { SurfaceFormat.BC2_UNORM,           new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC2_SRGB,            new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC3_UNORM,           new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC3_SRGB,            new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC4_UNORM,           new FormatInfo(8, 4, 4) },
            { SurfaceFormat.BC4_SNORM,           new FormatInfo(8, 4, 4) },
            { SurfaceFormat.BC5_UNORM,           new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC5_SNORM,           new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC6_UFLOAT,          new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC6_FLOAT,           new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC7_UNORM,           new FormatInfo(16, 4, 4) },
            { SurfaceFormat.BC7_SRGB,            new FormatInfo(16, 4, 4) },

            { SurfaceFormat.ASTC_4x4_UNORM,      new FormatInfo(16, 4, 4) },
            { SurfaceFormat.ASTC_4x4_SRGB,       new FormatInfo(16, 4, 4) },
            { SurfaceFormat.ASTC_5x5_UNORM,       new FormatInfo(16, 5, 5) },
            { SurfaceFormat.ASTC_5x5_SRGB,       new FormatInfo(16, 5, 5) },
            { SurfaceFormat.ASTC_6x5_UNORM,       new FormatInfo(16, 6, 5) },
            { SurfaceFormat.ASTC_6x5_SRGB,       new FormatInfo(16, 6, 5) },
            { SurfaceFormat.ASTC_8x5_UNORM,       new FormatInfo(16, 8, 5) },
            { SurfaceFormat.ASTC_8x5_SRGB,       new FormatInfo(16, 8, 5) },
            { SurfaceFormat.ASTC_8x6_UNORM,       new FormatInfo(16, 8, 6) },
            { SurfaceFormat.ASTC_8x6_SRGB,       new FormatInfo(16, 8, 6) },
            { SurfaceFormat.ASTC_10x5_UNORM,       new FormatInfo(16, 10, 5) },
            { SurfaceFormat.ASTC_10x5_SRGB,       new FormatInfo(16, 10, 5) },
            { SurfaceFormat.ASTC_10x6_UNORM,       new FormatInfo(16, 10, 6) },
            { SurfaceFormat.ASTC_10x6_SRGB,       new FormatInfo(16, 10, 6) },
            { SurfaceFormat.ASTC_10x10_SRGB,       new FormatInfo(16, 10, 10) },
            { SurfaceFormat.ASTC_10x10_UNORM,       new FormatInfo(16, 10, 10) },
            { SurfaceFormat.ASTC_12x10_SRGB,       new FormatInfo(16, 12, 10) },
            { SurfaceFormat.ASTC_12x10_UNORM,       new FormatInfo(16, 12, 10) },
            { SurfaceFormat.ASTC_12x12_SRGB,       new FormatInfo(16, 12, 12) },
            { SurfaceFormat.ASTC_12x12_UNORM,       new FormatInfo(16, 12, 12) },
        };

        class FormatInfo
        {
            public uint BytesPerPixel;
            public uint BlockWidth;
            public uint BlockHeight;

            public FormatInfo(uint bpp, uint blockW, uint blockH) {
                BytesPerPixel = bpp;
                BlockWidth = blockW;
                BlockHeight = blockH;
            }
        }

        /*---------------------------------------
         * 
         * Code ported from AboodXD's BNTX Extractor https://github.com/aboood40091/BNTX-Extractor/blob/master/swizzle.py
         * 
         *---------------------------------------*/

        public static uint GetBlockHeight(uint height)
        {
            uint blockHeight = pow2_round_up(height / 8);
            if (blockHeight > 16)
                blockHeight = 16;

            return blockHeight;
        }

        public static uint DIV_ROUND_UP(uint n, uint d)
        {
            return (n + d - 1) / d;
        }
        public static uint round_up(uint x, uint y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }
        public static uint pow2_round_up(uint x)
        {
            x -= 1;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        private static byte[] _swizzle(uint width, uint height, uint depth, uint blkWidth, uint blkHeight, uint blkDepth, int roundPitch, uint bpp, uint tileMode, int blockHeightLog2, byte[] data, int toSwizzle)
        {
            uint block_height = (uint)(1 << blockHeightLog2);

            width = DIV_ROUND_UP(width, blkWidth);
            height = DIV_ROUND_UP(height, blkHeight);
            depth = DIV_ROUND_UP(depth, blkDepth);

            uint pitch;
            uint surfSize;
            if (tileMode == 1)
            {
                pitch = width * bpp;

                if (roundPitch == 1)
                    pitch = round_up(pitch, 32);

                surfSize = pitch * height;
            }
            else
            {
                pitch = round_up(width * bpp, 64);
                surfSize = pitch * round_up(height, block_height * 8);
            }

            byte[] result = new byte[surfSize];

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    uint pos;
                    uint pos_;

                    if (tileMode == 1)
                        pos = y * pitch + x * bpp;
                    else
                        pos = getAddrBlockLinear(x, y, width, bpp, 0, block_height);

                    pos_ = (y * width + x) * bpp;

                    if (pos + bpp <= surfSize)
                    {
                        if (toSwizzle == 0)
                            Array.Copy(data, pos, result, pos_, bpp);
                        else
                            Array.Copy(data, pos_, result, pos, bpp);
                    }
                }
            }
            return result;
        }

        public static byte[] deswizzle(uint width, uint height, uint depth, uint blkWidth, uint blkHeight, uint blkDepth, int roundPitch, uint bpp, uint tileMode, int size_range, byte[] data)
        {
            return _swizzle(width, height, depth, blkWidth, blkHeight, blkDepth, roundPitch, bpp, tileMode, size_range, data, 0);
        }

        public static byte[] swizzle(uint width, uint height, uint depth, uint blkWidth, uint blkHeight, uint blkDepth, int roundPitch, uint bpp, uint tileMode, int size_range, byte[] data)
        {
            return _swizzle(width, height, depth, blkWidth, blkHeight, blkDepth, roundPitch, bpp, tileMode, size_range, data, 1);
        }

        static uint getAddrBlockLinear(uint x, uint y, uint width, uint bytes_per_pixel, uint base_address, uint block_height)
        {
            /*
              From Tega X1 TRM 
                               */
            uint image_width_in_gobs = DIV_ROUND_UP(width * bytes_per_pixel, 64);


            uint GOB_address = (base_address
                                + (y / (8 * block_height)) * 512 * block_height * image_width_in_gobs
                                + (x * bytes_per_pixel / 64) * 512 * block_height
                                + (y % (8 * block_height) / 8) * 512);

            x *= bytes_per_pixel;

            uint Address = (GOB_address + ((x % 64) / 32) * 256 + ((y % 8) / 2) * 64
                            + ((x % 32) / 16) * 32 + (y % 2) * 16 + (x % 16));
            return Address;
        }
    }
}