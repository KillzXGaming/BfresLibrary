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
    public class BoneVisibilityAnimConvert
    {
        internal class BoneVisibilityAnimStruct
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public int FrameCount { get; set; }
            public bool Loop { get; set; }
            public bool Baked { get; set; }

            public List<VisAnimGroupStruct> Groups { get; set; }

            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> UserData { get; set; } = new Dictionary<string, object>();
        }

        internal class VisAnimGroupStruct
        {
            public string Name { get; set; }

            public bool BaseVisible { get; set; }

            public List<CurveAnimHelper> Curves { get; set; }
        }

        public static string ToJson(VisibilityAnim anim)
        {
            BoneVisibilityAnimStruct animConv = new BoneVisibilityAnimStruct();
            animConv.Name = anim.Name;
            animConv.Path = anim.Path;
            animConv.Loop = anim.Loop;
            animConv.Baked = anim.Baked;
            animConv.FrameCount = anim.FrameCount;
            animConv.Groups = new List<VisAnimGroupStruct>();

            for (int i = 0; i < anim.Names?.Count; i++) {
                VisAnimGroupStruct groupConv = new VisAnimGroupStruct();
                groupConv.Curves = new List<CurveAnimHelper>();
                groupConv.BaseVisible = anim.BaseDataList[i];
                groupConv.Name = anim.Names[i];
                animConv.Groups.Add(groupConv);

                foreach (var curve in anim.Curves)
                {
                    if (curve.AnimDataOffset == i)
                        groupConv.Curves.Add(CurveAnimHelper.FromCurve(curve, "", false));
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

        public static VisibilityAnim FromJson(string json)
        {
            VisibilityAnim anim = new VisibilityAnim();
            FromJson(anim, json);
            return anim;
        }

        public static void FromJson(VisibilityAnim anim, string json)
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            };

            var animJson = JsonConvert.DeserializeObject<BoneVisibilityAnimStruct>(json);

            anim.Name = animJson.Name;
            anim.Baked = animJson.Baked;
            anim.Loop = animJson.Loop;
            anim.FrameCount = animJson.FrameCount;
            anim.BindIndices = new ushort[animJson.Groups.Count];
            anim.UserData = UserDataConvert.Convert(animJson.UserData);
            anim.BaseDataList = new bool[animJson.Groups.Count];
            anim.Names = new string[animJson.Groups.Count];
            anim.Curves.Clear();

            int index = 0;
            foreach (var groupAnimJson in animJson.Groups)
            {
                anim.BaseDataList[index] = groupAnimJson.BaseVisible;
                anim.Names[index] = groupAnimJson.Name;

                foreach (var curve in groupAnimJson.Curves)
                    anim.Curves.Add(ConvertCurve(curve, index));

                index++;
            }
        }

        static AnimCurve ConvertCurve(CurveAnimHelper curve, int index)
        {
            AnimCurve animCurve = new AnimCurve();
            animCurve.AnimDataOffset = (uint)index;
            animCurve.CurveType = AnimCurveType.StepBool;
            animCurve.Frames = curve.KeyFrames.Select(x => x.Key).ToArray();
            animCurve.KeyStepBoolData = curve.KeyFrames.Select(x => ToObject<BooleanKey>(x.Value).Value).ToArray();
            animCurve.Delta = 0;
            animCurve.StartFrame = 0;
            animCurve.EndFrame = curve.KeyFrames.LastOrDefault().Key;
            animCurve.KeyType = AnimCurveKeyType.Single;
            animCurve.FrameType = AnimCurveFrameType.Single;
            //Get max frame value
            float frame = curve.KeyFrames.Max(x => x.Key);
            if (frame < byte.MaxValue) animCurve.FrameType = AnimCurveFrameType.Byte;
            else if (frame < ushort.MaxValue) animCurve.FrameType = AnimCurveFrameType.Decimal10x5;

            return animCurve;
        }

        static T ToObject<T>(object obj)
        {
            if (obj is JObject) return ((JObject)obj).ToObject<T>();
            else
                return (T)obj;
        }
    }
}
