using Microsoft.Extensions.Logging.Console;

namespace SecureHelpdesk.Api.Configuration;

public static class LoggingConfigurationExtensions
{
    public static WebApplicationBuilder AddApiLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.ColorBehavior = LoggerColorBehavior.Enabled;
            options.IncludeScopes = true;
        });

        return builder;
    }
}
