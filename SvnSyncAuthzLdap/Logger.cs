using System;
using System.IO;

namespace SvnSyncAuthzLdap
{
    public static class Logger
    {
        public enum LogType
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
        }

        public static void Log(string message, LogType type)
        {
            string levelSetting = SettingsReader.GetSettingString("loggingLevel", "INFO", false);
            LogType level = LogType.DEBUG;
            if (!Enum.TryParse<LogType>(levelSetting, out level))
            {
                level = LogType.INFO;
            }

            if (level <= type)
            {
                using (StreamWriter sw = new StreamWriter(SettingsReader.GetSettingString("logFile", "SvnSyncAuthzLdap.log", false), true))
                {
                    sw.WriteLine(string.Format("{0}: {1} {2}", type.ToString(), DateTime.Now.ToString(), message));
                }
            }
        }
    }
}
