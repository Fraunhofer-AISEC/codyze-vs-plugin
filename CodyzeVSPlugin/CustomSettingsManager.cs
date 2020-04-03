using System.Configuration;

namespace CodyzeVSPlugin
{
    class CustomSettingsManager
    {
        public static string ReadPathSetting()
        {
            try
            {
                return Properties.Settings.Default.pathToCPGA;
            }
            catch (ConfigurationErrorsException)
            {
                CodyzeVSPluginPackage.DebugLine("Error reading app settings");
                return null;
            }
        }

        public static void AddUpdatePathSettings(string value)
        {
            try
            {
                Properties.Settings.Default.pathToCPGA = value;
                Properties.Settings.Default.Save();
            }
            catch (ConfigurationErrorsException)
            {
                CodyzeVSPluginPackage.DebugLine("Error writing app settings");
            }
        }

        public static string ReadCommandLineArgumentsSetting()
        {
            try
            {
                return Properties.Settings.Default.commandLineArguments;
            }
            catch (ConfigurationErrorsException)
            {
                CodyzeVSPluginPackage.DebugLine("Error reading app settings");
                return null;
            }
        }

        public static void AddUpdateCommandLineArgumentsSettings(string arguments)
        {
            try
            {
                Properties.Settings.Default.commandLineArguments = arguments;
                Properties.Settings.Default.Save();
            }
            catch (ConfigurationErrorsException)
            {
                CodyzeVSPluginPackage.DebugLine("Error writing app settings");
            }
        }

        public static string ReadPathToMarkFilesSetting()
        {
            try
            {
                return Properties.Settings.Default.pathToMarkFiles;
            }
            catch (ConfigurationErrorsException)
            {
                CodyzeVSPluginPackage.DebugLine("Error reading app settings");
                return null;
            }
        }

        public static void AddUpdatePathToMarkFilesSettings(string path)
        {
            string oldPath = ReadPathToMarkFilesSetting();
            string oldArguments = ReadCommandLineArgumentsSetting();
            if (oldPath != null && oldArguments != null)
            {
                Properties.Settings.Default.pathToMarkFiles = path;
                string newArguments;
                if (oldArguments.Equals("") || oldPath.Equals("") || oldArguments.StartsWith("\r\n") || !oldArguments.Contains(oldPath))
                {
                    newArguments = "-l -m " + path;
                }
                else
                {
                    newArguments = oldArguments.Replace(oldPath, path);
                }
                AddUpdateCommandLineArgumentsSettings(newArguments);
            }
        }

    }
}