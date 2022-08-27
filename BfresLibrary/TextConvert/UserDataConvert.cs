using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace BfresLibrary.TextConvert
{
    public class UserDataConvert
    {
        public static ResDict<UserData> Convert(Dictionary<string, object> userData)
        {
            ResDict<UserData> userDataDict = new ResDict<UserData>();

            foreach (var param in userData)
            {
                UserData usd = new UserData();

                string type = param.Key.Split('|')[0];
                string name = param.Key.Split('|')[1];
                UserDataType dataType = (UserDataType)Enum.Parse(typeof(UserDataType), type);

                if (dataType == UserDataType.Single)
                    usd = SetUserData(name, ((JArray)param.Value).ToObject<float[]>());
                if (dataType == UserDataType.Int32)
                    usd = SetUserData(name, ((JArray)param.Value).ToObject<int[]>());
                if (dataType == UserDataType.Byte)
                    usd = SetUserData(name, ((JArray)param.Value).ToObject<byte[]>());
                if (dataType == UserDataType.String)
                    usd = SetUserData(name, ((JArray)param.Value).ToObject<string[]>());
                if (dataType == UserDataType.WString)
                    usd = SetUserData(name, ((JArray)param.Value).ToObject<string[]>(), true);

                userDataDict.Add(usd.Name, usd);
            }
            return userDataDict;
        }

        static UserData SetUserData(string name, object value, bool isUnicode = false)
        {
            var userData = new UserData();
            userData.Name = name;
            if (value is int) userData.SetValue(new int[1] { (int)value });
            else if (value is float) userData.SetValue(new float[1] { (float)value });
            else if (value is string) userData.SetValue(new string[1] { (string)value }, isUnicode);
            else if (value is int[]) userData.SetValue((int[])value);
            else if (value is float[]) userData.SetValue((float[])value);
            else if (value is string[]) userData.SetValue((string[])value, isUnicode);
            return userData;
        }
    }
}
