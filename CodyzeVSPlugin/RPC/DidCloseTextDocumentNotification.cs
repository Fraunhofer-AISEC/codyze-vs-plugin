using System;
using System.IO;
using Newtonsoft.Json;

namespace CodyzeVSPlugin.RPC
{
    class DidCloseTextDocumentNotification : NotificationMessage
    {
        public class DidCloseTextDocumentParams
        {
            [JsonProperty(PropertyName = "textDocument")]
            public TextDocumentIdentifier TextDocumentIdentifier;

            public DidCloseTextDocumentParams(string uri)
            {
                this.TextDocumentIdentifier = new TextDocumentIdentifier(uri);
            }
        }

        public class TextDocumentIdentifier
        {
            [JsonProperty(PropertyName = "uri")]
            public string Uri;
            public TextDocumentIdentifier(string uri)
            {
                this.Uri = uri;
            }
        }

        [JsonProperty(PropertyName = "params")]
        public DidCloseTextDocumentParams Parameters;

        public DidCloseTextDocumentNotification(string uri) : base("textDocument/didClose")
        {
            this.Parameters = new DidCloseTextDocumentParams(uri);
        }

    }
}
