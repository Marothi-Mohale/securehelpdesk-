using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecureHelpdesk.Api.Swagger;

public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags =
        [
            new OpenApiTag
            {
                Name = "Auth",
                Description = "User registration, login, and current-user identity endpoints. Start here to obtain a JWT for secured ticket operations."
            },
            new OpenApiTag
            {
                Name = "Tickets",
                Description = "Ticket management endpoints covering creation, filtering, workflow updates, assignment, comments, and audit-aware detail retrieval."
            }
        ];
    }
}
