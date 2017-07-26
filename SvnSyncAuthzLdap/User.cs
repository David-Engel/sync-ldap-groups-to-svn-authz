using System;

namespace SvnSyncAuthzLdap
{
    public class User : Member, IComparable<User>
    {
        public string UserId { get; set; }
        public int PrimaryGroupId { get; set; }

        public int CompareTo(User otherUser)
        {
            if (this.UserId == null && otherUser.UserId == null)
            {
                return base.CompareTo(otherUser);
            }
            else if (this.UserId == null)
            {
                return -1;
            }
            else if (otherUser.UserId == null)
            {
                return 1;
            }
            else
            {
                return this.UserId.CompareTo(otherUser.UserId);
            }
        }
    }
}
