using Newtonsoft.Json;
using System.Threading;

namespace CodyzeVSPlugin.RPC
{
    abstract class RequestMessage : Message
    {
        private static long IDCounter = 0;

        protected RequestMessage(string method) : this(method, false)
        {
        }

        protected RequestMessage(string method, bool genID) : base()
        {
            this.Method = method;
            if (genID)
                this.ID = Interlocked.Increment(ref IDCounter);
        }

        [JsonProperty(PropertyName = "id")]
        public long ID;

        [JsonProperty(PropertyName = "method")]
        public string Method;
    }
}
