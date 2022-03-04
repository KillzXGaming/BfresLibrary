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
    public class SceneAnimConvert
    {
        internal class SceneAnimStuct
        {
            public string Name { get; set; }
            public string Path { get; set; }

            public bool UseDegrees { get; set; } = true;

            public List<CameraAnimStruct> CameraAnims { get; set; }
            public List<LightAnimStruct> LightAnims { get; set; }
            public List<FogAnimStruct> FogAnims { get; set; }

            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> UserData { get; set; } = new Dictionary<string, object>();
        }

        internal class CameraAnimStruct
        {
            public string Name { get; set; }

            public int FrameCount { get; set; }
            public bool Loop { get; set; }
            public bool Baked { get; set; }
            public bool EulerRotation { get; set; }
            public bool Perspective { get; set; }

            public List<CurveAnimHelper> Curves { get; set; }

            public CameraAnimData BaseData { get; set; }
        }

        internal class LightAnimStruct
        {
            public string Name { get; set; }

            public int FrameCount { get; set; }
            public bool Loop { get; set; }
            public bool Baked { get; set; }

            public bool BaseAngleAttn { get; set; }
            public bool BaseColor0 { get; set; }
            public bool BaseColor1 { get; set; }
            public bool BaseDir { get; set; }
            public bool BaseDistAttn { get; set; }
            public bool BasePos { get; set; }

            public List<CurveAnimHelper> Curves { get; set; }

            public LightAnimData BaseData { get; set; }
        }

        internal class FogAnimStruct
        {
            public string Name { get; set; }

            public int FrameCount { get; set; }
            public bool Loop { get; set; }
            public bool Baked { get; set; }

            public sbyte DistanceAttnFuncIndex { get; set; }
            public string DistanceAttnFuncName { get; set; }

            public List<CurveAnimHelper> Curves { get; set; }

            public FogAnimData BaseData { get; set; }
        }

        public static string ToJson(SceneAnim anim)
        {
            SceneAnimStuct animConv = new SceneAnimStuct();
            animConv.Name = anim.Name;
            animConv.Path = anim.Path;
            animConv.CameraAnims = new List<CameraAnimStruct>();
            animConv.LightAnims = new List<LightAnimStruct>();
            animConv.FogAnims = new List<FogAnimStruct>();

            foreach (var camAnim in anim.CameraAnims.Values) {
                CameraAnimStruct camAnimConv = new CameraAnimStruct();
                camAnimConv.Curves = new List<CurveAnimHelper>();
                camAnimConv.Name = camAnim.Name;
                camAnimConv.FrameCount = camAnim.FrameCount;
                camAnimConv.Loop = camAnim.Flags.HasFlag(CameraAnimFlags.Looping);
                camAnimConv.Baked = camAnim.Flags.HasFlag(CameraAnimFlags.BakedCurve);
                camAnimConv.BaseData = camAnim.BaseData;
                camAnimConv.EulerRotation = camAnim.Flags.HasFlag(CameraAnimFlags.EulerZXY);
                camAnimConv.Perspective = camAnim.Flags.HasFlag(CameraAnimFlags.Perspective);

                animConv.CameraAnims.Add(camAnimConv);
                foreach (var curve in camAnim.Curves) {
                    string target = ((CameraAnimDataOffset)curve.AnimDataOffset).ToString();

                    bool isDegrees = target.Contains("Rotation") && animConv.UseDegrees && camAnimConv.EulerRotation;
                    var convCurve = CurveAnimHelper.FromCurve(curve, target, isDegrees);
                    camAnimConv.Curves.Add(convCurve);
                }
            }
            foreach (var lightAnim in anim.LightAnims.Values) {
                LightAnimStruct lightAnimConv = new LightAnimStruct();
                lightAnimConv.Curves = new List<CurveAnimHelper>();
                lightAnimConv.Name = lightAnim.Name;
                lightAnimConv.FrameCount = lightAnim.FrameCount;
                lightAnimConv.Loop = lightAnim.Flags.HasFlag(LightAnimFlags.Looping);
                lightAnimConv.Baked = lightAnim.Flags.HasFlag(LightAnimFlags.BakedCurve);
                lightAnimConv.BaseAngleAttn = lightAnim.Flags.HasFlag(LightAnimFlags.BaseAngleAttn);
                lightAnimConv.BaseColor0 = lightAnim.Flags.HasFlag(LightAnimFlags.BaseColor0);
                lightAnimConv.BaseColor1 = lightAnim.Flags.HasFlag(LightAnimFlags.BaseColor1);
                lightAnimConv.BaseDir = lightAnim.Flags.HasFlag(LightAnimFlags.BaseDir);
                lightAnimConv.BaseDistAttn = lightAnim.Flags.HasFlag(LightAnimFlags.BaseDistAttn);
                lightAnimConv.BasePos = lightAnim.Flags.HasFlag(LightAnimFlags.BasePos);
                if (lightAnim.Flags.HasFlag(LightAnimFlags.BaseEnable))
                    lightAnimConv.BaseData = lightAnim.BaseData;

                animConv.LightAnims.Add(lightAnimConv);
                foreach (var curve in lightAnim.Curves)
                {
                    string target = ((LightAnimDataOffset)curve.AnimDataOffset).ToString();
                    var convCurve = CurveAnimHelper.FromCurve(curve, target, false);
                    lightAnimConv.Curves.Add(convCurve);
                }
            }
            foreach (var fogAnim in anim.FogAnims.Values) {
                FogAnimStruct fogAnimConv = new FogAnimStruct();
                fogAnimConv.Curves = new List<CurveAnimHelper>();
                fogAnimConv.Name = fogAnim.Name;
                fogAnimConv.FrameCount = fogAnim.FrameCount;
                fogAnimConv.Loop = fogAnim.Flags.HasFlag(FogAnimFlags.Looping);
                fogAnimConv.Baked = fogAnim.Flags.HasFlag(FogAnimFlags.BakedCurve);
                fogAnimConv.BaseData = fogAnim.BaseData;
                fogAnimConv.DistanceAttnFuncIndex = fogAnim.DistanceAttnFuncIndex;
                fogAnimConv.DistanceAttnFuncName = fogAnim.DistanceAttnFuncName;

                animConv.FogAnims.Add(fogAnimConv);
                foreach (var curve in fogAnim.Curves)
                {
                    string target = ((FogAnimDataOffset)curve.AnimDataOffset).ToString();
                    var convCurve = CurveAnimHelper.FromCurve(curve, target, false);
                    fogAnimConv.Curves.Add(convCurve);
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

        public static SceneAnim FromJson(string json)
        {
            SceneAnim anim = new SceneAnim();
            FromJson(anim, json);
            return anim;
        }

        public static void FromJson(SceneAnim anim, string json)
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            };

            var animJson = JsonConvert.DeserializeObject<SceneAnimStuct>(json);

            anim.Name = animJson.Name;
            anim.CameraAnims = new ResDict<CameraAnim>();
            anim.LightAnims = new ResDict<LightAnim>();
            anim.FogAnims = new ResDict<FogAnim>();
            anim.UserData = UserDataConvert.Convert(animJson.UserData);

            foreach (var camAnimJson in animJson.CameraAnims) {
                CameraAnim camAnim = new CameraAnim();
                camAnim.Flags = 0;

                anim.CameraAnims.Add(camAnimJson.Name, camAnim);

                if (camAnimJson.Loop)          camAnim.Flags |= CameraAnimFlags.Looping;
                if (camAnimJson.Baked)         camAnim.Flags |= CameraAnimFlags.BakedCurve;
                if (camAnimJson.Perspective)   camAnim.Flags |= CameraAnimFlags.Perspective;
                if (camAnimJson.EulerRotation) camAnim.Flags |= CameraAnimFlags.EulerZXY;

                camAnim.Name = camAnimJson.Name;
                camAnim.FrameCount = camAnimJson.FrameCount;
                camAnim.BaseData = camAnimJson.BaseData;
                foreach (var curveJson in camAnimJson.Curves)
                {
                    var target = (CameraAnimDataOffset)Enum.Parse(typeof(CameraAnimDataOffset), curveJson.Target);

                    var curve = CurveAnimHelper.GenerateCurve(curveJson, (uint)target, false);
                    camAnim.Curves.Add(curve);
                }
            }
            foreach (var lightAnimJson in animJson.LightAnims) {
                LightAnim lightAnim = new LightAnim();
                lightAnim.Flags = 0;

                anim.LightAnims.Add(lightAnimJson.Name, lightAnim);

                if (lightAnimJson.Loop) lightAnim.Flags |= LightAnimFlags.Looping;
                if (lightAnimJson.Baked) lightAnim.Flags |= LightAnimFlags.BakedCurve;
                if (lightAnimJson.BaseAngleAttn) lightAnim.Flags |= LightAnimFlags.BaseAngleAttn;
                if (lightAnimJson.BaseColor0) lightAnim.Flags |= LightAnimFlags.BaseColor0;
                if (lightAnimJson.BaseColor1) lightAnim.Flags |= LightAnimFlags.BaseColor1;
                if (lightAnimJson.BaseDir) lightAnim.Flags |= LightAnimFlags.BaseDir;
                if (lightAnimJson.BaseDistAttn) lightAnim.Flags |= LightAnimFlags.BaseDistAttn;
                if (lightAnimJson.BasePos) lightAnim.Flags |= LightAnimFlags.BasePos;

                lightAnim.Name = lightAnimJson.Name;
                lightAnim.FrameCount = lightAnimJson.FrameCount;
                lightAnim.BaseData = lightAnimJson.BaseData;

                if (lightAnim.BaseData.Enable == 1)
                    lightAnim.Flags |= LightAnimFlags.BaseEnable;

                foreach (var curveJson in lightAnimJson.Curves)
                {
                    var target = (CameraAnimDataOffset)Enum.Parse(typeof(CameraAnimDataOffset), curveJson.Target);

                    var curve = CurveAnimHelper.GenerateCurve(curveJson, (uint)target, false);
                    lightAnim.Curves.Add(curve);
                }
            }
            foreach (var fogAnimJson in animJson.FogAnims) {
                FogAnim fogAnim = new FogAnim();
                fogAnim.Flags = 0;

                anim.CameraAnims.Add(fogAnimJson.Name, fogAnim);

                if (fogAnimJson.Loop) fogAnim.Flags |= FogAnimFlags.Looping;
                if (fogAnimJson.Baked) fogAnim.Flags |= FogAnimFlags.BakedCurve;

                fogAnim.Name = fogAnimJson.Name;
                fogAnim.FrameCount = fogAnimJson.FrameCount;
                fogAnim.DistanceAttnFuncIndex = fogAnimJson.DistanceAttnFuncIndex;
                fogAnim.DistanceAttnFuncName = fogAnimJson.DistanceAttnFuncName;
                fogAnim.BaseData = fogAnimJson.BaseData;

                foreach (var curveJson in fogAnimJson.Curves)
                {
                    var target = (CameraAnimDataOffset)Enum.Parse(typeof(CameraAnimDataOffset), curveJson.Target);

                    var curve = CurveAnimHelper.GenerateCurve(curveJson, (uint)target, false);
                    fogAnim.Curves.Add(curve);
                }
            }
        }
    }
}
