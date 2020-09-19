using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BfresLibrary.PlatformConverters
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
            //Ckear all Switch render infos not used by wii u

            RemoveRenderInfo(material, "gsys_bake_normal_map");
            RemoveRenderInfo(material, "gsys_bake_emission_map");
            RemoveRenderInfo(material, "gsys_render_state_display_face");
            RemoveRenderInfo(material, "gsys_render_state_mode");
            RemoveRenderInfo(material, "gsys_depth_test_enable");
            RemoveRenderInfo(material, "gsys_depth_test_func");
            RemoveRenderInfo(material, "gsys_depth_test_write");
            RemoveRenderInfo(material, "gsys_alpha_test_enable");
            RemoveRenderInfo(material, "gsys_alpha_test_func");
            RemoveRenderInfo(material, "gsys_alpha_test_value");
            RemoveRenderInfo(material, "gsys_render_state_blend_mode");
            RemoveRenderInfo(material, "gsys_color_blend_rgb_op");
            RemoveRenderInfo(material, "gsys_color_blend_rgb_src_func");
            RemoveRenderInfo(material, "gsys_color_blend_rgb_dst_func");
            RemoveRenderInfo(material, "gsys_color_blend_alpha_op");
            RemoveRenderInfo(material, "gsys_color_blend_alpha_src_func");
            RemoveRenderInfo(material, "gsys_color_blend_alpha_dst_func");
            RemoveRenderInfo(material, "gsys_color_blend_const_color");

            for (int i = 0; i < 5; i++)
                material.SetRenderInfo($"gsys_model_fx{i}", "default");
        }

        internal override void ConvertToSwitchMaterial(Material material)
        {
            //Convert all render info

            if (material.ShaderAssign.ShadingModelName == "turbo_uber_xlu" || material.Name.Contains("CausticsArea"))
            {
                material.Visible = false;
                return;
            }

            Console.WriteLine("Converting material " + material.Name);

            for (int i = 0; i < 5; i++)
                RemoveRenderInfo(material, $"gsys_model_fx{i}");

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

            material.SetRenderInfo("gsys_color_blend_const_color", new float[4] { 0,0,0,0 });

            material.SetShaderParameter("gsys_alpha_test_ref_value",
                ShaderParamType.Float, material.RenderState.AlphaRefValue);
            material.SetShaderParameter("gsys_xlu_zprepass_alpha",
                ShaderParamType.Float4, new float[4] { 1, 1, 1, 1 });
            material.SetShaderParameter("screen_fake_scale_begin_ratio",
                ShaderParamType.Float, 0.25f);
            material.SetShaderParameter("screen_fake_scale_factor",
                ShaderParamType.Float3, new float[3] { 0,0,0 });

            if (!material.ShaderParams.ContainsKey("d_shadow_bake_l_cancel_rate"))
                material.SetShaderParameter("d_shadow_bake_l_cancel_rate",
                    ShaderParamType.Float, 0.25f);

            if (!material.ShaderParams.ContainsKey("decal_trail_intensity"))
                material.SetShaderParameter("decal_trail_intensity",
                    ShaderParamType.Float, 1);

            if (!material.ShaderParams.ContainsKey("indirect_magB"))
                material.SetShaderParameter("indirect_magB",
                    ShaderParamType.Float2, new float[2] { 1, 1 });

            if (!material.ShaderParams.ContainsKey("decal_trail_color"))
                material.SetShaderParameter("decal_trail_color",
                    ShaderParamType.Float2, new float[3] { 1, 1,1 });
            
            material.ShaderParams.RemoveKey("effect_normal_offset");
            material.ShaderParams.RemoveKey("gsys_model_fx_ratio");
            material.ShaderParams.RemoveKey("gsys_specular_roughness");
            material.ShaderParams.RemoveKey("zprepass_shadow_rate");

            //Adjust shader params
            //Here we'll make a new shader param list in the same switch order
            List<ShaderParam> param = new List<ShaderParam>();

            param.Add(material.ShaderParams["edge_light_rim_i"]);
            param.Add(material.ShaderParams["d_shadow_bake_l_cancel_rate"]);
            param.Add(material.ShaderParams["gsys_i_color_ratio0"]);
            param.Add(material.ShaderParams["gsys_edge_ratio0"]);
            param.Add(material.ShaderParams["gsys_edge_width0"]);
            param.Add(material.ShaderParams["bloom_intensity"]);
            param.Add(material.ShaderParams["gsys_outline_width"]);
            param.Add(material.ShaderParams["gsys_alpha_threshold"]);
            param.Add(material.ShaderParams["game_edge_pow"]);
            param.Add(material.ShaderParams["edge_alpha_scale"]);
            param.Add(material.ShaderParams["post_multi_texture"]);
            param.Add(material.ShaderParams["edge_alpha_width"]);
            param.Add(material.ShaderParams["edge_alpha_pow"]);
            param.Add(material.ShaderParams["transparency"]);
            param.Add(material.ShaderParams["alphat_out_start"]);
            param.Add(material.ShaderParams["alphat_out_end"]);
            param.Add(material.ShaderParams["gsys_area_env_index_diffuse"]);
            param.Add(material.ShaderParams["shadow_density"]);
            param.Add(material.ShaderParams["ao_density"]);
            param.Add(material.ShaderParams["transmit_intensity"]);
            param.Add(material.ShaderParams["edge_light_vc_intensity"]);
            param.Add(material.ShaderParams["specular_aniso_power"]);
            param.Add(material.ShaderParams["transmit_shadow_intensity"]);
            param.Add(material.ShaderParams["edge_light_intensity"]);
            param.Add(material.ShaderParams["light_pre_pass_intensity"]);
            param.Add(material.ShaderParams["gsys_bake_opacity"]);
            param.Add(material.ShaderParams["shiny_specular_intensity"]);
            param.Add(material.ShaderParams["specular_intensity"]);
            param.Add(material.ShaderParams["specular_roughness"]);
            param.Add(material.ShaderParams["specular_fresnel_i"]);
            param.Add(material.ShaderParams["specular_fresnel_s"]);
            param.Add(material.ShaderParams["specular_fresnel_m"]);
            param.Add(material.ShaderParams["shiny_specular_sharpness"]);
            param.Add(material.ShaderParams["emission_intensity"]);
            param.Add(material.ShaderParams["soft_edge_dist_inv"]);
            param.Add(material.ShaderParams["silhoutte_depth"]);
            param.Add(material.ShaderParams["refraction_intensity"]);
            param.Add(material.ShaderParams["normal_map_weight"]);
            param.Add(material.ShaderParams["shiny_specular_fresnel"]);
            param.Add(material.ShaderParams["silhoutte_depth_contrast"]);
            param.Add(material.ShaderParams["fresnel_look_depend_factor"]);
            param.Add(material.ShaderParams["mii_hair_specular_intensity"]);
            param.Add(material.ShaderParams["decal_trail_intensity"]);
            param.Add(material.ShaderParams["screen_fake_scale_begin_ratio"]);
            param.Add(material.ShaderParams["fog_emission_intensity"]);
            param.Add(material.ShaderParams["fog_emission_effect"]);
            param.Add(material.ShaderParams["fog_edge_power"]);
            param.Add(material.ShaderParams["fog_edge_width"]);
            param.Add(material.ShaderParams["gsys_alpha_test_ref_value"]);
            param.Add(material.ShaderParams["edge_light_sharpness"]);
            param.Add(material.ShaderParams["indirect_mag"]);
            param.Add(material.ShaderParams["indirect_magB"]);
            param.Add(material.ShaderParams["silhoutte_depth_color"]);
            param.Add(material.ShaderParams["gsys_mii_skin_color"]);
            param.Add(material.ShaderParams["gsys_mii_favorite_color"]);
            param.Add(material.ShaderParams["gsys_point_light_color"]);
            param.Add(material.ShaderParams["gsys_edge_color0"]);
            param.Add(material.ShaderParams["transmit_color"]);
            param.Add(material.ShaderParams["gsys_i_color0"]);
            param.Add(material.ShaderParams["gsys_i_color0_b"]);
            param.Add(material.ShaderParams["gsys_bake_light_scale"]);
            param.Add(material.ShaderParams["gsys_bake_light_scale1"]);
            param.Add(material.ShaderParams["gsys_bake_light_scale2"]);
            param.Add(material.ShaderParams["albedo_tex_color"]);
            param.Add(material.ShaderParams["decal_trail_color"]);
            param.Add(material.ShaderParams["fog_emission_color"]);
            param.Add(material.ShaderParams["screen_fake_scale_factor"]);
            param.Add(material.ShaderParams["edge_light_color"]);
            param.Add(material.ShaderParams["shiny_specular_color"]);
            param.Add(material.ShaderParams["specular_color"]);
            param.Add(material.ShaderParams["emission_color"]);
            param.Add(material.ShaderParams["gsys_depth_silhoutte_color"]);
            param.Add(material.ShaderParams["gsys_outline_color"]);
            param.Add(material.ShaderParams["gsys_area_env_data0"]);
            param.Add(material.ShaderParams["gsys_area_env_data1"]);
            param.Add(material.ShaderParams["gsys_bake_st0"]);
            param.Add(material.ShaderParams["gsys_bake_st1"]);
            param.Add(material.ShaderParams["multi_tex_reg0"]);
            param.Add(material.ShaderParams["multi_tex_reg1"]);
            param.Add(material.ShaderParams["multi_tex_param0"]);
            param.Add(material.ShaderParams["fog_edge_color"]);
            param.Add(material.ShaderParams["gsys_xlu_zprepass_alpha"]);
            param.Add(material.ShaderParams["multi_tex_reg2"]);
            param.Add(material.ShaderParams["gsys_sssss_color"]);
            param.Add(material.ShaderParams["tex_mtx1"]);
            param.Add(material.ShaderParams["tex_mtx2"]);
            param.Add(material.ShaderParams["tex_mtx0"]);

            material.ShaderParams.Clear();
            foreach (var prm in param)
                material.ShaderParams.Add(prm.Name, prm);

            //Adjust shader options

            //gsys_alpha_test_enable
            //gsys_alpha_test_func
            //enable_screen_fake_scale

            var shaderAssign = material.ShaderAssign;

            shaderAssign.ShaderOptions.RemoveKey("enable_effect_normal_offset");
            shaderAssign.ShaderOptions.RemoveKey("bake_calc_type");

            ResDict<ResString> shaderOptions = new ResDict<ResString>();

            var keys = shaderAssign.ShaderOptions.Keys.ToList();
            var values = shaderAssign.ShaderOptions.Values.ToList();
            int opIndex = 0;
            for (int i = 0; i < keys.Count + 3; i++)
            {
                if (i == 0)
                {
                    shaderOptions.Add("gsys_alpha_test_enable",
                        material.RenderState.AlphaControl.AlphaTestEnabled ? "0" : "0");
                }
                else if (i == 1)
                {
                    shaderOptions.Add("gsys_alpha_test_func", GetShaderOptionAlphaTestFunc(
                        CompareFunction[material.RenderState.AlphaControl.AlphaFunc]));
                }   
                else if (i == 100)
                {
                    shaderOptions.Add("enable_screen_fake_scale", "0");
                }
                else
                {
                    shaderOptions.Add(keys[opIndex], values[opIndex]);
                    opIndex++;
                }
            }
            shaderAssign.ShaderOptions = shaderOptions;
        }

        private void RemoveRenderInfo(Material mat, string key)
        {
            if (mat.RenderInfos.ContainsKey(key))
                mat.RenderInfos.RemoveKey(key);
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

        private string GetShaderOptionAlphaTestFunc(string func)
        {
            switch (func)
            {
                case "gequal": return "6";
                default:
                    throw new Exception("Unuspported alpha test func! " + func);
            }
        }
    }
}
