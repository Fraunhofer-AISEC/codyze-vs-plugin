using Newtonsoft.Json;
using System.Collections.Generic;

namespace CodyzeVSPlugin.RPC
{
    public abstract class NotificationMessage : Message
    {
        protected NotificationMessage(string method)
        {
            this.Method = method;
        }
        [JsonProperty(PropertyName = "method")]
        public string Method;
    }
}
