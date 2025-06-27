using System.Diagnostics;
using BfresLibrary.Core;
using BfresLibrary.GX2;
using System.ComponentModel;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ResFile"/>, storing multi-dimensional texture data.
    /// </summary>
    [DebuggerDisplay(nameof(TextureShared) + " {" + nameof(Name) + "}")]
    public class TextureShared : IResData, IBinarySection, INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class.
        /// </summary>
        public TextureShared()
        {

        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Texture}"/>
        /// instances.
        /// </summary>
        [Browsable(true)]
        [Description("Name")]
        [Category("Image Info")]
        [DisplayName("Name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        [Browsable(true)]
        [Description("The path the file was originally located.")]
        [Category("Image Info")]
        [DisplayName("Path")]
        public virtual string Path { get; set; }

        /// <summary>
        /// Gets or sets the width of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Width of the image")]
        [Category("Image Info")]
        [DisplayName("Width")]
        public virtual uint Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Height of the image")]
        [Category("Image Info")]
        [DisplayName("Height")]
        public virtual uint Height { get; set; }

        /// <summary>
        /// Gets or sets the depth of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Depth")]
        [DisplayName("Depth")]
        public virtual uint Depth { get; set; }

        /// <summary>
        /// Gets or sets the number of mipmaps stored in the <see cref="MipData"/>.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Number of mip maps")]
        [Category("Image Info")]
        [DisplayName("Mip Count")]
        public virtual uint MipCount { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Number of array images")]
        [Category("Image Info")]
        [DisplayName("Array Count")]
        public virtual uint ArrayLength { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<UserData> UserData { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public void Import(System.IO.Stream stream, ResFile ResFile) {
            ResFileLoader.ImportSection(stream, this, ResFile);
        }

        public virtual void Import(string FileName, ResFile ResFile) {
            ResFileLoader.ImportSection(FileName, this, ResFile);
        }

        public virtual void Export(string FileName, ResFile ResFile) {
            ResFileSaver.ExportSection(FileName, this, ResFile);
        }

        public virtual byte[] GetSwizzledData() {
            return null;
        }

        public virtual byte[] GetDeswizzledData(int arrayLevel, int mipLevel) {
            return null;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
        }

        void IResData.Save(ResFileSaver saver)
        {

        }
    }
}