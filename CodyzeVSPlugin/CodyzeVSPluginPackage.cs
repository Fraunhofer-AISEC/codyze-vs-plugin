using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using CodyzeVSPlugin.RPC;
using Task = System.Threading.Tasks.Task;
using static CodyzeVSPlugin.MyUserControl;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;
using System.IO;

namespace CodyzeVSPlugin
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CodyzeVSPluginPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionPageCustom), "Codyze Plugin", "Codyze Settings", 0, 0, true)]
    public sealed class CodyzeVSPluginPackage : AsyncPackage
    {
        /// <summary>
        /// CodyzeVSPluginPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "1554e953-cf82-493e-8d27-377ca3f09b70";


        #region Package Members

        private DTE DTE;
        private IVsSolution IVsSolution;
        private readonly Dictionary<Window, string> ActiveDocuments = new Dictionary<Window, string>();
        private readonly Dictionary<string, IVsHierarchy> ProjectMappings = new Dictionary<string, IVsHierarchy>();
        private LSPClient LSPClient;
        private ErrorListProvider ErrorListProvider;
        private DocumentEvents documentEvents;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await Command1.InitializeAsync(this);
            DTE = await GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(DTE);
            ErrorListProvider = new ErrorListProvider(this);
            IVsSolution = await GetServiceAsync(typeof(IVsSolution)) as IVsSolution;
            Assumes.Present(IVsSolution);
            DebugLine("Got DTE service version " + DTE.Version);

            DebugLine("Registering Events...");
            DTE.Events.SolutionEvents.Opened += SolutionEvents_Opened;
            DTE.Events.SolutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;
            DTE.Events.WindowEvents.WindowActivated += WindowEvents_WindowActivated;
            DTE.Events.WindowEvents.WindowClosing += WindowEvents_WindowClosing;
            documentEvents = DTE.Events.DocumentEvents;
            documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            SolutionEvents_Opened();
        }

        private async void SolutionEvents_Opened()
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync();
            DebugLine("Loading LSP Client...");
            this.LSPClient = LSPClient.Instance;
            this.LSPClient.SetNotificationHandler("textDocument/publishDiagnostics", OnPublishDiagnosticsAsync);
            var rootUri = new Uri(Path.GetDirectoryName(DTE.Solution.FileName)).AbsoluteUri;
            await LSPClient.InitializeAsync(rootUri);

            var activeWindow = DTE.ActiveWindow;
            if (activeWindow?.Document != null) NotifyDocumentOpened(activeWindow); else DebugLine("No active document.");
        }

        public void AnalyzeCodeButtonPressed()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var activeWindow = DTE.ActiveWindow;
            if (activeWindow?.Document != null)
            {
                string content = GetDocumentText(activeWindow.Document);
                var uri = new Uri(activeWindow.Document.FullName).AbsoluteUri;
                LSPClient.DidSaveDocument(uri, content); //provisorily
                                                         //LSPClient.ChangeDocument(uri, DidChangeTextDocumentNotification.MYSTERY_VERSION_NUMBER, content); //not implemented yet, server sided
            }
            else
                System.Windows.MessageBox.Show("There is no open document.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private void DocumentEvents_DocumentSaved(Document Document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string uri = new Uri(Document.FullName).AbsoluteUri;
            var text = GetDocumentText(Document);
            LSPClient.DidSaveDocument(uri, text);
        }


        private void WindowEvents_WindowClosing(Window Window)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ActiveDocuments.TryGetValue(Window, out string documentUri);
            if (documentUri == null)
            {
                DebugLine("Unknown document to close: " + Window.Caption);
                return;
            }
            DebugLine("Closing document: " + documentUri);
            ActiveDocuments.Remove(Window);
            HighlightWordTagger.NotifyDocumentClosed(documentUri);
            ProjectMappings.Remove(documentUri);
            ClearErrorList(new Uri(documentUri).LocalPath);
            LSPClient.CloseDocument(documentUri);
        }

        private void ClearErrorList(string documentPath)
        {
            for (int i = 0; i < ErrorListProvider.Tasks.Count; i++)
                if (ErrorListProvider.Tasks[i].Document == documentPath)
                    ErrorListProvider.Tasks.RemoveAt(i--);
        }

        private Task<HighlightWordTagger> GetTaggerAsync(PublishDiagnosticsNotification m)
        {
            HighlightWordTagger tagger;

            // Wait for the tagger to be initialized
            lock (HighlightWordTagger.InitializationLock)
            {
                if ((tagger = HighlightWordTagger.GetTaggerForUri(m.Parameters.Uri)) == null)
                {
                    DebugLine("Registering for event");
                    var tcs = new TaskCompletionSource<HighlightWordTagger>();
                    void handler(object sender, string uri)
                    {
                        if (uri == m.Parameters.Uri)
                        {
                            tcs.SetResult(HighlightWordTagger.GetTaggerForUri(m.Parameters.Uri));
                            HighlightWordTagger.TaggerCreatedEvent -= handler;
                            DebugLine("Got tagger.");
                        }
                    }

                    HighlightWordTagger.TaggerCreatedEvent += handler;
                    return tcs.Task;
                }
            }

            return Task.FromResult(tagger);
        }

        private async Task OnPublishDiagnosticsAsync(NotificationMessage m)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var message = m as PublishDiagnosticsNotification;
            var getTaggerTask = GetTaggerAsync(message);

            string document = new Uri(message.Parameters.Uri).LocalPath;
            ClearErrorList(document);
            foreach (var d in message.Parameters.Diagnostics)
            {
                TaskErrorCategory errorCategory;
                if (d.Severity == 1)
                    errorCategory = TaskErrorCategory.Error;
                else if (d.Severity == 2)
                    errorCategory = TaskErrorCategory.Warning;
                else
                    continue; // TODO Do we need other categories?

                var error = new ErrorTask()
                {
                    ErrorCategory = errorCategory,
                    Category = TaskCategory.CodeSense,
                    Text = d.Message,
                    Document = document,
                    Line = d.Range.Start.Line + 1,
                    Column = d.Range.Start.Character + 1,
                    HierarchyItem = ProjectMappings[message.Parameters.Uri]
                };

                error.Navigate += (sender, e) =>
                {
                    ErrorListProvider.Navigate(error, Guid.Parse(EnvDTE.Constants.vsViewKindCode));
                };

                ErrorListProvider.Tasks.Add(error);
            }
            ErrorListProvider.Show();
            DebugLine("Error list time: " + sw.ElapsedMilliseconds + " ms");
            // Notify the tagger
            HighlightWordTagger tagger = await getTaggerTask;
            tagger.UpdateData(message);
            sw.Stop();
            DebugLine("Total time: " + sw.ElapsedMilliseconds + " ms");
        }

        private static string GetDocumentText(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var textDocument = document.Object("TextDocument") as TextDocument;
            EditPoint editPoint = textDocument.StartPoint.CreateEditPoint();
            var content = editPoint.GetText(textDocument.EndPoint);
            return content;
        }

        private void NotifyDocumentOpened(Window window)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!window.Document.FullName.EndsWith(".cpp"))
                return;
            var uri = new Uri(window.Document.FullName).AbsoluteUri;
            ActiveDocuments[window] = uri;
            IVsSolution.GetProjectOfUniqueName(window.Document.ProjectItem.ContainingProject.UniqueName, out IVsHierarchy hierarchyItem);
            ProjectMappings[uri] = hierarchyItem;
            DebugLine("Notify document opened: " + window.Document.FullName);
            string content = GetDocumentText(window.Document);
            LSPClient.OpenDocument(window.Document.FullName, content);
        }

        private void SolutionEvents_BeforeClosing()
        {
            LSPClient.Terminate();
        }

        private void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Document openedDocument = GotFocus.Document;
            if (openedDocument == null) return;
            LineAsyncQuickInfoSource.NotifyWindowFocusChanged(openedDocument.FullName);
            if (!ActiveDocuments.ContainsKey(GotFocus)) NotifyDocumentOpened(GotFocus);
        }

        public static void DebugLine(string message)
        {
#if DEBUG
            foreach (string line in message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                Debug.WriteLine(line, "Codyze");
#else
            return;
#endif
        }

        #endregion
    }
}
