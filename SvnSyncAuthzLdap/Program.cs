using System;
using System.Collections.Generic;
using System.Linq;

namespace SvnSyncAuthzLdap
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                LdapReader lr = new LdapReader();
                Dictionary<string, Group> groups = lr.GetGroups();

                AuthzWriter aw = new AuthzWriter(SettingsReader.GetSettingString("outputFile", "authz", true), groups.Values.ToList<Group>());
                aw.Write();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), Logger.LogType.ERROR);
                return 1;
            }

            return 0;
        }
    }
}
