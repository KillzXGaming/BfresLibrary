using BfresLibrary.Core;

namespace BfresLibrary.GX2
{
    /// <summary>
    /// Represents GX2 settings controlling how depth and stencil buffer checks are performed and handled.
    /// </summary>
    public class DepthControl
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _stencilBit = 0;
        private const int _depthTestBit = 1;
        private const int _depthWriteBit = 2;
        private const int _depthFuncBit = 4, _depthFuncBits = 3;
        private const int _backStencilBit = 7;
        private const int _frontStencilFuncBit = 8, _frontStencilFuncBits = 3;
        private const int _frontStencilFailBit = 11, _frontStencilFailBits = 3;
        private const int _frontStencilZPassBit = 14, _frontStencilZPassBits = 3;
        private const int _frontStencilZFailBit = 17, _frontStencilZFailBits = 3;
        private const int _backStencilFuncBit = 20, _backStencilFuncBits = 3;
        private const int _backStencilFailBit = 23, _backStencilFailBits = 3;
        private const int _backStencilZPassBit = 26, _backStencilZPassBits = 3;
        private const int _backStencilZFailBit = 29, _backStencilZFailBits = 3;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal uint Value;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether depth testing is enabled.
        /// </summary>
        public bool DepthTestEnabled
        {
            get { return Value.GetBit(_depthTestBit); }
            set { Value = Value.SetBit(_depthTestBit, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether writing to the depth buffer is enabled.
        /// </summary>
        public bool DepthWriteEnabled
        {
            get { return Value.GetBit(_depthWriteBit); }
            set { Value = Value.SetBit(_depthWriteBit, value); }
        }

        /// <summary>
        /// Gets or sets the depth buffer comparison function, controlling whether a new fragment is allowed to
        /// overwrite the old value in the depth buffer.
        /// </summary>
        public GX2CompareFunction DepthFunc
        {
            get { return (GX2CompareFunction)Value.Decode(_depthFuncBit, _depthFuncBits); }
            set { Value = Value.Encode((uint)value, _depthFuncBit, _depthFuncBits); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether stencil testing is enabled.
        /// </summary>
        public bool StencilTestEnabled
        {
            get { return Value.GetBit(_stencilBit); }
            set { Value = Value.SetBit(_stencilBit, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether back-facing polygons are allowed to write to the stencil buffer or
        /// not.
        /// </summary>
        public bool BackStencilEnabled
        {
            get { return Value.GetBit(_backStencilBit); }
            set { Value = Value.SetBit(_backStencilBit, value); }
        }

        /// <summary>
        /// Gets or sets the front-facing polygon stencil comparison function.
        /// </summary>
        public GX2CompareFunction FrontStencilFunc
        {
            get { return (GX2CompareFunction)Value.Decode(_frontStencilFuncBit, _frontStencilFuncBits); }
            set { Value = Value.Encode((uint)value, _frontStencilFuncBit, _frontStencilFuncBits); }
        }

        /// <summary>
        /// Gets or sets the stencil function configuring what to do with the existing stencil value when the stencil
        /// test fails for front-facing polygons.
        /// </summary>
        public GX2StencilFunction FrontStencilFail
        {
            get { return (GX2StencilFunction)Value.Decode(_frontStencilFailBit, _frontStencilFailBits); }
            set { Value = Value.Encode((uint)value, _frontStencilFailBit, _frontStencilFailBits); }
        }

        /// <summary>
        /// Gets or sets the stencil function taking effect when the stencil test passes with the depth buffer for
        /// front-facing polygons.
        /// </summary>
        public GX2StencilFunction FrontStencilZPass
        {
            get { return (GX2StencilFunction)Value.Decode(_frontStencilZPassBit, _frontStencilZPassBits); }
            set { Value = Value.Encode((uint)value, _frontStencilZPassBit, _frontStencilZPassBits); }
        }

        /// <summary>
        /// Gets or sets the function taking effect when the stencil test fails with the depth buffer for front-facing
        /// polygons.
        /// </summary>
        public GX2StencilFunction FrontStencilZFail
        {
            get { return (GX2StencilFunction)Value.Decode(_frontStencilZFailBit, _frontStencilZFailBits); }
            set { Value = Value.Encode((uint)value, _frontStencilZFailBit, _frontStencilZFailBits); }
        }

        /// <summary>
        /// Gets or sets the back-facing polygon stencil comparison function.
        /// </summary>
        public GX2CompareFunction BackStencilFunc
        {
            get { return (GX2CompareFunction)Value.Decode(_backStencilFuncBit, _backStencilFuncBits); }
            set { Value = Value.Encode((uint)value, _backStencilFuncBit, _backStencilFuncBits); }
        }

        /// <summary>
        /// Gets or sets the stencil function configuring what to do with the existing stencil value when the stencil
        /// test fails for back-facing polygons.
        /// </summary>
        public GX2StencilFunction BackStencilFail
        {
            get { return (GX2StencilFunction)Value.Decode(_backStencilFailBit, _backStencilFailBits); }
            set { Value = Value.Encode((uint)value, _backStencilFailBit, _backStencilFailBits); }
        }

        /// <summary>
        /// Gets or sets the stencil function taking effect when the stencil test passes with the depth buffer for
        /// back-facing polygons.
        /// </summary>
        public GX2StencilFunction BackStencilZPass
        {
            get { return (GX2StencilFunction)Value.Decode(_backStencilZPassBit, _backStencilZPassBits); }
            set { Value = Value.Encode((uint)value, _backStencilZPassBit, _backStencilZPassBits); }
        }

        /// <summary>
        /// Gets or sets the function taking effect when the stencil test fails with the depth buffer for back-facing
        /// polygons.
        /// </summary>
        public GX2StencilFunction BackStencilZFail
        {
            get { return (GX2StencilFunction)Value.Decode(_backStencilZFailBit, _backStencilZFailBits); }
            set { Value = Value.Encode((uint)value, _backStencilZFailBit, _backStencilZFailBits); }
        }
    }
}