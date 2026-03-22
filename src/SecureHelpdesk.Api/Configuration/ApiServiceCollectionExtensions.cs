using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Application.Common;

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
                        entry => string.IsNullOrWhiteSpace(entry.Key) ? "$" : entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                var payload = ApiErrorResponseFactory.CreateValidation(context.HttpContext, errors);
                return new BadRequestObjectResult(payload);
            };
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();

        return services;
    }
}
