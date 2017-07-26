using System;
using System.Configuration;

namespace SvnSyncAuthzLdap
{
    public static class SettingsReader
    {
        public static string GetSettingString(string key, string defaultValue, bool required)
        {
            string retVal = ConfigurationManager.AppSettings.Get(key);
            if (string.IsNullOrEmpty(retVal))
            {
                if (required)
                {
                    throw new ArgumentException("Required application setting value is empty or missing: " + key);
                }

                retVal = defaultValue;
            }

            return retVal;
        }
    }
}
