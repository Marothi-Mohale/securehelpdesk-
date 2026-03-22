using SecureHelpdesk.Api.Configuration;
using SecureHelpdesk.Api.Middleware;
using SecureHelpdesk.Infrastructure.Configuration;
using SecureHelpdesk.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiLogging();
builder.Services.AddApiServices();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Secure Helpdesk API Docs";
        options.DisplayRequestDuration();
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(2);
        options.EnablePersistAuthorization();
    });

    app.MapGet("/", () => Results.Redirect("/demo/index.html"))
        .ExcludeFromDescription();

    app.MapGet("/demo", () => Results.Redirect("/demo/index.html"))
        .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
    var canSeedInEnvironment = app.Environment.IsDevelopment() || seedOptions.AllowInNonDevelopment;
    if (seedOptions.SeedOnStartup && canSeedInEnvironment)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
    }
    else if (seedOptions.SeedOnStartup && !canSeedInEnvironment)
    {
        app.Logger.LogWarning(
            "Demo data seeding was requested but skipped because the environment is {EnvironmentName}. Set {Section}:{Setting} to true to allow seeding outside Development.",
            app.Environment.EnvironmentName,
            SeedOptions.SectionName,
            nameof(SeedOptions.AllowInNonDevelopment));
    }
}

app.Run();

public partial class Program;
