using System;
using System.IO;
using Newtonsoft.Json;

namespace CodyzeVSPlugin.RPC
{
    class DidOpenTextDocumentNotification : NotificationMessage
    {
        public class DidOpenTextDocumentParams
        {
            [JsonProperty(PropertyName = "textDocument")]
            public TextDocumentItem TextDocument;

            public DidOpenTextDocumentParams(TextDocumentItem textDocument)
            {
                this.TextDocument = textDocument;
            }
        }

        [JsonProperty(PropertyName = "params")]
        public DidOpenTextDocumentParams Parameters;

        public DidOpenTextDocumentNotification(string documentPath, string content = null) : base("textDocument/didOpen")
        {
            if (content == null) content = File.ReadAllText(documentPath);
            this.Parameters = new DidOpenTextDocumentParams(new TextDocumentItem(new Uri(documentPath).AbsoluteUri, "cpp", -1, content));
        }

    }
}
