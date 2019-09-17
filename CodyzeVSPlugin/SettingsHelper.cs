using System.Windows.Forms;
using System.IO;

namespace CodyzeVSPlugin
{
    class SettingsHelper
    {
        public static string CheckCommandLineArguments()
        {
            CheckForPathToMarkFiles();
            string pathToMarkFiles = CustomSettingsManager.ReadPathToMarkFilesSetting();
            string oldArguments = CustomSettingsManager.ReadCommandLineArgumentsSetting();
            string arguments = CheckCommandLineArguments(oldArguments, pathToMarkFiles);
            return arguments;
        }

        public static string CheckCommandLineArguments(string arguments, string pathToMarkFiles)
        {
            if (arguments == null || arguments.Length == 0)
                return StandardizeCommandLineArguments(pathToMarkFiles);

            if (!arguments.StartsWith("-l "))
                arguments = "-l " + arguments;

            if (!arguments.Contains("-m " + pathToMarkFiles))
                arguments += " -m " + pathToMarkFiles;

            return arguments;
        }

        private static string StandardizeCommandLineArguments(string pathToMarkFiles)
        {
            string arguments = "-l -m " + pathToMarkFiles;
            CustomSettingsManager.AddUpdateCommandLineArgumentsSettings(arguments);
            return arguments;
        }

        public static string CheckForPathToMarkFiles()
        {
            string path = CustomSettingsManager.ReadPathToMarkFilesSetting();
            if (path == null || path.Equals("") || !Directory.Exists(path))
            {
                string newPath = ShowFolderBrowserForMarkFilesLocation(path);
                path = newPath;
                CustomSettingsManager.AddUpdatePathToMarkFilesSettings(path);
            }
            return path;
        }

        public static string ShowFolderBrowserForMarkFilesLocation(string path)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Select the mark files directory";
                DialogResult res = folderBrowser.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (Directory.Exists(folderBrowser.SelectedPath))
                        path = folderBrowser.SelectedPath;
                }
            }
            return path;
        }


    }
}
