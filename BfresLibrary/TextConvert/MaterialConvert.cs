using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Syroot.NintenTools.Bfres.TextConvert
{
    public class MaterialConvert
    {
        public class MaterialStruct
        {
            public string Name { get; set; }

            public bool Visible { get; set; }

            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> RenderInfo { get; set; }
            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> Parameters { get; set; }
            [JsonProperty(ItemConverterType = typeof(NoFormattingConverter))]
            public Dictionary<string, object> UserData { get; set; }

            public ShaderAssignStruct ShaderAssign { get; set; }

            public RenderState RenderState { get; set; }

            public MaterialStruct()
            {
                Parameters = new Dictionary<string, object>();
                RenderInfo = new Dictionary<string, object>();
                UserData = new Dictionary<string, object>();
            }
        }

        public class ShaderAssignStruct
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

            foreach (var param in material.ShaderParams.Values)
                matConv.Parameters.Add(param.Name, param.DataValue);

            foreach (var param in material.RenderInfos.Values)
                matConv.RenderInfo.Add(param.Name, param.Data);

            foreach (var param in material.UserData.Values)
                matConv.UserData.Add(param.Name, param.GetData());

            return JsonConvert.SerializeObject(matConv, Formatting.Indented);
        }

        public static Material FromJson(string json)
        {
            var matJson = JsonConvert.DeserializeObject<MaterialStruct>(json);
            Material mat = new Material();
            mat.Name = matJson.Name;
            mat.Visible = matJson.Visible;
            mat.ShaderAssign = ConvertShaderAssign(matJson.ShaderAssign);
            if (matJson.RenderState != null)
                mat.RenderState = matJson.RenderState;

            foreach (var param in matJson.Parameters)
            {

            }

            foreach (var param in matJson.RenderInfo)
            {
            }

            foreach (var param in matJson.UserData)
            {

            }

            return mat;
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
