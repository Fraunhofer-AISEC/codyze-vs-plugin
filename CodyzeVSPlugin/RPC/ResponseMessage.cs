using Newtonsoft.Json;
using System.Collections.Generic;

namespace CodyzeVSPlugin.RPC
{
    class ResponseMessage : Message
    {
        protected ResponseMessage() : base()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public long ID;

        [JsonProperty(PropertyName = "result")]
        public Dictionary<string, object> Result;

        [JsonProperty(PropertyName = "error")]
        public Dictionary<string, object> Error;
    }
}
