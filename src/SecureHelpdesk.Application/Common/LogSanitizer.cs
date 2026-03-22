namespace SecureHelpdesk.Application.Common;

public static class LogSanitizer
{
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return "unknown";
        }

        var parts = email.Split('@', 2);
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
        {
            return $"**@{domain}";
        }

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }
}
