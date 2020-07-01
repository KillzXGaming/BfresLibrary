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

        public static string ParseFormat(Material material)
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

        public class NoFormattingConverter : JsonConverter
        {
            [ThreadStatic]
            static bool cannotWrite;

            // Disables the converter in a thread-safe manner.
            bool CannotWrite { get { return cannotWrite; } set { cannotWrite = value; } }

            public override bool CanWrite { get { return !CannotWrite; } }

            public override bool CanRead { get { return false; } }

            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException(); // Should be applied as a property rather than included in the JsonSerializerSettings.Converters list.
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                using (new PushValue<bool>(true, () => CannotWrite, val => CannotWrite = val))
                using (new PushValue<Formatting>(Formatting.None, () => writer.Formatting, val => writer.Formatting = val))
                {
                    serializer.Serialize(writer, value);
                }
            }
        }

        public struct PushValue<T> : IDisposable
        {
            Action<T> setValue;
            T oldValue;

            public PushValue(T value, Func<T> getValue, Action<T> setValue)
            {
                if (getValue == null || setValue == null)
                    throw new ArgumentNullException();
                this.setValue = setValue;
                this.oldValue = getValue();
                setValue(value);
            }

            #region IDisposable Members

            // By using a disposable struct we avoid the overhead of allocating and freeing an instance of a finalizable class.
            public void Dispose()
            {
                if (setValue != null)
                    setValue(oldValue);
            }

            #endregion
        }
    }
}
