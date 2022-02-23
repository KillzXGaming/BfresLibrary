﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Syroot.Maths;

namespace BfresLibrary.TextConvert
{
    public class SkeletalAnimConvert
    {
        internal class SkeletalAnimStruct
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public int FrameCount { get; set; }
            public bool Loop { get; set; }
            public bool Baked { get; set; }
            public bool UseDegrees { get; set; } = true;

            [JsonConverter(typeof(StringEnumConverter))]
            public SkeletalAnimFlagsScale FlagsScale { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public SkeletalAnimFlagsRotate FlagsRotate { get; set; }

            public List<BoneAnimStruct> BoneAnims { get; set; }

            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> UserData { get; set; } = new Dictionary<string, object>();
        }

        internal class BoneAnimStruct
        {
            public string Name { get; set; }

            public bool SegmentScaleCompensate { get; set; }

            public bool UseBaseTranslation { get; set; }
            public bool UseBaseRotation { get; set; }
            public bool UseBaseScale { get; set; }

            public List<CurveAnimStruct> Curves { get; set; }

            public BaseData BaseData { get; set; }
        }

        internal struct BaseData
        {
            public uint Flags;

            public Vector3F Scale;

            public Vector3F Translate;

            public Vector4F Rotate;
        }

        public static string ToJson(SkeletalAnim anim)
        {
            SkeletalAnimStruct animConv = new SkeletalAnimStruct();
            animConv.Name = anim.Name;
            animConv.Path = anim.Path;
            animConv.Loop = anim.Loop;
            animConv.Baked = anim.Baked;
            animConv.FrameCount = anim.FrameCount;
            animConv.FlagsScale = anim.FlagsScale;
            animConv.FlagsRotate = anim.FlagsRotate;
            animConv.BoneAnims = new List<BoneAnimStruct>();

            foreach (var boneAnim in anim.BoneAnims) {
                BoneAnimStruct boneAnimConv = new BoneAnimStruct();
                boneAnimConv.Curves = new List<CurveAnimStruct>();
                boneAnimConv.Name = boneAnim.Name;
                Vector4F rotation = boneAnim.BaseData.Rotate;
                if (animConv.UseDegrees && animConv.FlagsRotate == SkeletalAnimFlagsRotate.EulerXYZ)
                {
                    rotation = new Vector4F(
                        rotation.X * CurveConvert.Rad2Deg,
                        rotation.Y * CurveConvert.Rad2Deg, 
                        rotation.Z * CurveConvert.Rad2Deg,
                        rotation.W);
                }

                boneAnimConv.BaseData = new BaseData()
                {
                    Flags = boneAnim.BaseData.Flags,
                    Rotate = rotation,
                    Translate = boneAnim.BaseData.Translate,
                    Scale = boneAnim.BaseData.Scale,
                };
                boneAnimConv.SegmentScaleCompensate = boneAnim.ApplySegmentScaleCompensate;
                boneAnimConv.UseBaseTranslation = boneAnim.FlagsBase.HasFlag(BoneAnimFlagsBase.Translate);
                boneAnimConv.UseBaseRotation = boneAnim.FlagsBase.HasFlag(BoneAnimFlagsBase.Rotate);
                boneAnimConv.UseBaseScale = boneAnim.FlagsBase.HasFlag(BoneAnimFlagsBase.Scale);
                animConv.BoneAnims.Add(boneAnimConv);

                foreach (var curve in boneAnim.Curves) {
                    string target = ((AnimTarget)curve.AnimDataOffset).ToString(); 
                    var convCurve = CurveConvert.FromCurve(curve, target,
                        target.Contains("Rotate") && animConv.UseDegrees);
                    boneAnimConv.Curves.Add(convCurve);
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

        public static SkeletalAnim FromJson(string json)
        {
            SkeletalAnim anim = new SkeletalAnim();
            FromJson(anim, json);
            return anim;
        }

        public static void FromJson(SkeletalAnim anim, string json)
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            };

            var animJson = JsonConvert.DeserializeObject<SkeletalAnimStruct>(json);

            anim.Name = animJson.Name;
            anim.Baked = animJson.Baked;
            anim.Loop = animJson.Loop;
            anim.FrameCount = animJson.FrameCount;
            anim.Baked = animJson.Baked;
            anim.FlagsRotate = animJson.FlagsRotate;
            anim.FlagsScale = animJson.FlagsScale;
            anim.BoneAnims = new List<BoneAnim>();
            anim.BindIndices = new ushort[animJson.BoneAnims.Count];
            anim.UserData = UserDataConvert.Convert(animJson.UserData);

            foreach (var boneAnimJson in animJson.BoneAnims) {
                BoneAnim boneAnim = new BoneAnim();
                anim.BoneAnims.Add(boneAnim);

                //Always these indices
                boneAnim.Name = boneAnimJson.Name;
                boneAnim.BeginRotate = 3;
                boneAnim.BeginTranslate = 6;
                boneAnim.BeginBaseTranslate = 7;
                Vector4F rotation = boneAnim.BaseData.Rotate;
                if (animJson.UseDegrees && animJson.FlagsRotate == SkeletalAnimFlagsRotate.EulerXYZ)
                {
                    rotation = new Vector4F(
                        rotation.X * CurveConvert.Deg2Rad,
                        rotation.Y * CurveConvert.Deg2Rad,
                        rotation.Z * CurveConvert.Deg2Rad,
                        rotation.W);
                }
                boneAnim.BaseData = new BoneAnimData()
                {
                    Flags = boneAnimJson.BaseData.Flags,
                    Rotate = rotation,
                    Translate = boneAnimJson.BaseData.Translate,
                    Scale = boneAnimJson.BaseData.Scale,
                };

                boneAnim.FlagsTransform |= BoneAnimFlagsTransform.Identity;
                if (boneAnimJson.UseBaseTranslation)
                    boneAnim.FlagsBase |= BoneAnimFlagsBase.Translate;
                if (boneAnimJson.UseBaseRotation)
                    boneAnim.FlagsBase |= BoneAnimFlagsBase.Rotate;
                if (boneAnimJson.UseBaseScale)
                    boneAnim.FlagsBase |= BoneAnimFlagsBase.Scale;
                foreach (var curveJson in boneAnimJson.Curves)
                {
                    var target = (AnimTarget)Enum.Parse(typeof(AnimTarget), curveJson.Target);

                    var curve = CurveConvert.GenerateCurve(curveJson, (uint)target, 
                        curveJson.Target.Contains("Rotate") && animJson.UseDegrees);
                    boneAnim.Curves.Add(curve);
                    boneAnim.FlagsCurve = SetCurveTarget(target);
                }
                boneAnim.CalculateTransformFlags();
                boneAnim.ApplySegmentScaleCompensate = boneAnimJson.SegmentScaleCompensate;
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
