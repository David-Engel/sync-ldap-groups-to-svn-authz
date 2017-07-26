using System;

namespace SvnSyncAuthzLdap
{
    public class Member : IComparable<Member>
    {
        public string DistinguishedName { get; set; }
        public string CN { get; set; }

        public int CompareTo(Member otherMember)
        {
            if (this is Group && otherMember is Group)
            {
                return ((Group)this).CompareTo((Group)otherMember);
            }
            else if (this is User && otherMember is User)
            {
                return ((User)this).CompareTo((User)otherMember);
            }
            else if (this.CN == null && otherMember.CN == null)
            {
                return 0;
            }
            else if (this.CN == null)
            {
                return -1;
            }
            else if (otherMember.CN == null)
            {
                return 1;
            }
            else
            {
                return this.CN.CompareTo(otherMember.CN);
            }
        }
    }
}
