using System;
using System.Diagnostics;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a reference to a <see cref="Texture"/> instance by name.
    /// </summary>
    [DebuggerDisplay(nameof(TextureRef) + " {" + nameof(Name) + "}")]
    public class TextureRef : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureRef"/> class.
        /// </summary>
        public TextureRef()
        {
            Name = "";
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{TextureRef}"/> instances. Typically the same as the <see cref="Texture.Name"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The referenced <see cref="Texture"/> instance.
        /// </summary>
        public TextureShared Texture { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Name = loader.LoadString();
            Texture = loader.Load<WiiU.Texture>();
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(Name);
            saver.Save(Texture);
        }
    }
}