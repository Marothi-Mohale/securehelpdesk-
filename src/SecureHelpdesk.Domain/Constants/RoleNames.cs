namespace SecureHelpdesk.Domain.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Agent = "Agent";
    public const string User = "User";

    public static readonly string[] All = [Admin, Agent, User];
}
