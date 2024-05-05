using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using BfresLibrary.TextConvert;

namespace BfresLibrary
{
    public class CurveAnimHelper
    {
        public string Target { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AnimCurveType Interpolation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AnimCurveFrameType FrameType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AnimCurveKeyType KeyType { get; set; }

        public string WrapMode { get; set; }

        public float Scale { get; set; }
        public float Offset { get; set; }

        [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
        public Dictionary<float, object> KeyFrames { get; set; }

        public static CurveAnimHelper FromCurve(AnimCurve curve, string target, bool useDegrees)
        {
            var convCurve = new CurveAnimHelper();
            convCurve.KeyFrames = new Dictionary<float, object>();
            convCurve.Target = target;
            convCurve.Scale = curve.Scale;
            convCurve.Offset = curve.Offset;
            convCurve.WrapMode = $"{curve.PreWrap}, {curve.PostWrap}";
            convCurve.Interpolation = curve.CurveType;
            convCurve.FrameType = curve.FrameType;
            convCurve.KeyType = curve.KeyType;

            float valueScale = curve.Scale > 0 ? curve.Scale : 1;
            for (int i = 0; i < curve.Frames.Length; i++)
            {
                var frame = curve.Frames[i];
                switch (curve.CurveType)
                {
                    case AnimCurveType.Cubic:
                        {
                            var coef0 = curve.Keys[i, 0] * valueScale + curve.Offset;
                            var slopes = GetSlopes(curve, i);
                            if (useDegrees)
                            {
                                coef0 *= Rad2Deg;
                                slopes[0] *= Rad2Deg;
                                slopes[1] *= Rad2Deg;
                            }

                            convCurve.KeyFrames.Add(frame, new HermiteKey()
                            {
                                Value = coef0,
                                In = slopes[0],
                                Out = slopes[1],
                            });
                        }
                        break;
                    case AnimCurveType.StepBool:
                        convCurve.KeyFrames.Add(frame, new BooleanKey()
                        {
                            Value = curve.KeyStepBoolData[i],
                        });
                        break;
                    case AnimCurveType.StepInt:
                        convCurve.KeyFrames.Add(frame, new KeyFrame()
                        {
                            Value = (int)curve.Keys[i, 0] + (int)curve.Offset
                        });
                        break;
                    case AnimCurveType.Linear:
                        {
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            if (useDegrees)
                                value *= Rad2Deg;

                            convCurve.KeyFrames.Add(frame, new LinearKeyFrame()
                            {
                                Value = value,
                                Delta = curve.Keys[i, 1] * valueScale,
                            });
                        }
                        break;
                    default:
                        {
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            if (useDegrees)
                                value *= Rad2Deg;

                            convCurve.KeyFrames.Add(frame, new KeyFrame()
                            {
                                Value = value
                            });
                        }
                        break;
                }
            }
            return convCurve;
        }

        public static AnimCurve GenerateCurve(CurveAnimHelper curveJson, uint target, bool isDegrees)
        {
            AnimCurve curve = new AnimCurve();
            curve.Offset = curveJson.Offset;
            curve.Scale = curveJson.Scale;
            curve.CurveType = curveJson.Interpolation;
            curve.FrameType = curveJson.FrameType;
            curve.KeyType = curveJson.KeyType;
            curve.AnimDataOffset = target;

            var first = curveJson.KeyFrames.First();
            var last = curveJson.KeyFrames.Last();
            curve.EndFrame = last.Key;
            curve.StartFrame = first.Key;

            var keys = curveJson.KeyFrames.Values.ToList();
            var frames = curveJson.KeyFrames.Keys.ToList();
            curve.Frames = frames.ToArray();
            curve.Keys = new float[keys.Count, 1];
            if (curve.CurveType == AnimCurveType.Cubic) curve.Keys = new float[keys.Count, 4];
            if (curve.CurveType == AnimCurveType.Linear) curve.Keys = new float[keys.Count, 2];

            for (int i = 0; i < keys.Count; i++)
            {
                switch (curve.CurveType)
                {
                    case AnimCurveType.Cubic:
                        var hermiteKey = ToObject<HermiteKey>(keys[i]);

                        float time = 0;
                        float value = hermiteKey.Value;
                        float outSlope = hermiteKey.Out;
                        float nextValue = 0;
                        float nextInSlope = 0;
                        if (i < keys.Count - 1)
                        {
                            var nextKey = ToObject<HermiteKey>(keys[i + 1]);
                            var nextFrame = frames[i + 1];

                            nextValue = nextKey.Value;
                            nextInSlope = nextKey.In;
                            time = nextFrame - frames[i];
                        }
                        if (isDegrees)
                        {
                            value *= Deg2Rad;
                            nextValue *= Deg2Rad;
                            nextInSlope *= Deg2Rad;
                            outSlope *= Deg2Rad;
                        }

                        float[] coefs = HermiteToCubicKey(
                            value, nextValue,
                            outSlope * time, nextInSlope * time);

                        curve.Keys[i, 0] = coefs[0];
                        if (time != 0)
                        {
                            curve.Keys[i, 1] = coefs[1];
                            curve.Keys[i, 2] = coefs[2];
                            curve.Keys[i, 3] = coefs[3];
                        }
                        break;
                    case AnimCurveType.StepBool:
                        var booleanKey = ToObject<BooleanKey>(keys[i]);
                        curve.KeyStepBoolData[i] = booleanKey.Value;
                        break;
                    case AnimCurveType.Linear:
                        if (keys[i] is LinearKeyFrame)
                        {
                            var linearKey = ToObject<LinearKeyFrame>(keys[i]);
                            float linearValue = linearKey.Value;
                            if (isDegrees)
                            {
                                linearValue *= Deg2Rad;
                            }
                            curve.Keys[i, 0] = linearValue;
                            curve.Keys[i, 1] = linearKey.Delta;
                        }
                        else
                        {
                            var linearKey = ToObject<KeyFrame>(keys[i]);
                            float linearValue = linearKey.Value;
                            if (isDegrees)
                            {
                                linearValue *= Deg2Rad;
                            }
                            curve.Keys[i, 0] = linearValue;
                            curve.Keys[i, 1] = 0;
                        }
                        break;
                    case AnimCurveType.StepInt:
                        var stepKey = ToObject<KeyFrame>(keys[i]);
                        curve.Keys[i, 0] = stepKey.Value;
                        break;
                }
            }

            if (curve.Keys.Length >= 2)
            {
                var lastKey = curve.Keys[keys.Count - 1, 0];
                var firstKey = curve.Keys[0, 0];

                curve.Delta = lastKey - firstKey;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                curve.Keys[i, 0] -= curve.Offset;

                //Apply scale for cubic and linear curves only
                if (curve.CurveType == AnimCurveType.Cubic)
                {
                    if (curve.Scale != 0)
                    {
                        curve.Keys[i, 0] /= curve.Scale;
                        curve.Keys[i, 1] /= curve.Scale;
                        curve.Keys[i, 2] /= curve.Scale;
                        curve.Keys[i, 3] /= curve.Scale;
                    }
                }
                else if (curve.CurveType == AnimCurveType.Linear)
                {
                    if (curve.Scale != 0)
                    {
                        curve.Keys[i, 0] /= curve.Scale;
                        curve.Keys[i, 1] /= curve.Scale;
                    }
                }
            }

            return curve;
        }

        static T ToObject<T>(object obj)
        {
            if (obj is JObject) return ((JObject)obj).ToObject<T>();
            else
                return (T)obj;
        }

        public static float Rad2Deg = (float)(360 / (System.Math.PI * 2));
        public static float Deg2Rad = (float)(System.Math.PI * 2) / 360;

        public static float[] HermiteToCubicKey(float p0, float p1, float s0, float s1)
        {
            float[] coefs = new float[4];
            coefs[3] = (p0 * 2) + (p1 * -2) + (s0 * 1) + (s1 * 1);
            coefs[2] = (p0 * -3) + (p1 * 3) + (s0 * -2) + (s1 * -1);
            coefs[1] = (p0 * 0) + (p1 * 0) + (s0 * 1) + (s1 * 0);
            coefs[0] = (p0 * 1) + (p1 * 0) + (s0 * 0) + (s1 * 0);
            return coefs;
        }

        //Method to extract the slopes from a cubic curve
        //Need to get the time, delta, out and next in slope values
        public static float[] GetSlopes(AnimCurve curve, float index)
        {
            float[] slopes = new float[2];
            if (curve.CurveType == AnimCurveType.Cubic)
            {
                float InSlope = 0;
                float OutSlope = 0;
                for (int i = 0; i < curve.Frames.Length; i++)
                {
                    var coef0 = curve.Keys[i, 0] * curve.Scale + curve.Offset;
                    var coef1 = curve.Keys[i, 1] * curve.Scale;
                    var coef2 = curve.Keys[i, 2] * curve.Scale;
                    var coef3 = curve.Keys[i, 3] * curve.Scale;
                    float time = 0;
                    float delta = 0;
                    if (i < curve.Frames.Length - 1)
                    {
                        var nextValue = curve.Keys[i + 1, 0] * curve.Scale + curve.Offset;
                        delta = nextValue - coef0;
                        time = curve.Frames[i + 1] - curve.Frames[i];
                    }

                    var slopeData = GetCubicSlopes(time, delta,
                        new float[4] { coef0, coef1, coef2, coef3, });

                    if (index == i)
                    {
                        OutSlope = slopeData[1];
                        return new float[2] { InSlope, OutSlope };
                    }

                    //The previous inslope is used
                    InSlope = slopeData[0];
                }
            }

            return slopes;
        }

        public static float[] GetCubicSlopes(float time, float delta, float[] coef)
        {
            float outSlope = coef[1] / time;
            float param = coef[3] - (-2 * delta);
            float inSlope = param / time - outSlope;
            return new float[2] { inSlope, coef[1] == 0 ? 0 : outSlope };
        }
    }

    public class HermiteKey
    {
        public float Value { get; set; }
        public float In { get; set; }
        public float Out { get; set; }
    }

    public class BooleanKey
    {
        public bool Value { get; set; }
    }

    public class LinearKeyFrame
    {
        public float Value { get; set; }
        public float Delta { get; set; }
    }

    public class KeyFrame
    {
        public float Value { get; set; }
    }
}
