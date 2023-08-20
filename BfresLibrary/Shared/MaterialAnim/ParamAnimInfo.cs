using System.Collections.Generic;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a material animation in a <see cref="MaterialAnim"/> subfile, storing material animation data.
    /// </summary>
    public class ParamAnimInfo : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParamAnimInfo"/> class.
        /// </summary>
        public ParamAnimInfo()
        {
            Name = "";

            BeginCurve = 0;
            FloatCurveCount = 0;
            IntCurveCount = 0;
            BeginConstant = 0;
            ConstantCount = 0;
            SubBindIndex = ushort.MaxValue;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the first <see cref="AnimCurve"/> instance in the parent
        /// <see cref="ShaderParamMatAnim"/>.
        /// </summary>
        public ushort BeginCurve { get; set; }

        public ushort FloatCurveCount { get; set; }

        public ushort IntCurveCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the first <see cref="AnimConstant"/> instance in the parent
        /// <see cref="ShaderParamMatAnim"/>.
        /// </summary>
        public ushort BeginConstant { get; set; }

        /// <summary>
        /// Gets or sets the number of <see cref="AnimConstant"/> instances used in the parent
        /// <see cref="ShaderParamMatAnim"/>.
        /// </summary>
        public ushort ConstantCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="ShaderParam"/> in the <see cref="Material"/>.
        /// </summary>
        public ushort SubBindIndex { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{MaterialAnim}"/>
        /// instances.
        /// </summary>
        public string Name
        {
            get; set;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                Name = loader.LoadString();
                BeginCurve = loader.ReadUInt16();
                FloatCurveCount = loader.ReadUInt16();
                IntCurveCount = loader.ReadUInt16();
                BeginConstant = loader.ReadUInt16();
                ConstantCount = loader.ReadUInt16();
                SubBindIndex = loader.ReadUInt16();
                loader.Seek(4); //padding
            }
            else
            {
                BeginCurve = loader.ReadUInt16();
                FloatCurveCount = loader.ReadUInt16();
                IntCurveCount = loader.ReadUInt16();
                BeginConstant = loader.ReadUInt16();
                ConstantCount = loader.ReadUInt16();
                SubBindIndex = loader.ReadUInt16();
                Name = loader.LoadString();
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                saver.SaveString(Name);
                saver.Write(BeginCurve);
                saver.Write(FloatCurveCount);
                saver.Write(IntCurveCount);
                saver.Write(BeginConstant);
                saver.Write(ConstantCount);
                saver.Write(SubBindIndex);
                saver.Write(0); //padding
            }
            else
            {
                saver.Write(BeginCurve);
                saver.Write(FloatCurveCount);
                saver.Write(IntCurveCount);
                saver.Write(BeginConstant);
                saver.Write(ConstantCount);
                saver.Write(SubBindIndex);
                saver.SaveString(Name);
            }
        }
    }
}
