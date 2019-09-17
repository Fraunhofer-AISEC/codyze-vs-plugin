using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using CodyzeVSPlugin.RPC;
using CodyzeVSPlugin.Highlighting;

namespace CodyzeVSPlugin
{
    public class HighlightWordTagger : ITagger<TextMarkerTag>
    {
        private static Dictionary<string, HighlightWordTagger> Taggers = new Dictionary<string, HighlightWordTagger>();
        private Dictionary<SnapshotSpan, List<PublishDiagnosticsNotification.Diagnostic>> Diagnostics;

        public static readonly object InitializationLock = new object();
        public static bool active = false;
        public static event EventHandler<string> TaggerCreatedEvent;

        ITextView View { get; set; }
        ITextBuffer SourceBuffer { get; set; }
        ITextSearchService TextSearchService { get; set; }
        ITextStructureNavigator TextStructureNavigator { get; set; }
        ITextSnapshot CurrentRequestSnapshot { get; set; }
        NormalizedSnapshotSpanCollection TagSpans { get; set; }


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public HighlightWordTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
ITextStructureNavigator textStructureNavigator)
        {
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            this.TextSearchService = textSearchService;
            this.TextStructureNavigator = textStructureNavigator;
            this.CurrentRequestSnapshot = null;
            this.TagSpans = new NormalizedSnapshotSpanCollection();
            sourceBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document);
            if (!document.FilePath.Equals("Temp.txt"))
            {
                string uri = new Uri(document.FilePath).AbsoluteUri;
                lock (InitializationLock) Taggers[uri] = this;
                TaggerCreatedEvent?.Invoke(this, uri);
            }
        }

        public static HighlightWordTagger GetTaggerForUri(string uri)
        {
            Taggers.TryGetValue(uri, out HighlightWordTagger tagger);
            return tagger;
        }

        public static void NotifyDocumentClosed(string uri)
        {
            Taggers[uri] = null;
        }


        public void UpdateData(PublishDiagnosticsNotification notification)
        {
            CodyzeVSPluginPackage.DebugLine("Updating tagging data...");
            CurrentRequestSnapshot = SourceBuffer.CurrentSnapshot;
            List<SnapshotSpan> spans = new List<SnapshotSpan>();
            Diagnostics = new Dictionary<SnapshotSpan, List<PublishDiagnosticsNotification.Diagnostic>>();
            foreach (var d in notification.Parameters.Diagnostics)
            {
                int startPos = ParseDocumentPositionToCharacterPosition(d.Range.Start);
                int length = GetSnapshotSpanLength(d.Range.Start, d.Range.End);
                SnapshotSpan snapshotSpan = new SnapshotSpan(CurrentRequestSnapshot, startPos, length);
                if (!spans.Contains(snapshotSpan)) spans.Add(snapshotSpan);
                List<PublishDiagnosticsNotification.Diagnostic> list;
                if (!Diagnostics.ContainsKey(snapshotSpan))
                {
                    list = new List<PublishDiagnosticsNotification.Diagnostic>();
                    list.Add(d);
                    Diagnostics.Add(snapshotSpan, list);
                }
                else
                {
                    Diagnostics.TryGetValue(snapshotSpan, out list);
                    list.Add(d);
                    Diagnostics.Remove(snapshotSpan);
                    Diagnostics.Add(snapshotSpan, list);
                }
            }
            LineAsyncQuickInfoSource.UpdateDiagnosticProperties(Diagnostics, this);
            TagSpans = new NormalizedSnapshotSpanCollection(spans);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(CurrentRequestSnapshot, 0, CurrentRequestSnapshot.Length)));
        }

        private int ParseDocumentPositionToCharacterPosition(RPC.PublishDiagnosticsNotification.Position documentPosition)
        {
            int sum = 0;
            for (int i = 0; i < CurrentRequestSnapshot.LineCount - 1 && i < documentPosition.Line; i++)
            {
                sum += CurrentRequestSnapshot.GetLineFromLineNumber(i).LengthIncludingLineBreak;
            }
            sum += documentPosition.Character;
            return sum;
        }

        private int GetSnapshotSpanLength(RPC.PublishDiagnosticsNotification.Position documentPositionStart, RPC.PublishDiagnosticsNotification.Position documentPositionEnd)
        {
            if (documentPositionStart.Line == documentPositionEnd.Line)
                return documentPositionEnd.Character - documentPositionStart.Character;
            int length = 0;
            for (int i = documentPositionStart.Line; i < CurrentRequestSnapshot.LineCount - 1 && i < documentPositionEnd.Line; i++)
            {
                length += CurrentRequestSnapshot.GetLineFromLineNumber(i).LengthIncludingLineBreak;
            }
            length += documentPositionEnd.Character;
            return length;
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (CurrentRequestSnapshot == null)
                yield break;

            NormalizedSnapshotSpanCollection tagSpans = TagSpans;

            if (spans[0].Snapshot != CurrentRequestSnapshot)
            {
                tagSpans = new NormalizedSnapshotSpanCollection(
                    TagSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));
            }

            foreach (SnapshotSpan span in tagSpans)
            {
                int severity = 5;
                List<PublishDiagnosticsNotification.Diagnostic> diagnostics;
                if(Diagnostics.TryGetValue(span, out diagnostics))
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        if (severity > diagnostic.Severity) //choose the most severe severity
                            severity = diagnostic.Severity;
                    }
                    yield return new TagSpan<TextMarkerTag>(span, HighlightingFormatHandler.ChooseHighlightingFormat(span, severity));
                }
                
            }
        }


    }
}
