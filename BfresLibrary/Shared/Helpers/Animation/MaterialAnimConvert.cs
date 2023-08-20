using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Syroot.Maths;
using BfresLibrary.TextConvert;

namespace BfresLibrary.Helpers
{
    public class MaterialAnimHelper
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int FrameCount { get; set; }
        public bool Loop { get; set; }
        public bool Baked { get; set; }
        public List<string> TextureList = new List<string>();

        public List<MaterialDataAnimHelper> MaterialAnims { get; set; }

        [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
        public Dictionary<string, object> UserData { get; set; } = new Dictionary<string, object>();

        public static MaterialAnim FromStruct(MaterialAnimHelper matAnim)
        {
            MaterialAnim anim = new MaterialAnim();
            FromStruct(anim, matAnim);
            return anim;
        }

        public static void FromStruct(MaterialAnim anim, MaterialAnimHelper animJson)
        {
            anim.TextureNames = new ResDict<TextureRef>();
            foreach (var tex in animJson.TextureList)
                anim.TextureNames.Add(tex, new TextureRef() { Name = tex });

            anim.Name = animJson.Name;
            anim.Baked = animJson.Baked;
            anim.Loop = animJson.Loop;
            anim.FrameCount = animJson.FrameCount;
            anim.Baked = animJson.Baked;
            anim.MaterialAnimDataList = new List<MaterialAnimData>();
            anim.BindIndices = new ushort[animJson.MaterialAnims.Count];
            for (int i = 0; i < anim.BindIndices.Length; i++)
                anim.BindIndices[i] = 65535;

            anim.UserData = UserDataConvert.Convert(animJson.UserData);

            foreach (var matAnimJson in animJson.MaterialAnims)
            {
                MaterialAnimData matAnim = new MaterialAnimData();
                anim.MaterialAnimDataList.Add(matAnim);
                matAnim.Name = matAnimJson.Name;
                matAnim.ParamAnimInfos = new List<ParamAnimInfo>();
                matAnim.PatternAnimInfos = new List<PatternAnimInfo>();
                matAnim.ShaderParamCurveIndex = 0;
                List<ushort> texturePatternBase = new List<ushort>();

                foreach (var samplerInfo in matAnimJson.Samplers)
                {
                    var info = new PatternAnimInfo();
                    info.Name = samplerInfo.Name;
                    info.CurveIndex = -1;

                    matAnim.PatternAnimInfos.Add(info);
                    texturePatternBase.Add(samplerInfo.Constant);

                    if (samplerInfo.Curve != null)
                    {
                        info.CurveIndex = (short)matAnim.Curves.Count;
                        matAnim.Curves.Add(CurveAnimHelper.GenerateCurve(samplerInfo.Curve, 0, false));
                    }
                }

                ushort curveIndex = 0;
                ushort constantIndex = 0;

                foreach (var paramInfo in matAnimJson.Params)
                {
                    var info = new ParamAnimInfo();
                    info.BeginCurve = curveIndex;
                    info.BeginConstant = constantIndex;
                    info.ConstantCount = (ushort)paramInfo.Constants.Count;
                    info.FloatCurveCount = (ushort)paramInfo.Curves.Count();
                    info.IntCurveCount = 0;
                    info.Name = paramInfo.Name;
                    matAnim.ParamAnimInfos.Add(info);

                    Console.WriteLine($"Param {info.Name} constantIndex {constantIndex} ConstantCount {info.ConstantCount}");

                    if (paramInfo.Curves.Count > 0)
                        matAnim.VisualConstantIndex = 0;

                    foreach (var curveJson in paramInfo.Curves)
                    {
                        uint target = uint.Parse(curveJson.Target);
                        matAnim.Curves.Add(CurveAnimHelper.GenerateCurve(curveJson, target, false));
                    }
                    foreach (var constJson in paramInfo.Constants)
                        matAnim.Constants.Add(constJson);

                    curveIndex += (ushort)paramInfo.Curves.Count;
                    constantIndex += (ushort)paramInfo.Constants.Count;
                }
                matAnim.BaseDataList = texturePatternBase.ToArray();
            }
        }
    }

    public class MaterialDataAnimHelper
    {
        public string Name { get; set; }

        public List<ShaderParamAnimHelper> Params = new List<ShaderParamAnimHelper>();
        public List<SamplerAnimHelper> Samplers = new List<SamplerAnimHelper>();
    }

    public struct ShaderParamAnimHelper
    {
        public string Name { get; set; }

        public List<CurveAnimHelper> Curves { get; set; }
        public List<AnimConstant> Constants { get; set; }
    }

    public struct SamplerAnimHelper
    {
        public string Name { get; set; }

        public CurveAnimHelper Curve { get; set; }
        public ushort Constant { get; set; }
    }
}
