using System;
using System.IO;
using Newtonsoft.Json;

namespace CodyzeVSPlugin.RPC
{
    class DidChangeTextDocumentNotification : NotificationMessage
    {
        public const long MYSTERY_VERSION_NUMBER = -17;

        public class DidChangeTextDocumentParams
        {
            [JsonProperty(PropertyName = "textDocument")]
            public VersionedTextDocumentIdentifier VersionedTextDocumentIdentifier;

            [JsonProperty(PropertyName = "contentChanges")]
            public TextDocumentContentChangeEvent[] TextDocumentContentChangeEvents;

            public DidChangeTextDocumentParams(string uri, long version, string text)
            {
                this.VersionedTextDocumentIdentifier = new VersionedTextDocumentIdentifier(uri, version);
                this.TextDocumentContentChangeEvents = new TextDocumentContentChangeEvent[] { new TextDocumentContentChangeEvent(text)};
            }
        }

        public class VersionedTextDocumentIdentifier : DidCloseTextDocumentNotification.TextDocumentIdentifier
        {

            [JsonProperty(PropertyName = "version")]
            public long Version;

            public VersionedTextDocumentIdentifier(string uri, long version) : base(uri) 
            {
                this.Version = version;
            }
        }

        public class TextDocumentContentChangeEvent
        {
            /*[JsonProperty(PropertyName = "range")]
            public PublishDiagnosticsNotification.Range Range;

            [JsonProperty(PropertyName = "rangeLength")]
            public long RangeLength;*/

            [JsonProperty(PropertyName = "text")]
            public string Text;

            public TextDocumentContentChangeEvent(string text) 
            {
                this.Text = text;
            }
        }

        [JsonProperty(PropertyName = "params")]
        public DidChangeTextDocumentParams Parameters;

        public DidChangeTextDocumentNotification(string uri, long version, string text) : base("textDocument/didChange")
        {
            this.Parameters = new DidChangeTextDocumentParams(uri, version, text);
        }

    }
}
