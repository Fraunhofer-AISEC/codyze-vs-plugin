using Newtonsoft.Json;
using System.Collections.Generic;

namespace CodyzeVSPlugin.RPC
{
    class InitializedNotification : NotificationMessage
    {
        [JsonProperty(PropertyName = "params")]
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();

        public InitializedNotification() : base("initialized")
        {
        }
    }
}
