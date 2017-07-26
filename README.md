# Background

We run Subversion on a Windows stack and I needed a tool to sync LDAP groups from our Active Directory server to the Subversion authz file.  I initially tried using Jeremy Whitlock's Python script sync_ldap_groups_to_svn_authz.py but had trouble getting it to work consistently (mainly the LDAP connection would just stop working for reasons I could not figure out at the time).  So starting from the logic in his script, I re-wrote it in C#.

# Credit

Obviously, thanks to Jeremy Whitlock for his Python implementation.

# Usage

All settings are defined in the associated config file and are hopefully self explanatory.

I set up a Scheduled Task to run this every two hours and it's been working great.