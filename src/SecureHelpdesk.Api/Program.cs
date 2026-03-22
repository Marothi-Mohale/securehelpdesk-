using SecureHelpdesk.Api.Configuration;
using SecureHelpdesk.Api.Middleware;
using SecureHelpdesk.Infrastructure.Configuration;
using SecureHelpdesk.Infrastructure.Seed;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiLogging();
builder.Services.AddApiServices();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

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
