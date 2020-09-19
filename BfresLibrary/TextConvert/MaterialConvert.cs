using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BfresLibrary.TextConvert
{
    public class MaterialConvert
    {
        internal class MaterialStruct
        {
            public string Name { get; set; }

            public bool Visible { get; set; }

            public List<string> Textures { get; set; }
            public List<Sampler> Samplers { get; set; }

            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> RenderInfo { get; set; }
            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> Parameters { get; set; }
            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> UserData { get; set; }

            public ShaderAssignStruct ShaderAssign { get; set; }

            public RenderState RenderState { get; set; }

            public byte[] VolatileFlags { get; set; }

            public MaterialStruct()
            {
                Parameters = new Dictionary<string, object>();
                RenderInfo = new Dictionary<string, object>();
                UserData = new Dictionary<string, object>();
            }
        }

        internal class ShaderAssignStruct
        {
            public string ShaderArchive { get; set; }
            public string ShaderModel { get; set; }

            public Dictionary<string, string> Options = new Dictionary<string, string>();
            public Dictionary<string, string> SamplerAssign = new Dictionary<string, string>();
            public Dictionary<string, string> AttributeAssign = new Dictionary<string, string>();
        }

        public static string ToJson(Material material)
        {
            MaterialStruct matConv = new MaterialStruct();
            matConv.Name = material.Name;
            matConv.ShaderAssign = ConvertShaderAssign(material.ShaderAssign);
            matConv.Visible = material.Flags.HasFlag(MaterialFlags.Visible);
            if (material.RenderState != null)
                matConv.RenderState = material.RenderState;

            matConv.VolatileFlags = material.VolatileFlags;
            matConv.Textures = new List<string>();
            foreach (var tex in material.TextureRefs)
                matConv.Textures.Add(tex.Name);
            matConv.Samplers = material.Samplers.Values.ToList();

            foreach (var param in material.ShaderParams.Values)
                matConv.Parameters.Add($"{param.Type}|{param.Name}", param.DataValue);

            foreach (var param in material.RenderInfos.Values)
                matConv.RenderInfo.Add($"{param.Type}|{param.Name}", param.Data);

            foreach (var param in material.UserData.Values)
                matConv.UserData.Add($"{param.Type}|{param.Name}", param.GetData());

            return JsonConvert.SerializeObject(matConv, Formatting.Indented);
        }

        public static Material FromJson(string json)
        {
            Material material = new Material();
            FromJson(material, json);
            return material;
        }

        public static void FromJson(Material mat, string json)
        {
            var matJson = JsonConvert.DeserializeObject<MaterialStruct>(json);
            mat.Name = matJson.Name;
            mat.Visible = matJson.Visible;
            mat.ShaderAssign = ConvertShaderAssign(matJson.ShaderAssign);
            mat.TextureRefs = new List<TextureRef>();
            mat.Samplers = new ResDict<Sampler>();
            mat.ShaderParams = new ResDict<ShaderParam>();
            mat.UserData = new ResDict<UserData>();
            mat.RenderInfos = new ResDict<RenderInfo>();
            mat.ShaderParamData = new byte[0];
            mat.VolatileFlags = new byte[0];

            if (matJson.RenderState != null)
                mat.RenderState = matJson.RenderState;

            foreach (var tex in matJson.Textures)
                mat.TextureRefs.Add(new TextureRef() { Name = tex });

            foreach (var sampler in matJson.Samplers)
                mat.Samplers.Add(sampler.Name, sampler);

            mat.TextureSlotArray = new long[matJson.Textures.Count];
            mat.SamplerSlotArray = new long[matJson.Textures.Count];

            mat.VolatileFlags = matJson.VolatileFlags;
            foreach (var param in matJson.Parameters)
            {
                string type = param.Key.Split('|')[0];
                string name = param.Key.Split('|')[1];

                ShaderParam shaderParam = new ShaderParam();
                shaderParam.Name = name;
                var dataType = (ShaderParamType)Enum.Parse(typeof(ShaderParamType), type);

                object value = null;
                switch (dataType)
                {
                    case ShaderParamType.Float:
                        value = Convert.ToSingle(param.Value);
                        break;
                    case ShaderParamType.UInt:
                        value = Convert.ToUInt32(param.Value);
                        break;
                    case ShaderParamType.Int:
                        value = Convert.ToInt32(param.Value);
                        break;
                    case ShaderParamType.Bool:
                        value = Convert.ToBoolean(param.Value);
                        break;
                    case ShaderParamType.Srt2D:
                        value = ((JObject)param.Value).ToObject<Srt2D>();
                        break;
                    case ShaderParamType.Srt3D:
                        value = ((JObject)param.Value).ToObject<Srt3D>();
                        break;
                    case ShaderParamType.TexSrt:
                        value = ((JObject)param.Value).ToObject<TexSrt>();
                        break;
                    case ShaderParamType.TexSrtEx:
                        value = ((JObject)param.Value).ToObject<TexSrtEx>();
                        break;
                    case ShaderParamType.Float2:
                    case ShaderParamType.Float2x2:
                    case ShaderParamType.Float2x3:
                    case ShaderParamType.Float2x4:
                    case ShaderParamType.Float3:
                    case ShaderParamType.Float3x2:
                    case ShaderParamType.Float3x3:
                    case ShaderParamType.Float3x4:
                    case ShaderParamType.Float4:
                    case ShaderParamType.Float4x2:
                    case ShaderParamType.Float4x3:
                    case ShaderParamType.Float4x4:
                        value = ((JArray)param.Value).ToObject<float[]>();
                        break;
                    case ShaderParamType.Bool2:
                    case ShaderParamType.Bool3:
                    case ShaderParamType.Bool4:
                        value = ((JArray)param.Value).ToObject<bool>();
                        break;
                    case ShaderParamType.Int2:
                    case ShaderParamType.Int3:
                    case ShaderParamType.Int4:
                        value = ((JArray)param.Value).ToObject<int[]>();
                        break;
                    case ShaderParamType.UInt2:
                    case ShaderParamType.UInt3:
                    case ShaderParamType.UInt4:
                        value = ((JArray)param.Value).ToObject<uint[]>();
                        break;
                    default:
                        value = param.Value;
                        break;
                }

                mat.SetShaderParameter(name, dataType, value);
            }

            foreach (var param in matJson.RenderInfo)
            {
                string type = param.Key.Split('|')[0];
                string name = param.Key.Split('|')[1];
                RenderInfoType dataType = (RenderInfoType)Enum.Parse(typeof(RenderInfoType), type);

                if (dataType == RenderInfoType.Single)
                    mat.SetRenderInfo(name, ((JArray)param.Value).ToObject<float[]>());
                if (dataType == RenderInfoType.Int32)
                    mat.SetRenderInfo(name, ((JArray)param.Value).ToObject<int[]>());
                if (dataType == RenderInfoType.String)
                    mat.SetRenderInfo(name, ((JArray)param.Value).ToObject<string[]>());
            }

            foreach (var param in matJson.UserData)
            {
                string type = param.Key.Split('|')[0];
                string name = param.Key.Split('|')[1];
                UserDataType dataType = (UserDataType)Enum.Parse(typeof(UserDataType), type);

                if (dataType == UserDataType.Single)
                    mat.SetUserData(name, ((JArray)param.Value).ToObject<float[]>());
                if (dataType == UserDataType.Int32)
                    mat.SetUserData(name, ((JArray)param.Value).ToObject<int[]>());
                if (dataType == UserDataType.Byte)
                    mat.SetUserData(name, ((JArray)param.Value).ToObject<byte[]>());
                if (dataType == UserDataType.String)
                    mat.SetUserData(name, ((JArray)param.Value).ToObject<string[]>());
                if (dataType == UserDataType.WString)
                    mat.SetUserData(name, ((JArray)param.Value).ToObject<string[]>(), true);
            }
        }

        private static ShaderAssignStruct ConvertShaderAssign(ShaderAssign shader)
        {
            ShaderAssignStruct shaderConv = new ShaderAssignStruct();
            shaderConv.ShaderArchive = shader.ShaderArchiveName;
            shaderConv.ShaderModel = shader.ShadingModelName;

            foreach (var param in shader.SamplerAssigns)
                shaderConv.SamplerAssign.Add(param.Key, param.Value);
            foreach (var param in shader.AttribAssigns)
                shaderConv.AttributeAssign.Add(param.Key, param.Value);
            foreach (var param in shader.ShaderOptions)
                shaderConv.Options.Add(param.Key, param.Value);

            return shaderConv;
        }

        private static ShaderAssign ConvertShaderAssign(ShaderAssignStruct shader)
        {
            ShaderAssign shaderAssign = new ShaderAssign();
            shaderAssign.ShaderArchiveName = shader.ShaderArchive;
            shaderAssign.ShadingModelName = shader.ShaderModel;

            foreach (var param in shader.SamplerAssign)
                shaderAssign.SamplerAssigns.Add(param.Key, param.Value);
            foreach (var param in shader.AttributeAssign)
                shaderAssign.AttribAssigns.Add(param.Key, param.Value);
            foreach (var param in shader.Options)
                shaderAssign.ShaderOptions.Add(param.Key, param.Value);

            return shaderAssign;
        }
    }
}
