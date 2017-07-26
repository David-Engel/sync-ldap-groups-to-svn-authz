using System;
using System.Collections.Generic;

namespace SvnSyncAuthzLdap
{
    public class Group : Member, IComparable<Group>
    {
        private List<Member> _groupMembers = new List<Member>();
        public string FormattedGroupName { get; set; }
        public int ObjectSid { get; set; }
        public List<Member> GroupMembers
        {
            get;
            set;
        }

        public int CompareTo(Group otherGroup)
        {
            if (this.FormattedGroupName == null && otherGroup.FormattedGroupName == null)
            {
                return base.CompareTo(otherGroup);
            }
            else if (this.FormattedGroupName == null)
            {
                return -1;
            }
            else if (otherGroup.FormattedGroupName == null)
            {
                return 1;
            }
            else
            {
                return this.FormattedGroupName.CompareTo(otherGroup.FormattedGroupName);
            }
        }
    }
}
