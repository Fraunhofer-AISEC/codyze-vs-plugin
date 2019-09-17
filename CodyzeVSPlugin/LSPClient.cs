using Newtonsoft.Json;
using Microsoft.VisualStudio.Shell;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using Task = System.Threading.Tasks.Task;
using CodyzeVSPlugin.RPC;
using System.Threading.Tasks;

namespace CodyzeVSPlugin
{
    class LSPClient
    {
        private static LSPClient instance;
        private static readonly object SingletonLock = new object();
        private string ServerBasePath;
        private bool Shutdown = false;

        public static LSPClient Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    if (instance == null)
                        instance = new LSPClient();
                    return instance;
                }
            }
        }
        private readonly Process ServerProcess = new Process();
        private bool Initialized;

        private delegate Task OnResponseMessageReceivedHandler(ResponseMessage m);
        public delegate Task OnNotificationMessageReceivedHandler(NotificationMessage m);

        private readonly ConcurrentDictionary<long, OnResponseMessageReceivedHandler> RequestHandlers =
            new ConcurrentDictionary<long, OnResponseMessageReceivedHandler>();
        private readonly ConcurrentDictionary<string, OnNotificationMessageReceivedHandler> NotificationHandlers =
            new ConcurrentDictionary<string, OnNotificationMessageReceivedHandler>();

        private LSPClient()
        {
            CheckForServerPath();
            this.ServerProcess.StartInfo.UseShellExecute = false;
            this.ServerProcess.StartInfo.WorkingDirectory = ServerBasePath;
            this.ServerProcess.StartInfo.FileName = Path.Combine(ServerBasePath, @"bin\codyze.bat");
            this.ServerProcess.StartInfo.Arguments = SettingsHelper.CheckCommandLineArguments();
            this.ServerProcess.StartInfo.RedirectStandardOutput = true;
            this.ServerProcess.StartInfo.RedirectStandardInput = true;
            this.ServerProcess.StartInfo.RedirectStandardError = false;
#if DEBUG
            this.ServerProcess.StartInfo.CreateNoWindow = false;
#else
            this.ServerProcess.StartInfo.CreateNoWindow = true;
#endif

            CodyzeVSPluginPackage.DebugLine("Server Command: " + this.ServerProcess.StartInfo.FileName + " " + this.ServerProcess.StartInfo.Arguments);
        }

        private async Task HandleOutputAsync()
        {
            string line;
            using (StreamReader sr = new StreamReader(this.ServerProcess.StandardOutput.BaseStream))
            {
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("Content-Length: "))
                    {
                        int length = int.Parse(line.Substring("Content-Length: ".Length));
                        await sr.ReadLineAsync(); // Empty Line
                        char[] buffer = new char[length];
                        await sr.ReadAsync(buffer, 0, length); // Read chars as indicated by content length
                        var messageString = new string(buffer);
                        CodyzeVSPluginPackage.DebugLine("Message from Server: " + messageString);
                        Message m = DeserializeIt(messageString);

                        if (m is ResponseMessage m1)
                        {
                            if (RequestHandlers.TryGetValue(m1.ID, out OnResponseMessageReceivedHandler handler)) await handler(m1);
                        }
                        else if (m is NotificationMessage m2)
                        {
                            if (NotificationHandlers.TryGetValue(m2.Method, out OnNotificationMessageReceivedHandler handler)) await handler(m2);
                        }
                        else
                        {
                            CodyzeVSPluginPackage.DebugLine("Unhandled message: " + m);
                        }
                    }
                }
            }
        }

        public async Task InitializeAsync(string rootUri)
        {
            if (Initialized)
                return;

            this.ServerProcess.Start();
            _ = Task.Run(this.HandleOutputAsync); // Start reading StandardOutput of server

            var tcs = new TaskCompletionSource<object>();
            InitializeRequest initializeRequest = new InitializeRequest(this.ServerProcess.Id, rootUri);
            InvokeMethod(initializeRequest, async m =>
            {
                //object capabilities;
                //if (responseMessage.Result.TryGetValue("capabilities", out capabilities))
                //CodyzeVSPluginPackage.DebugLine(capabilities); //TODO: do something with this information?
                InvokeMethod(new InitializedNotification());
                Initialized = true; //now other requests are allowed to be sent
                CodyzeVSPluginPackage.DebugLine("Initialized.");
                tcs.SetResult(null);
                await Task.CompletedTask;
            });
            await tcs.Task;
        }

        private (int, string) SerializeIt(Message message)
        {
            string messageSerialized = JsonConvert.SerializeObject(message);
            int contentLength = Encoding.UTF8.GetByteCount(messageSerialized);
            return (contentLength, messageSerialized);
        }

        private Message DeserializeIt(string messageString)
        {
            Message message = null;
            if (messageString.Contains(@"""jsonrpc"":""2.0"""))
            {
                if (messageString.Contains(@"""id""")) message = JsonConvert.DeserializeObject<ResponseMessage>(messageString);
                else message = JsonConvert.DeserializeObject<NotificationMessage>(messageString, new NotificationConverter());
            }
            return message;
        }

        private void InvokeMethod(RequestMessage requestMessage, OnResponseMessageReceivedHandler handler = null)
        {
            if (Shutdown)
                return;

            if (!Initialized && !(requestMessage is InitializeRequest))
            {
                CodyzeVSPluginPackage.DebugLine("ERROR: Invoked method before initialization.");
                return;
            }
            (int contentLength, string requestMessageSerialized) = SerializeIt(requestMessage);
            var messageToBeSent = "Content-Length: " + contentLength + "\r\n\r\n" + requestMessageSerialized;
            if (handler != null) RequestHandlers[requestMessage.ID] = handler;
            CodyzeVSPluginPackage.DebugLine("Message to Server:");
            CodyzeVSPluginPackage.DebugLine(messageToBeSent);
            ServerProcess.StandardInput.Write(messageToBeSent);
        }

        private void InvokeMethod(NotificationMessage notificationMessage)
        {
            if (Shutdown && !(notificationMessage is ExitNotification))
                return;

            if (!Initialized && !(notificationMessage is InitializedNotification))
            {
                CodyzeVSPluginPackage.DebugLine("ERROR: Invoked method before initialization.");
                return;
            }
            (int contentLength, string notificationMessageSerialized) = SerializeIt(notificationMessage);
            var messageToBeSent = "Content-Length: " + contentLength + "\r\n\r\n" + notificationMessageSerialized;
            CodyzeVSPluginPackage.DebugLine("Notification to Server:");
            CodyzeVSPluginPackage.DebugLine(messageToBeSent);
            ServerProcess.StandardInput.Write(messageToBeSent);
        }

        public void OpenDocument(string path, string content = null)
        {
            InvokeMethod(new DidOpenTextDocumentNotification(path, content));
        }

        public void CloseDocument(string uri)
        {
            InvokeMethod(new DidCloseTextDocumentNotification(uri));
        }

        public void ChangeDocument(string uri, long version, string text)
        {
            InvokeMethod(new DidChangeTextDocumentNotification(uri, version, text));
        }

        public void DidSaveDocument(string uri, string text)
        {
            InvokeMethod(new DidSaveTextDocumentNotification(uri, text));
        }

        public void SetNotificationHandler(string method, OnNotificationMessageReceivedHandler handler)
        {
            NotificationHandlers[method] = handler;
        }

        public void Terminate()
        {
            lock(SingletonLock)
            {
                instance = null;
            }

            InvokeMethod(new ShutdownRequest(), async m1 =>
            {
                if (m1.Error == null)
                    InvokeMethod(new ExitNotification()); //everything is okay, we can send now an exit notification
                else
                    CodyzeVSPluginPackage.DebugLine("Shutdown Error: " + m1.Error); //TODO do a wise reaction

                await Task.CompletedTask;
            });
            Shutdown = true;

            ServerProcess.WaitForExit(5000);
            if (!ServerProcess.HasExited && !ServerProcess.CloseMainWindow())
                ServerProcess.Kill();
            CodyzeVSPluginPackage.DebugLine("Exit with code " + ServerProcess.ExitCode);
        }

        public void CheckForServerPath()
        {
            string path = Properties.Settings.Default.pathToCPGA.ToString();

            while (!File.Exists(Path.Combine(path, @"bin\codyze.bat")))
            {
                using (var folderBrowser = new FolderBrowserDialog())
                {
                    folderBrowser.Description = "Select the path to Codyze (containing the bin/lib folders).";
                    if (folderBrowser.ShowDialog() == DialogResult.OK)
                    {
                        string folderPath = folderBrowser.SelectedPath;
                        path = folderPath;
                        CustomSettingsManager.AddUpdatePathSettings(folderPath);
                    }
                }
            }

            this.ServerBasePath = path;
        }

    }
}
