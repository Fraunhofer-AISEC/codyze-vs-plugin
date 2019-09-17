using Newtonsoft.Json;

namespace CodyzeVSPlugin.RPC
{
    public abstract class Message
    {
        protected Message()
        {
            RPCVersion = "2.0";
        }

        [JsonProperty(PropertyName = "jsonrpc")]
        public string RPCVersion;
    }
}
