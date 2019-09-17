using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CodyzeVSPlugin.RPC
{
    class NotificationConverter : JsonConverter
    {
        static readonly Dictionary<string, Type> MethodToType;

        static NotificationConverter()
        {
            MethodToType = new Dictionary<string, Type>()
            {
                {"textDocument/publishDiagnostics", typeof(PublishDiagnosticsNotification) }
            };
        }
        public override bool CanConvert(Type objectType)
        {
            return typeof(NotificationMessage).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var method = jObject["method"].ToString();
            var actualType = MethodToType.ContainsKey(method)? MethodToType[method] : null;
            if (actualType == null) throw new NotImplementedException("Unknown method for notification: " + method);
            if (existingValue == null || existingValue.GetType() != actualType)
            {
                var contract = serializer.ContractResolver.ResolveContract(actualType);
                existingValue = contract.DefaultCreator();
            }
            using (var subReader = jObject.CreateReader())
            {
                serializer.Populate(subReader, existingValue);
            }
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
