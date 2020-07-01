using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.Bfres.Core;
using Syroot.NintenTools.Bfres.Swizzling;
using Syroot.NintenTools.NSW.Bntx.GFX;

namespace Syroot.NintenTools.Bfres.Switch
{
    public class SwitchTexture : TextureShared, IResData
    {
        /// <summary>
        /// The attached BNTX texture instance used.
        /// </summary>
        public Texture Texture { get; set; }

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

        public SwitchTexture(Texture texture) {
            Texture = texture;

            Width = texture.Width;
            Height = texture.Height;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public override byte[] GetSwizzledData() {
            //Combine all the arrays from lists
            List<byte[]> arrayList = new List<byte[]>();
            foreach (var arrayData in Texture.TextureData)
                arrayList.Add(ByteUtils.CombineArray(arrayData.ToArray()));
            return ByteUtils.CombineArray(arrayList.ToArray());
        }

        public override byte[] GetDeswizzledData(int arrayLevel, int mipLevel)
        {
            if (Texture.TextureData.Count < arrayLevel)
                throw new Exception($"Invalid array level! {arrayLevel}");
            if (Texture.TextureData[arrayLevel].Count < mipLevel)
                throw new Exception($"Invalid mip level! {mipLevel}");

            return TegraX1Swizzle.GetImageData(Texture,
                Texture.TextureData[arrayLevel][mipLevel], arrayLevel, mipLevel, 0,
                1, Texture.TileMode == TileMode.LinearAligned);
        }

        void IResData.Load(ResFileLoader loader)
        {
        }

        void IResData.Save(ResFileSaver saver)
        {

        }
    }
}
