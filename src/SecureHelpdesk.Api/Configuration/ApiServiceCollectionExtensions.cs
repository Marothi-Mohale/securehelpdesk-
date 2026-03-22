using Microsoft.AspNetCore.Mvc;

namespace SecureHelpdesk.Api.Configuration;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                return new BadRequestObjectResult(new
                {
                    message = "Validation failed.",
                    errors
                });
            };
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();

        return services;
    }
}
