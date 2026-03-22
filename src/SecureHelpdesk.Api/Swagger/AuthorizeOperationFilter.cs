using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecureHelpdesk.Api.Swagger;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
            || context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true;

        if (hasAllowAnonymous)
        {
            return;
        }

        var authorizeAttributes = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().ToArray();
        var controllerAuthorizeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().ToArray()
            ?? [];

        if (authorizeAttributes.Length == 0 && controllerAuthorizeAttributes.Length == 0)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Authentication is required or the bearer token is invalid." });

        var effectiveAuthorizeAttributes = authorizeAttributes.Length > 0 ? authorizeAttributes : controllerAuthorizeAttributes;
        if (effectiveAuthorizeAttributes.Any(attribute => !string.IsNullOrWhiteSpace(attribute.Roles)))
        {
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "The authenticated user does not have permission to perform this action." });
        }
    }
}
