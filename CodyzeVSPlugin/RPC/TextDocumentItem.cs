using Newtonsoft.Json;

namespace CodyzeVSPlugin.RPC
{
    class TextDocumentItem
    {
        [JsonProperty(PropertyName = "uri")]
        public string Uri;
        [JsonProperty(PropertyName = "languageId")]
        public string LanguageID;
        [JsonProperty(PropertyName = "version")]
        public long Version;
        [JsonProperty(PropertyName = "text")]
        public string Text;

        public TextDocumentItem(string uri, string languageID, long version, string text)
        {
            this.Uri = uri;
            this.LanguageID = languageID;
            this.Version = version;
            this.Text = text;
        }
    }
}
