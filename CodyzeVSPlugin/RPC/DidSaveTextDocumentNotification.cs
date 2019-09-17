using System;
using System.IO;
using Newtonsoft.Json;

namespace CodyzeVSPlugin.RPC
{
    class DidSaveTextDocumentNotification : NotificationMessage
    {
        public class DidSaveTextDocumentParams
        {
            [JsonProperty(PropertyName = "textDocument")]
            public DidCloseTextDocumentNotification.TextDocumentIdentifier TextDocumentIdentifier;
            //public DidChangeTextDocumentNotification.VersionedTextDocumentIdentifier VersionedTextDocumentIdentifier;

            [JsonProperty(PropertyName = "text")]
            public string Text;

            public DidSaveTextDocumentParams(string uri, string text)
            {
                this.TextDocumentIdentifier = new DidCloseTextDocumentNotification.TextDocumentIdentifier(uri);
                //this.VersionedTextDocumentIdentifier = new DidChangeTextDocumentNotification.VersionedTextDocumentIdentifier(uri,version);
                this.Text = text;
            }
        }

        
        [JsonProperty(PropertyName = "params")]
        public DidSaveTextDocumentParams Parameters;

        public DidSaveTextDocumentNotification(string uri, string text) : base("textDocument/didSave")
        { 
            this.Parameters = new DidSaveTextDocumentParams(uri, text);
        }

    }
}
