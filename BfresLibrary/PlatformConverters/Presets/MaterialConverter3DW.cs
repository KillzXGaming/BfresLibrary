using System.Collections.Generic;

namespace BfresLibrary.PlatformConverters
{
    internal class MaterialConverter3DW : MaterialConverterBase
    {
        //Changes with Switch 3DW

        // Parameters
        //alpha_test_value (Seems to be const "0,5")

        // Render Info
        //color_blend_alpha_dst_func
        //color_blend_alpha_op
        //color_blend_alpha_src_func
        //color_blend_const_color
        //color_blend_rgb_dst_func
        //color_blend_rgb_op
        //color_blend_rgb_src_func
        //depth_test_func
        //display_face
        //enable_depth_test
        //enable_depth_write

        internal override void ConvertToWiiUMaterial(Material material)
        {
        }

        internal override void ConvertToSwitchMaterial(Material material) 
        {
            material.SetRenderInfo("color_blend_alpha_dst_func", BlendFunction[
                  material.RenderState.BlendControl.AlphaDestinationBlend]);
            material.SetRenderInfo("color_blend_alpha_op", BlendCombine[
                  material.RenderState.BlendControl.AlphaCombine]);
            material.SetRenderInfo("color_blend_alpha_src_func", BlendFunction[
                  material.RenderState.BlendControl.AlphaSourceBlend]);

            material.SetRenderInfo("color_blend_const_color", new float[4] { 0, 0, 0, 0 });

            material.SetRenderInfo("color_blend_rgb_dst_func", BlendFunction[
                  material.RenderState.BlendControl.ColorSourceBlend]);
            material.SetRenderInfo("color_blend_rgb_op", BlendFunction[
                  material.RenderState.BlendControl.ColorDestinationBlend]);
            material.SetRenderInfo("color_blend_rgb_src_func", BlendCombine[
                  material.RenderState.BlendControl.ColorCombine]);

            material.SetRenderInfo("display_face", GetCullState(material.RenderState));

            material.SetRenderInfo("enable_depth_test", RenderInfoBoolString(
                material.RenderState.DepthControl.DepthTestEnabled));
            material.SetRenderInfo("enable_depth_write", RenderInfoBoolString(
                material.RenderState.DepthControl.DepthWriteEnabled));
            material.SetRenderInfo("depth_test_func", CompareFunction[
                  material.RenderState.DepthControl.DepthFunc]);
        }

        private string GetCullState(RenderState state) {
            if(state.PolygonControl.CullBack && state.PolygonControl.CullFront)
                return "none";
            else if(state.PolygonControl.CullBack)
                return "front";
            else if(state.PolygonControl.CullFront)
                return "back";
            return "both";
        }

        Dictionary<GX2.GX2BlendCombine, string> BlendCombine = new Dictionary<GX2.GX2BlendCombine, string>()
        {
            { GX2.GX2BlendCombine.Add, "add" },
            { GX2.GX2BlendCombine.Maximum, "max" },
            { GX2.GX2BlendCombine.Minimum, "min" },
        };


        Dictionary<GX2.GX2CompareFunction, string> CompareFunction = new Dictionary<GX2.GX2CompareFunction, string>()
        {
            { GX2.GX2CompareFunction.Always, "always" },
            { GX2.GX2CompareFunction.Never, "never" },
            { GX2.GX2CompareFunction.GreaterOrEqual, "gequal" },
            { GX2.GX2CompareFunction.LessOrEqual, "lequal" },
            { GX2.GX2CompareFunction.Equal, "equal" },
            { GX2.GX2CompareFunction.Less, "less" },
            { GX2.GX2CompareFunction.Greater, "greater" },
            { GX2.GX2CompareFunction.NotEqual, "noequal" },
        };

        Dictionary<GX2.GX2BlendFunction, string> BlendFunction = new Dictionary<GX2.GX2BlendFunction, string>()
        {
            { GX2.GX2BlendFunction.OneMinusSourceAlpha, "one_minus_src_alpha" },
            { GX2.GX2BlendFunction.SourceAlpha, "src_alpha" },
            { GX2.GX2BlendFunction.SourceColor, "src_color" },
            //Todo confirm these 2
            { GX2.GX2BlendFunction.ConstantAlpha, "const_alpha" },
            { GX2.GX2BlendFunction.ConstantColor, "const_color" },

            { GX2.GX2BlendFunction.DestinationAlpha, "dst_alpha" },
            { GX2.GX2BlendFunction.One, "one" },
            { GX2.GX2BlendFunction.Zero, "zero" },
        };
    }
}
