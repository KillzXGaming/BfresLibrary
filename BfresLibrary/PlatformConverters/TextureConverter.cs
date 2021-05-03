using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace BfresLibrary.PlatformConverters
{
    class TextureConverter
    {
        internal static BntxFile CreateBNTX(List<TextureShared> textureList)
        {
            BntxFile bntx = new BntxFile();
            bntx.Target = new char[] { 'N', 'X', ' ', ' ' };
            bntx.Name = "textures";
            bntx.Alignment = 0xC;
            bntx.TargetAddressSize = 0x40;
            bntx.VersionMajor = 0;
            bntx.VersionMajor2 = 4;
            bntx.VersionMinor = 0;
            bntx.VersionMinor2 = 0;
            bntx.Textures = new List<Texture>();
            bntx.TextureDict = new Syroot.NintenTools.NSW.Bntx.ResDict();
            bntx.RelocationTable = new RelocationTable();
            bntx.Flag = 0;

            foreach (var tex in textureList)
            {
                //Create a new switch texture instance
                var textureNX = new Switch.SwitchTexture(bntx, null);
                textureNX.FromWiiU((WiiU.Texture)tex);

                //Now set the new texture
                bntx.Textures.Add(textureNX.Texture);
                bntx.TextureDict.Add(textureNX.Name);
            }
            return bntx;
        }

        internal static Dictionary<GX2.GX2SurfaceFormat, SurfaceFormat> FormatList = new Dictionary<GX2.GX2SurfaceFormat, SurfaceFormat>()
        {
            { GX2.GX2SurfaceFormat.TC_R8_G8_B8_A8_SNorm, SurfaceFormat.R8_G8_B8_A8_SNORM },
            { GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNorm, SurfaceFormat.R8_G8_B8_A8_UNORM },
            { GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB, SurfaceFormat.R8_G8_B8_A8_SRGB },
            { GX2.GX2SurfaceFormat.TCS_R5_G6_B5_UNorm, SurfaceFormat.R5_G6_B5_UNORM },
            { GX2.GX2SurfaceFormat.TC_R5_G5_B5_A1_UNorm, SurfaceFormat.R5_G5_B5_A1_UNORM },
            { GX2.GX2SurfaceFormat.TC_A1_B5_G5_R5_UNorm, SurfaceFormat.A1_B5_G5_R5_UNORM },
            { GX2.GX2SurfaceFormat.TC_R8_G8_UNorm, SurfaceFormat.R8_G8_UNORM },
            { GX2.GX2SurfaceFormat.T_BC1_UNorm, SurfaceFormat.BC1_UNORM },
            { GX2.GX2SurfaceFormat.T_BC1_SRGB, SurfaceFormat.BC1_SRGB },
            { GX2.GX2SurfaceFormat.T_BC2_UNorm, SurfaceFormat.BC2_UNORM },
            { GX2.GX2SurfaceFormat.T_BC2_SRGB, SurfaceFormat.BC2_SRGB },
            { GX2.GX2SurfaceFormat.T_BC3_UNorm, SurfaceFormat.BC3_UNORM },
            { GX2.GX2SurfaceFormat.T_BC3_SRGB, SurfaceFormat.BC3_SRGB },
            { GX2.GX2SurfaceFormat.T_BC4_UNorm, SurfaceFormat.BC4_UNORM },
            { GX2.GX2SurfaceFormat.T_BC4_SNorm, SurfaceFormat.BC4_SNORM },
            { GX2.GX2SurfaceFormat.T_BC5_UNorm, SurfaceFormat.BC5_UNORM },
            { GX2.GX2SurfaceFormat.T_BC5_SNorm, SurfaceFormat.BC5_SNORM },
        };
    }
}
