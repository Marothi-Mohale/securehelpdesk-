namespace SecureHelpdesk.Infrastructure.Seed;

public class SeedOptions
{
    public const string SectionName = "DemoData";

    public bool SeedOnStartup { get; init; } = true;
}
