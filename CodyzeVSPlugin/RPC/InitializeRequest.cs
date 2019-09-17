using Newtonsoft.Json;
using System;

namespace CodyzeVSPlugin.RPC
{
    class InitializeRequest : RequestMessage
    {
        public class InitializeParams
        {
            [JsonProperty(PropertyName = "processId")]
            public int? ProcessID;
            [JsonProperty(PropertyName = "rootUri")]
            public string RootUri;

            public InitializeParams(int? processId, string rootUri)
            {
                this.ProcessID = processId;
                this.RootUri = rootUri;
            }
        }
        [JsonProperty(PropertyName = "params")]
        public InitializeParams Parameters;
        public InitializeRequest(int? processId = null, string rootUri = null) : base("initialize", true)
        {
            this.Parameters = new InitializeParams(processId, rootUri);
        }
    }
}
