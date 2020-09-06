using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syroot.NintenTools.Bfres.PlatformConverters
{
    internal class MaterialConverterMK8 : MaterialConverterBase
    {
        //MK8D changes:

        //NEW RENDER INFO
        //gsys_bake_normal_map
        //gsys_bake_emission_map
        //gsys_render_state_display_face
        //gsys_render_state_mode
        //gsys_depth_test_enable
        //gsys_depth_test_func
        //gsys_depth_test_write
        //gsys_alpha_test_enable
        //gsys_alpha_test_func
        //gsys_alpha_test_value
        //gsys_render_state_blend_mode
        //gsys_color_blend_rgb_op
        //gsys_color_blend_rgb_src_func
        //gsys_color_blend_rgb_dst_func
        //gsys_color_blend_alpha_op
        //gsys_color_blend_alpha_src_func
        //gsys_color_blend_alpha_dst_func
        //gsys_color_blend_const_color

        //REMOVED RENDER INFO
        //gsys_model_fx##

        //NEW PARAMS
        //gsys_alpha_test_ref_value
        //gsys_xlu_zprepass_alpha
        //screen_fake_scale_begin_ratio
        //screen_fake_scale_factor

        //REMOVED PARAMS
        //effect_normal_offset
        //gsys_model_fx_ratio
        //gsys_specular_roughness
        //zprepass_shadow_rate

        //NEW OPTIONS:
        //gsys_alpha_test_enable
        //gsys_alpha_test_func
        //enable_screen_fake_scale

        //REMOVED OPTIONS:
        //bake_calc_type
        //enable_effect_normal_offset

        internal override void ConvertToWiiUMaterial(Material material)
        {

        }

        internal override void ConvertToSwitchMaterial(Material material)
        {
            //Convert all render info

            for (int i = 0; i < 5; i++)
            {
                if (material.RenderInfos.ContainsKey($"gsys_model_fx{i}"))
                    material.RenderInfos.Remove($"gsys_model_fx{i}");
            }
      
            material.SetRenderInfo("gsys_bake_normal_map", "default");
            material.SetRenderInfo("gsys_bake_emission_map", "default");
            material.SetRenderInfo("gsys_render_state_display_face", GetCullState(material.RenderState));
            material.SetRenderInfo("gsys_render_state_mode", GetRenderState(material.RenderState));
            material.SetRenderInfo("gsys_depth_test_enable", RenderInfoBoolString(
                material.RenderState.DepthControl.DepthTestEnabled));
            material.SetRenderInfo("gsys_depth_test_func", CompareFunction[
                  material.RenderState.DepthControl.DepthFunc]);
            material.SetRenderInfo("gsys_depth_test_write", RenderInfoBoolString(
                material.RenderState.DepthControl.DepthWriteEnabled));
            material.SetRenderInfo("gsys_alpha_test_enable", RenderInfoBoolString(
                material.RenderState.AlphaControl.AlphaTestEnabled));
            material.SetRenderInfo("gsys_alpha_test_func", CompareFunction[
                  material.RenderState.AlphaControl.AlphaFunc]);
            material.SetRenderInfo("gsys_alpha_test_value",
                  material.RenderState.AlphaRefValue);
            material.SetRenderInfo("gsys_render_state_blend_mode", GetBlendMode(material.RenderState));

            material.SetRenderInfo("gsys_color_blend_rgb_op", BlendCombine[
                  material.RenderState.BlendControl.ColorCombine]);
            material.SetRenderInfo("gsys_color_blend_rgb_src_func", BlendFunction[
                  material.RenderState.BlendControl.ColorSourceBlend]);
            material.SetRenderInfo("gsys_color_blend_rgb_dst_func", BlendFunction[
                  material.RenderState.BlendControl.ColorDestinationBlend]);

            material.SetRenderInfo("gsys_color_blend_alpha_op", BlendCombine[
                  material.RenderState.BlendControl.AlphaCombine]);
            material.SetRenderInfo("gsys_color_blend_alpha_src_func", BlendFunction[
                  material.RenderState.BlendControl.AlphaSourceBlend]);
            material.SetRenderInfo("gsys_color_blend_alpha_dst_func", BlendFunction[
                  material.RenderState.BlendControl.AlphaDestinationBlend]);

            material.SetRenderInfo("gsys_color_blend_const_color", new int[4] { 0,0,0,0 });
        }

        private string GetCullState(RenderState state)
        {
            if (state.PolygonControl.CullBack && state.PolygonControl.CullFront)
                return "none";
            else if (state.PolygonControl.CullBack)
                return "front";
            else if (state.PolygonControl.CullFront) 
                return "back";
            return "both";
        }

        private string GetRenderState(RenderState state)
        {
            if (state.FlagsMode == RenderStateFlagsMode.Opaque)
                return "opaque";
            else if (state.FlagsMode == RenderStateFlagsMode.AlphaMask)
                return "mask";
            else if (state.FlagsMode == RenderStateFlagsMode.Translucent)
                return "translucent";
            else
                return "custom";
        }

        private string GetBlendMode(RenderState state)
        {
            if (state.FlagsBlendMode == RenderStateFlagsBlendMode.Color)
                return "color";
            else if (state.FlagsBlendMode == RenderStateFlagsBlendMode.Logical)
                return "logic";
            else
                return "none";
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
