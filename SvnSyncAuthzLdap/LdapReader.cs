using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;

namespace SvnSyncAuthzLdap
{
    public class LdapReader
    {
        private string _bindUser;
        private string _bindPassword;
        private string _ldapHost;
        private string _baseQuery;
        private string _userIdAttribute;

        public Dictionary<string, Group> GetGroups()
        {
            LoadSettings();

            LdapDirectoryIdentifier di = new LdapDirectoryIdentifier(_ldapHost);
            NetworkCredential nc = new NetworkCredential(_bindUser, _bindPassword);
            LdapConnection conn = new LdapConnection(di, nc);
            SearchRequest groupSearch = new SearchRequest(_baseQuery, "objectClass=group", SearchScope.Subtree, new string[] { "cn", "member", "objectsid" });

            Logger.Log("Getting groups from the directory", Logger.LogType.DEBUG);
            SearchResponse groupResponse = (SearchResponse)conn.SendRequest(groupSearch);
            Dictionary<string, Group> groups = new Dictionary<string, Group>();
            if (groupResponse.Entries.Count <= 0)
            {
                Logger.Log("No groups found in the directory.", Logger.LogType.INFO);
                return groups;
            }

            SearchRequest userSearch = new SearchRequest(_baseQuery, "(&(objectCategory=person)(objectClass=user))", SearchScope.Subtree, new string[] { "cn", _userIdAttribute, "primarygroupid" });
            Logger.Log("Getting users from the directory", Logger.LogType.DEBUG);
            SearchResponse userResponse = (SearchResponse)conn.SendRequest(userSearch);
            Dictionary<string, User> users = new Dictionary<string, User>();
            foreach (SearchResultEntry entry in userResponse.Entries)
            {
                Logger.Log(entry.DistinguishedName, Logger.LogType.DEBUG);
                User user = new User();
                user.DistinguishedName = entry.DistinguishedName;
                if (entry.Attributes.Contains("cn") &&
                    entry.Attributes["cn"].GetValues(typeof(string)).Length >= 1)
                {
                    user.CN = entry.Attributes["cn"].GetValues(typeof(string))[0].ToString();
                }

                if (entry.Attributes.Contains("primarygroupid") &&
                    entry.Attributes["primarygroupid"].GetValues(typeof(string)).Length >= 1)
                {
                    user.PrimaryGroupId = int.Parse(entry.Attributes["primarygroupid"].GetValues(typeof(string))[0].ToString());
                }

                if (entry.Attributes.Contains(_userIdAttribute) &&
                    entry.Attributes[_userIdAttribute].GetValues(typeof(string)).Length >= 1)
                {
                    user.UserId = entry.Attributes[_userIdAttribute].GetValues(typeof(string))[0].ToString();
                    Logger.Log("Adding user: " + user.UserId, Logger.LogType.DEBUG);
                    users.Add(user.DistinguishedName, user);
                }
                else
                {
                    Logger.Log("Skipping entry due to no " + _userIdAttribute + ": " + entry.DistinguishedName, Logger.LogType.DEBUG);
                }
            }

            Dictionary<string, Group> tempGroups = new Dictionary<string, Group>();
            foreach (SearchResultEntry entry in groupResponse.Entries)
            {
                Logger.Log(entry.DistinguishedName, Logger.LogType.INFO);
                Group group = new Group();
                group.DistinguishedName = entry.DistinguishedName;
                byte[] sid = (byte[])entry.Attributes["objectSid"].GetValues(typeof(byte[]))[0];
                if (sid.Length >= 24)
                {
                    group.ObjectSid = BitConverter.ToInt32(sid, 24);
                }

                tempGroups.Add(group.DistinguishedName, group);
                if (entry.Attributes.Contains("cn") &&
                    entry.Attributes["cn"].GetValues(typeof(string)).Length >= 1)
                {
                    group.CN = entry.Attributes["cn"].GetValues(typeof(string))[0].ToString();
                }
            }

            foreach (SearchResultEntry entry in groupResponse.Entries)
            {
                Group group = tempGroups[entry.DistinguishedName];
                group.GroupMembers = GetMembersFromGroup(conn, entry, tempGroups, users);
                if (group.GroupMembers.Count > -1)
                {
                    Logger.Log("Adding group: " + group.DistinguishedName, Logger.LogType.DEBUG);
                    groups.Add(group.DistinguishedName, group);
                }
                else
                {
                    Logger.Log("Skipping group due to no members: " + group.DistinguishedName, Logger.LogType.DEBUG);
                }
            }

            // Add users to their primary group since they are not "members" of that group
            foreach (User user in users.Values)
            {
                if (user.PrimaryGroupId > 0)
                {
                    bool found = false;
                    foreach (Group group in groups.Values)
                    {
                        if (group.ObjectSid == user.PrimaryGroupId &&
                            !group.GroupMembers.Contains(user))
                        {
                            group.GroupMembers.Add(user);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Logger.Log(string.Format("Primary group {0} not found for {1}", user.PrimaryGroupId.ToString(), user.UserId), Logger.LogType.WARN);
                    }
                }
            }

            return groups;
        }

        private List<Member> GetMembersFromGroup(LdapConnection conn, SearchResultEntry group, Dictionary<string, Group> groups, Dictionary<string, User> users)
        {
            List<Member> retVal = new List<Member>();
            if (group.Attributes.Contains("member"))
            {
                foreach (string member in group.Attributes["member"].GetValues(typeof(string)))
                {
                    if (groups.ContainsKey(member.ToString()))
                    {
                        retVal.Add(groups[member.ToString()]);
                    }
                    else if (users.ContainsKey(member.ToString()))
                    {
                        retVal.Add(users[member.ToString()]);
                    }
                    else
                    {
                        Logger.Log("Member not found in groups or users list: " + member.ToString(), Logger.LogType.DEBUG);
                    }
                }
            }

            return retVal;
        }

        private void LoadSettings()
        {
            _bindUser = SettingsReader.GetSettingString("bindUser", "", true);
            _bindPassword = SettingsReader.GetSettingString("bindPassword", "", true);
            _ldapHost = SettingsReader.GetSettingString("ldapHost", "", true);
            _baseQuery = SettingsReader.GetSettingString("baseQuery", "", true);
            _userIdAttribute = SettingsReader.GetSettingString("userIdAttribute", "", true);
        }
    }
}
