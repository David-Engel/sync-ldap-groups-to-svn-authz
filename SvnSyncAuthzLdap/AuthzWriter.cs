using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SvnSyncAuthzLdap
{
    public class AuthzWriter
    {
        private const string START_CONTENT = "### Start generated content: LDAP Groups to Subversion Authz Groups Bridge";
        private const string END_CONTENT = "### End generated content: LDAP Groups to Subversion Authz Groups Bridge ###";
        private string _authzFile;
        private List<Group> _groups;
        public AuthzWriter(string authzFile, List<Group> groups)
        {
            _authzFile = authzFile;
            _groups = groups;
        }

        public void Write()
        {
            DoBackups();
            StringBuilder sbBeforeContent = new StringBuilder();
            StringBuilder sbAfterContent = new StringBuilder();
            StringBuilder current = sbBeforeContent;
            bool inContent = false;
            if (File.Exists(_authzFile))
            {
                using (StreamReader sr = new StreamReader(_authzFile))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!inContent && line.StartsWith(START_CONTENT))
                        {
                            inContent = true;
                        }
                        else if (inContent && line.StartsWith(END_CONTENT))
                        {
                            inContent = false;
                            current = sbAfterContent;
                        }
                        else if (inContent)
                        {
                            continue;
                        }
                        else
                        {
                            current.AppendLine(line);
                        }
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(_authzFile))
            {
                sw.Write(sbBeforeContent.ToString());
                if (!sbBeforeContent.ToString().Contains("[groups]"))
                {
                    sw.WriteLine("[groups]");
                    sw.WriteLine();
                }

                sw.WriteLine(string.Format(START_CONTENT + " {0} ###", DateTime.Now.ToString()));
                foreach (Group group in _groups)
                {
                    Regex rex = new Regex("\\W");
                    group.FormattedGroupName = rex.Replace(group.CN, "");
                }

                _groups.Sort();

                foreach (Group group in _groups)
                {
                    group.GroupMembers.Sort();
                    sw.WriteLine(string.Format("{0} = {1}", group.FormattedGroupName, GetGroupMemberList(group)));
                }

                sw.WriteLine();
                sw.WriteLine("################################################################################");
                sw.WriteLine("###########   LDAP Groups to Subversion Authz Groups Bridge (Legend)  ##########");
                sw.WriteLine("################################################################################");
                foreach (Group group in _groups)
                {
                    sw.WriteLine(string.Format("### {0} = {1}", group.FormattedGroupName, group.DistinguishedName));
                }

                sw.WriteLine(END_CONTENT);
                sw.Write(sbAfterContent.ToString());
            }
        }

        private void DoBackups()
        {
            string daysToKeepString = SettingsReader.GetSettingString("outputFileBackupDaysToKeep", "0", false);
            int daysToKeep = 0;
            int.TryParse(daysToKeepString, out daysToKeep);

            if (daysToKeep <= 0)
            {
                return;
            }

            if (File.Exists(_authzFile))
            {
                using (StreamWriter sw = new StreamWriter(_authzFile + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".bak"))
                {
                    using (StreamReader sr = new StreamReader(_authzFile))
                    {
                        sw.Write(sr.ReadToEnd());
                    }
                }
            }

            FileInfo authzFile = new FileInfo(_authzFile);
            foreach (string file in Directory.GetFiles(authzFile.DirectoryName, authzFile.Name + "_*.bak", SearchOption.TopDirectoryOnly))
            {
                DateTime fileDate;
                FileInfo fileInfo = new FileInfo(file);
                if (DateTime.TryParseExact(fileInfo.Name.Substring(authzFile.Name.Length + 1, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                {
                    if (fileDate < DateTime.Now.Subtract(new TimeSpan(daysToKeep, 0, 0, 0)))
                    {
                        try
                        {
                            fileInfo.Delete();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Unable to delete old backup file: " + file + "\r\n" + ex.Message, Logger.LogType.WARN);
                        }
                    }
                }
            }
        }

        private string GetGroupMemberList(Group group)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;

            foreach (Member member in group.GroupMembers)
            {
                if (member is Group)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append("@" + ((Group)member).FormattedGroupName);
                }
            }

            foreach (Member member in group.GroupMembers)
            {
                if (member is User)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append(((User)member).UserId.ToLower());
                }
            }

            return sb.ToString();
        }
    }
}
