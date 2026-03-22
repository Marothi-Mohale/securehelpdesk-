using System.Reflection;
using Microsoft.OpenApi.Models;
using SecureHelpdesk.Api.Swagger;
using SecureHelpdesk.Application.DTOs.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecureHelpdesk.Api.Configuration;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Secure Helpdesk API",
                Version = "v1",
                Description = "Secure Helpdesk / Ticketing API for internal support workflows. Explore JWT authentication, role-based access, ticket lifecycle management, comments, audit history, filtering, pagination, and standardized error handling."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste a JWT access token here using the Bearer scheme. Obtain a token from `POST /api/auth/login` or `POST /api/auth/register` first."
            });

            options.SupportNonNullableReferenceTypes();
            options.CustomOperationIds(apiDescription =>
            {
                var controller = apiDescription.ActionDescriptor.RouteValues["controller"];
                return $"{controller}_{apiDescription.HttpMethod}_{apiDescription.RelativePath}"
                    .Replace("/", "_")
                    .Replace("{", string.Empty)
                    .Replace("}", string.Empty)
                    .Replace(":", "_");
            });
            options.OrderActionsBy(apiDescription =>
                $"{apiDescription.ActionDescriptor.RouteValues["controller"]}_{apiDescription.HttpMethod}_{apiDescription.RelativePath}");

            options.OperationFilter<AuthorizeOperationFilter>();
            options.SchemaFilter<ExampleSchemaFilter>();
            options.DocumentFilter<TagDescriptionsDocumentFilter>();

            IncludeXmlComments(options, Assembly.GetExecutingAssembly());
            IncludeXmlComments(options, typeof(RegisterRequestDto).Assembly);
        });

        return services;
    }

    private static void IncludeXmlComments(SwaggerGenOptions options, Assembly assembly)
    {
        var xmlFile = $"{assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }
}
