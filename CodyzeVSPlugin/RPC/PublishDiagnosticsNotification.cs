using Newtonsoft.Json;
using System.Collections.Generic;

namespace CodyzeVSPlugin.RPC
{
    public class PublishDiagnosticsNotification : NotificationMessage
    {
        public class PublishDiagnosticsParams
        {
            [JsonProperty(PropertyName = "uri")]
            public string Uri;

            [JsonProperty(PropertyName = "diagnostics")]
            public List<Diagnostic> Diagnostics;
        }

        public class Diagnostic
        {
            [JsonProperty(PropertyName = "range")]
            public Range Range;

            [JsonProperty(PropertyName = "severity")]
            public int Severity;

            [JsonProperty(PropertyName = "code")]
            public string Code;

            [JsonProperty(PropertyName = "source")]
            public string Source;

            [JsonProperty(PropertyName = "message")]
            public string Message;

            [JsonProperty(PropertyName = "relatedInformation")]
            public List<DiagnosticRelatedInformation> RelatedInformation;
        }

        public class Range
        {
            [JsonProperty(PropertyName = "start")]
            public Position Start;

            [JsonProperty(PropertyName = "end")]
            public Position End;

            public override string ToString()
            {
                return "From " + Start + " to " + End;
            }
        }

        public class Position
        {
            [JsonProperty(PropertyName = "line")]
            public int Line;

            [JsonProperty(PropertyName = "character")]
            public int Character;

            public override string ToString()
            {
                return Line + ":" + Character;
            }
        }

        public class DiagnosticRelatedInformation
        {
            [JsonProperty(PropertyName = "location")]
            public Location Location;

            [JsonProperty(PropertyName = "message")]
            public string Message;
        }

        public class Location
        {
            [JsonProperty(PropertyName = "uri")]
            public string Uri;

            [JsonProperty(PropertyName = "range")]
            public Range Range;
        }

        [JsonProperty(PropertyName = "params")]
        public PublishDiagnosticsParams Parameters;

        public PublishDiagnosticsNotification() : base("textDocument/publishDiagnostics")
        {
        }
    }
}
