﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Bellight.Core.Misc
{
    public class BellightJsonSerializer: ISerializer
    {
        public JsonSerializerSettings Settings { get; set; } = DefaultJsonSerializerSettings;
        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, Settings);
        }

        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, Settings);
        }

        public object DeserializeObject(string value, string typeName)
        {
            var type = Type.GetType(typeName);
            return DeserializeObject(value, type);
        }

        public string TrySerializeObject(object value)
        {
            return Try(() => SerializeObject(value));
        }

        public T TryDeserializeObject<T>(string value)
        {
            return Try(() => DeserializeObject<T>(value));
        }

        public object TryDeserializeObject(string value, Type type)
        {
            return Try(() => DeserializeObject(value, type));
        }

        public object TryDeserializeObject(string value, string typeName)
        {
            return Try(() => DeserializeObject(value, typeName));
        }

        public static JsonSerializerSettings DefaultJsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            }
        }

        private T Try<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                StaticLog.Error(ex, ex.Message);
                return default(T);
            }
        }
    }
}