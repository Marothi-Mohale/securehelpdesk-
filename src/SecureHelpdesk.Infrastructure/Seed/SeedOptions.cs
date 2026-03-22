namespace SecureHelpdesk.Infrastructure.Seed;

public class SeedOptions
{
    public const string SectionName = "DemoData";

    public bool SeedOnStartup { get; init; } = true;
    public bool AllowInNonDevelopment { get; init; }
    public string DefaultPassword { get; init; } = "LocalDev123!";
    public string? AdminPassword { get; init; }
    public string? AgentPassword { get; init; }
    public string? UserPassword { get; init; }
}
