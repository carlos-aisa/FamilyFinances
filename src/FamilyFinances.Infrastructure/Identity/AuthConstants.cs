namespace FamilyFinances.Infrastructure.Identity;

public static class AuthConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Reader = "Reader";
    }

    public static class Policies
    {
        public const string CanRead = "CanRead";
        public const string CanWrite = "CanWrite";
    }
}