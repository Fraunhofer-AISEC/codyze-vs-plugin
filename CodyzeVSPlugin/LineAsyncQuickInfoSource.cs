using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CodyzeVSPlugin

{
    internal sealed class LineAsyncQuickInfoSource : IAsyncQuickInfoSource
    {
        private ITextBuffer _textBuffer;
        private static HighlightWordTagger activeTagger = null;
        private static Dictionary<HighlightWordTagger, List<Dictionary<SnapshotSpan,ContainerElement>>> DiagnosticQuickInfos = new Dictionary<HighlightWordTagger,List<Dictionary<SnapshotSpan,ContainerElement>>>();

        public static void NotifyWindowFocusChanged(String documentName)
        {
            string uri = new Uri(documentName).AbsoluteUri;
            HighlightWordTagger tagger;
            if ((tagger = HighlightWordTagger.GetTaggerForUri(uri)) != null)
                activeTagger = tagger;
        }

        public LineAsyncQuickInfoSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);

            if(triggerPoint != null && activeTagger != null)
            {
                var line = triggerPoint.Value.GetContainingLine();
                var lineNumber = triggerPoint.Value.GetContainingLine().LineNumber;
                var lineSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);

                List<Dictionary<SnapshotSpan, ContainerElement>> dics;
                if(DiagnosticQuickInfos.TryGetValue(activeTagger, out dics))
                {
                    foreach (var dic in dics)
                    {
                        var spans = dic.Keys;
                        foreach (var span in spans)
                        {
                            if (((SnapshotPoint)triggerPoint).Snapshot.Equals(span.Snapshot) && span.Contains((SnapshotPoint)triggerPoint))
                            {
                                ContainerElement elm;
                                dic.TryGetValue(span, out elm);
                                return Task.FromResult(new QuickInfoItem(lineSpan, elm));
                            }
                        }


                    }
                }
                
                
            }
            return Task.FromResult<QuickInfoItem>(null);
        }


        public static void UpdateDiagnosticProperties(Dictionary<SnapshotSpan, List<RPC.PublishDiagnosticsNotification.Diagnostic>> Diagnostics, HighlightWordTagger tagger)
        {
            List<Dictionary<SnapshotSpan,ContainerElement>> dics = new List<Dictionary<SnapshotSpan, ContainerElement>>();
            foreach(SnapshotSpan span in Diagnostics.Keys)
            {
                List<RPC.PublishDiagnosticsNotification.Diagnostic> diagnosticList;
                Diagnostics.TryGetValue(span, out diagnosticList);
                List<ContainerElement> elms = new List<ContainerElement>();
                foreach(var diagnostic in diagnosticList)
                {
                    if (diagnostic.Message.Equals("File is being scanned")) continue;
                    var sev = new ClassifiedTextElement(
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "severity: "),
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, diagnostic.Severity.ToString())
                                );
                    string codeText;
                    if (diagnostic.Code == null) codeText = "the code was not specified.";
                    else codeText = diagnostic.Code;
                    var code = new ClassifiedTextElement(
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "code: "),
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, codeText)
                                );
                    var message = new ClassifiedTextElement(
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "message: "),
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, diagnostic.Message)
                                );
                    ContainerElement diagnosticElm = new ContainerElement(
                                ContainerElementStyle.Stacked,
                                sev,
                                code,
                                message
                                );
                    elms.Add(diagnosticElm);
                }
                if (elms.Count > 0)
                {
                    var BigElm = new ContainerElement(
                                    ContainerElementStyle.VerticalPadding,
                                    elms);
                    var dic = new Dictionary<SnapshotSpan, ContainerElement>();
                    dic[span] = BigElm;
                    dics.Add(dic);
                }
                
            }
            activeTagger = tagger;
            if(dics.Count > 0)
                DiagnosticQuickInfos[activeTagger] = dics;

        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}