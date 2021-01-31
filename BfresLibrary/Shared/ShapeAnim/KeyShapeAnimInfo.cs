using System.Diagnostics;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a key shape animation info in a <see cref="VertexShapeAnim"/> instance.
    /// </summary>
    [DebuggerDisplay(nameof(KeyShapeAnimInfo) + " {" + nameof(Name) + "}")]
    public class KeyShapeAnimInfo : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the curve in the <see cref="VertexShapeAnim"/>.
        /// </summary>
        public sbyte CurveIndex;

        /// <summary>
        /// Gets or sets the index of the <see cref="KeyShape"/> in the <see cref="Shape"/>.
        /// </summary>
        public sbyte SubBindIndex;

        /// <summary>
        /// Gets or sets the name of the <see cref="KeyShape"/> in the <see cref="Shape"/>.
        /// </summary>
        public string Name;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                Name = loader.LoadString();
                CurveIndex = loader.ReadSByte();
                SubBindIndex = loader.ReadSByte();
                loader.Seek(6);
            }
            else
            {
                CurveIndex = loader.ReadSByte();
                SubBindIndex = loader.ReadSByte();
                loader.Seek(2);
                Name = loader.LoadString();
            }
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                saver.SaveString(Name);
                saver.Write(CurveIndex);
                saver.Write(SubBindIndex);
                saver.Seek(6);
            }
            else
            {
                saver.Write(CurveIndex);
                saver.Write(SubBindIndex);
                saver.Seek(2);
                saver.SaveString(Name);
            }
        }
    }
}