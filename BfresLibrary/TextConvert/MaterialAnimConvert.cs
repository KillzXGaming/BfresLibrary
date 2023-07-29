using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BfresLibrary.TextConvert
{
    public class MaterialAnimConvert
    {
        internal class MaterialAnimStuct
        {
            public string Name { get; set; }
            public string Path { get; set; }

            public int FrameCount { get; set; }
            public bool Loop { get; set; }
            public bool Baked { get; set; }

            public List<MaterialAnimGroupStruct> MaterialAnims { get; set; }

            public List<string> Textures { get; set; } = new List<string>();


            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> UserData { get; set; } = new Dictionary<string, object>();
        }

        internal class MaterialAnimGroupStruct
        {
            public string Name { get; set; }

            public List<ushort> BaseDataList = new List<ushort>();

            public List<PatternAnimInfo> PatternAnimInfos { get; set; }
            public List<ParamAnimInfo> ParameterInfos { get; set; }

            public List<AnimConstant> Constants { get; set; }
            public List<CurveAnimHelper> Curves { get; set; }
        }

        public static string ToJson(MaterialAnim anim)
        {
            MaterialAnimStuct animConv = new MaterialAnimStuct();
            animConv.Name = anim.Name;
            animConv.Path = anim.Path;
            animConv.MaterialAnims = new List<MaterialAnimGroupStruct>();
            animConv.FrameCount = anim.FrameCount;
            animConv.Loop = anim.Flags.HasFlag(MaterialAnim.MaterialAnimFlags.Looping);
            animConv.Baked = anim.Flags.HasFlag(MaterialAnim.MaterialAnimFlags.BakedCurve);

            animConv.Textures.Clear();
            foreach (var tex in anim.TextureNames)
                animConv.Textures.Add(tex.Key);

            foreach (var matAnim in anim.MaterialAnimDataList) {
                MaterialAnimGroupStruct matAnimConv = new MaterialAnimGroupStruct();
                animConv.MaterialAnims.Add(matAnimConv);

                matAnimConv.Curves = new List<CurveAnimHelper>();
                matAnimConv.Name = matAnim.Name;
                if (matAnim.Constants != null)
                    matAnimConv.Constants = matAnim.Constants.ToList();
                if (matAnim.BaseDataList != null)
                    matAnimConv.BaseDataList = matAnim.BaseDataList.ToList();
                if (matAnim.ParamAnimInfos != null)
                    matAnimConv.ParameterInfos = matAnim.ParamAnimInfos.ToList();
                if (matAnim.PatternAnimInfos != null)
                    matAnimConv.PatternAnimInfos = matAnim.PatternAnimInfos.ToList();

                foreach (var curve in matAnim.Curves) {
                    string target = curve.AnimDataOffset.ToString();

                    var convCurve = CurveAnimHelper.FromCurve(curve, target, false);
                    matAnimConv.Curves.Add(convCurve);
                }
            }
     
            foreach (var param in anim.UserData.Values)
                animConv.UserData.Add($"{param.Type}|{param.Name}", param.GetData());

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            };

            return JsonConvert.SerializeObject(animConv, Formatting.Indented);
        }

        public static MaterialAnim FromJson(string json)
        {
            MaterialAnim anim = new MaterialAnim();
            FromJson(anim, json);
            return anim;
        }

        public static void FromJson(MaterialAnim anim, string json)
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            };

            var animJson = JsonConvert.DeserializeObject<MaterialAnimStuct>(json);

            anim.Name = animJson.Name;
            anim.MaterialAnimDataList = new List<MaterialAnimData>();
            anim.UserData = UserDataConvert.Convert(animJson.UserData);
            anim.FrameCount = animJson.FrameCount;
            if (animJson.Loop)
                anim.Flags |= MaterialAnim.MaterialAnimFlags.Looping;
            if (animJson.Baked)
                anim.Flags |= MaterialAnim.MaterialAnimFlags.BakedCurve;

            foreach (var tex in animJson.Textures)
                anim.TextureNames.Add(tex, new TextureRef()
                {
                    Name = tex,
                });

            foreach (var matAnimJson in animJson.MaterialAnims) {
                MaterialAnimData matAnim = new MaterialAnimData();
                anim.MaterialAnimDataList.Add(matAnim);

                matAnim.Name = matAnimJson.Name;
                matAnim.PatternAnimInfos = matAnimJson.PatternAnimInfos;
                matAnim.ParamAnimInfos = matAnimJson.ParameterInfos;
                matAnim.Constants = matAnimJson.Constants;
                matAnim.BaseDataList = matAnimJson.BaseDataList.ToArray();

                foreach (var curveJson in matAnimJson.Curves)
                {
                    var target = uint.Parse(curveJson.Target);

                    var curve = CurveAnimHelper.GenerateCurve(curveJson, (uint)target, false);
                    matAnim.Curves.Add(curve);
                }
            }
        }
    }
}
