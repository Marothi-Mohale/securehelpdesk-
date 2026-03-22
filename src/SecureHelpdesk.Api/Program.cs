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
    if (seedOptions.SeedOnStartup)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
    }
}

app.Run();
