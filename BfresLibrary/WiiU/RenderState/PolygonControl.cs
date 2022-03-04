using BfresLibrary.Core;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents GX2 polygon drawing settings controlling if and how triangles are rendered.
    /// </summary>
    public class PolygonControl
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _cullFrontBit = 0;
        private const int _cullBackBit = 1;
        private const int _frontFaceBit = 2;
        private const int _polygonModeBit = 3;
        private const int _polygonModeFrontBit = 5, _polygonModeFrontBits = 3;
        private const int _polygonModeBackBit = 8, _polygonModeBackBits = 3;
        private const int _polygonOffsetFrontBit = 11;
        private const int _polygonOffsetBackBit = 12;
        private const int _polygonLineOffsetBit = 13;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        public uint Value;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether front-facing polygons are culled.
        /// </summary>
        public bool CullFront
        {
            get { return Value.GetBit(_cullFrontBit); }
            set { Value = Value.SetBit(_cullFrontBit, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether back-facing polygons are culled.
        /// </summary>
        public bool CullBack
        {
            get { return Value.GetBit(_cullBackBit); }
            set { Value = Value.SetBit(_cullBackBit, value); }
        }

        /// <summary>
        /// Gets or sets the order in which vertices have to form the triangle to be handled as a front- rather than
        /// back-face.
        /// </summary>
        public GX2FrontFaceMode FrontFace
        {
            get { return Value.GetBit(_frontFaceBit) ? GX2FrontFaceMode.Clockwise : GX2FrontFaceMode.CounterClockwise; }
            set { Value = Value.SetBit(_frontFaceBit, value == GX2FrontFaceMode.Clockwise); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether polygons are drawn at all.
        /// </summary>
        public bool PolygonModeEnabled
        {
            get { return Value.GetBit(_polygonModeBit); }
            set { Value = Value.SetBit(_polygonModeBit, value); }
        }

        /// <summary>
        /// Gets or sets how front facing polygons are drawn.
        /// </summary>
        public GX2PolygonMode PolygonModeFront
        {
            get { return (GX2PolygonMode)Value.Decode(_polygonModeFrontBits, _polygonModeFrontBit); }
            set { Value = Value.Encode((uint)value, _polygonModeFrontBits, _polygonModeFrontBit); }
        }

        /// <summary>
        /// Gets or sets how back facing polygons are drawn.
        /// </summary>
        public GX2PolygonMode PolygonModeBack
        {
            get { return (GX2PolygonMode)Value.Decode(_polygonModeBackBits, _polygonModeBackBit); }
            set { Value = Value.Encode((uint)value, _polygonModeBackBits, _polygonModeBackBit); }
        }

        /// <summary>
        /// Gets or sets whether front-facing polygons are drawn offset (useful for decals to combat Z fighting).
        /// </summary>
        public bool PolygonOffsetFrontEnabled
        {
            get { return Value.GetBit(_polygonOffsetFrontBit); }
            set { Value = Value.SetBit(_polygonOffsetFrontBit, value); }
        }

        /// <summary>
        /// Gets or sets whether back-facing polygons are drawn offset (useful for decals to combat Z fighting).
        /// </summary>
        public bool PolygonOffsetBackEnabled
        {
            get { return Value.GetBit(_polygonOffsetBackBit); }
            set { Value = Value.SetBit(_polygonOffsetBackBit, value); }
        }

        /// <summary>
        /// Gets or sets whether lines are drawn offset (useful for decals to combat Z fighting).
        /// </summary>
        public bool PolygonLineOffsetEnabled
        {
            get { return Value.GetBit(_polygonLineOffsetBit); }
            set { Value = Value.SetBit(_polygonLineOffsetBit, value); }
        }
    }
}
