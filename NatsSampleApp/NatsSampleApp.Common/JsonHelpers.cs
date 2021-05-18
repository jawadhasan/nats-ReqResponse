using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NatsSampleApp.Common
{
    public static class JsonHelpers
    {
        public static byte[] Serialize<T>(T obj) =>
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));

        public static T Deserialize<T>(byte[] data) =>
            JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
    }
}
