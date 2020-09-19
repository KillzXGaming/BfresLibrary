using System.Collections.Generic;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a material animation in a <see cref="MaterialAnim"/> subfile, storing material animation data.
    /// </summary>
    public class PatternAnimInfo : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePatternAnimInfo"/> class.
        /// </summary>
        public PatternAnimInfo()
        {
            Name = "";
            CurveIndex = -1;
            BeginConstant = ushort.MaxValue;
            SubBindIndex = -1;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the curve in the <see cref="TexturePatternAnimInfo"/>.
        /// </summary>
        public short CurveIndex;

        /// <summary>
        /// Gets or sets the index of the first <see cref="AnimConstant"/> instance in the parent
        /// <see cref="ShaderParamMatAnim"/>.
        /// </summary>
        public ushort BeginConstant { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="KeyShape"/> in the <see cref="Shape"/>.
        /// </summary>
        public sbyte SubBindIndex;

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{MaterialAnim}"/>
        /// instances.
        /// </summary>
        public string Name
        {
            get; set;
        }

        // ---- METHODS (PUBLIC) ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Searches for a shader parameter in the materials that match this one
        /// </summary>
        /// <returns></returns>
        public ShaderParam FindParameter(string material, List<Model> models)
        {
            foreach (var model in models) {
                if (!model.Materials.ContainsKey(material))
                    continue;

                var mat = model.Materials[material];
                if (mat.ShaderParams.ContainsKey(this.Name))
                    return mat.ShaderParams[this.Name];
            }
            return null;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                Name = loader.LoadString();
                CurveIndex = loader.ReadInt16();
                BeginConstant = loader.ReadUInt16();
                SubBindIndex = loader.ReadSByte();
                loader.Seek(3); //padding
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
                saver.Write(BeginConstant);
                saver.Write(SubBindIndex);
                saver.Seek(3); //padding
            }
            else
            {
                saver.Write((sbyte)CurveIndex);
                saver.Write((sbyte)SubBindIndex);
                saver.Seek(2);
                saver.SaveString(Name);
            }
        }
    }
}
