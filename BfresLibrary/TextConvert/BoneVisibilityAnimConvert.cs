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
            anim.Baked = animJson.Baked;
            anim.BindIndices = new ushort[animJson.Groups.Count];
            anim.UserData = UserDataConvert.Convert(animJson.UserData);

            foreach (var groupAnimJson in animJson.Groups) {
                var curve = groupAnimJson.Curves;
            }
        }

        static BoneAnimFlagsCurve SetCurveTarget(AnimTarget target)
        {
            BoneAnimFlagsCurve flags = (BoneAnimFlagsCurve)0;
            switch (target)
            {
                case AnimTarget.PositionX: flags |= BoneAnimFlagsCurve.TranslateX; break;
                case AnimTarget.PositionY: flags |= BoneAnimFlagsCurve.TranslateY; break;
                case AnimTarget.PositionZ: flags |= BoneAnimFlagsCurve.TranslateZ; break;
                case AnimTarget.ScaleX: flags |= BoneAnimFlagsCurve.ScaleX; break;
                case AnimTarget.ScaleY: flags |= BoneAnimFlagsCurve.ScaleY; break;
                case AnimTarget.ScaleZ: flags |= BoneAnimFlagsCurve.ScaleZ; break;
                case AnimTarget.RotateX: flags |= BoneAnimFlagsCurve.RotateX; break;
                case AnimTarget.RotateY: flags |= BoneAnimFlagsCurve.RotateY; break;
                case AnimTarget.RotateZ: flags |= BoneAnimFlagsCurve.RotateZ; break;
                case AnimTarget.RotateW: flags |= BoneAnimFlagsCurve.RotateW; break;
            }
            return flags;
        }

        public enum AnimTarget
        {
            ScaleX = 0x4,
            ScaleY = 0x8,
            ScaleZ = 0xC,
            PositionX = 0x10,
            PositionY = 0x14,
            PositionZ = 0x18,
            RotateX = 0x20,
            RotateY = 0x24,
            RotateZ = 0x28,
            RotateW = 0x2C,
        }
    }
}
