using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SecureHelpdesk.Application.DTOs.Auth;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecureHelpdesk.Api.Swagger;

public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Example = context.Type switch
        {
            var type when type == typeof(RegisterRequestDto) => new OpenApiObject
            {
                ["fullName"] = new OpenApiString("Jordan Smith"),
                ["email"] = new OpenApiString("jordan.smith@securehelpdesk.local"),
                ["password"] = new OpenApiString("UserDemo123!")
            },
            var type when type == typeof(LoginRequestDto) => new OpenApiObject
            {
                ["email"] = new OpenApiString("admin@securehelpdesk.local"),
                ["password"] = new OpenApiString("AdminDemo123!")
            },
            var type when type == typeof(UserProfileDto) => new OpenApiObject
            {
                ["id"] = new OpenApiString("4d5f7eb2-0f7f-4ce7-9f3d-17b5d0d96831"),
                ["fullName"] = new OpenApiString("System Admin"),
                ["email"] = new OpenApiString("admin@securehelpdesk.local"),
                ["roles"] = new OpenApiArray
                {
                    new OpenApiString("Admin")
                }
            },
            var type when type == typeof(AuthResponseDto) => new OpenApiObject
            {
                ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.demo-token"),
                ["tokenType"] = new OpenApiString("Bearer"),
                ["expiresAtUtc"] = new OpenApiString("2026-03-22T16:45:00Z"),
                ["user"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("4d5f7eb2-0f7f-4ce7-9f3d-17b5d0d96831"),
                    ["fullName"] = new OpenApiString("System Admin"),
                    ["email"] = new OpenApiString("admin@securehelpdesk.local"),
                    ["roles"] = new OpenApiArray
                    {
                        new OpenApiString("Admin")
                    }
                }
            },
            var type when type == typeof(CreateTicketRequestDto) => new OpenApiObject
            {
                ["title"] = new OpenApiString("Cannot access payroll portal"),
                ["description"] = new OpenApiString("After MFA completes, the payroll portal returns an unauthorized error for my account."),
                ["priority"] = new OpenApiString("High")
            },
            var type when type == typeof(UpdateTicketRequestDto) => new OpenApiObject
            {
                ["title"] = new OpenApiString("Cannot access payroll portal after MFA"),
                ["description"] = new OpenApiString("User confirms the issue started immediately after a password reset."),
                ["priority"] = new OpenApiString("Critical")
            },
            var type when type == typeof(UpdateTicketStatusRequestDto) => new OpenApiObject
            {
                ["status"] = new OpenApiString("Resolved")
            },
            var type when type == typeof(AssignTicketRequestDto) => new OpenApiObject
            {
                ["agentUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210")
            },
            var type when type == typeof(AddCommentRequestDto) => new OpenApiObject
            {
                ["content"] = new OpenApiString("I have reviewed the SSO configuration and I am waiting on the identity sync to complete.")
            },
            var type when type == typeof(TicketListQueryRequestDto) => new OpenApiObject
            {
                ["search"] = new OpenApiString("payroll"),
                ["status"] = new OpenApiString("InProgress"),
                ["priority"] = new OpenApiString("High"),
                ["assignedToUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                ["createdByUserId"] = new OpenApiString("4f4ddf06-8e23-4ee2-b44f-85e2ff795d3d"),
                ["sortBy"] = new OpenApiString("UpdatedAt"),
                ["sortDirection"] = new OpenApiString("Desc"),
                ["pageNumber"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(10)
            },
            var type when type == typeof(CommentResponseDto) => new OpenApiObject
            {
                ["id"] = new OpenApiString("2afdd2a3-2c9e-4607-8c70-f660763133e5"),
                ["content"] = new OpenApiString("I have reviewed the SSO configuration and I am waiting on the identity sync to complete."),
                ["createdAtUtc"] = new OpenApiString("2026-03-22T14:10:00Z"),
                ["authorUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                ["authorName"] = new OpenApiString("Alice Agent")
            },
            var type when type == typeof(AuditLogResponseDto) => new OpenApiObject
            {
                ["id"] = new OpenApiString("d99f3454-9f24-4a0f-9a5f-ebeb44b5cdd8"),
                ["actionType"] = new OpenApiString("StatusChanged"),
                ["oldValue"] = new OpenApiString("Open"),
                ["newValue"] = new OpenApiString("InProgress"),
                ["description"] = new OpenApiString("Status changed from Open to InProgress"),
                ["changedByUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                ["changedByName"] = new OpenApiString("Alice Agent"),
                ["changedAtUtc"] = new OpenApiString("2026-03-22T13:30:00Z")
            },
            var type when type == typeof(TicketListResponseDto) => new OpenApiObject
            {
                ["id"] = new OpenApiString("8d2feee2-a92c-4568-af4a-8f09c3991137"),
                ["title"] = new OpenApiString("Cannot access payroll portal"),
                ["priority"] = new OpenApiString("High"),
                ["status"] = new OpenApiString("InProgress"),
                ["createdByUserId"] = new OpenApiString("4f4ddf06-8e23-4ee2-b44f-85e2ff795d3d"),
                ["createdByName"] = new OpenApiString("Uma User"),
                ["assignedToUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                ["assignedToName"] = new OpenApiString("Alice Agent"),
                ["commentCount"] = new OpenApiInteger(2),
                ["createdAtUtc"] = new OpenApiString("2026-03-18T08:00:00Z"),
                ["updatedAtUtc"] = new OpenApiString("2026-03-20T09:30:00Z")
            },
            var type when type == typeof(TicketResponseDto) => new OpenApiObject
            {
                ["id"] = new OpenApiString("8d2feee2-a92c-4568-af4a-8f09c3991137"),
                ["title"] = new OpenApiString("Cannot access payroll portal"),
                ["description"] = new OpenApiString("After MFA completes, the payroll portal returns an unauthorized error for my account."),
                ["priority"] = new OpenApiString("High"),
                ["status"] = new OpenApiString("InProgress"),
                ["createdByUserId"] = new OpenApiString("4f4ddf06-8e23-4ee2-b44f-85e2ff795d3d"),
                ["createdByName"] = new OpenApiString("Uma User"),
                ["assignedToUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                ["assignedToName"] = new OpenApiString("Alice Agent"),
                ["createdAtUtc"] = new OpenApiString("2026-03-18T08:00:00Z"),
                ["updatedAtUtc"] = new OpenApiString("2026-03-20T09:30:00Z"),
                ["comments"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("2afdd2a3-2c9e-4607-8c70-f660763133e5"),
                        ["content"] = new OpenApiString("Issue started immediately after my password reset."),
                        ["createdAtUtc"] = new OpenApiString("2026-03-18T08:15:00Z"),
                        ["authorUserId"] = new OpenApiString("4f4ddf06-8e23-4ee2-b44f-85e2ff795d3d"),
                        ["authorName"] = new OpenApiString("Uma User")
                    }
                },
                ["auditLogs"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("d99f3454-9f24-4a0f-9a5f-ebeb44b5cdd8"),
                        ["actionType"] = new OpenApiString("StatusChanged"),
                        ["oldValue"] = new OpenApiString("Open"),
                        ["newValue"] = new OpenApiString("InProgress"),
                        ["description"] = new OpenApiString("Status changed from Open to InProgress"),
                        ["changedByUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                        ["changedByName"] = new OpenApiString("Alice Agent"),
                        ["changedAtUtc"] = new OpenApiString("2026-03-20T09:30:00Z")
                    }
                }
            },
            var type when type == typeof(ApiErrorResponseDto) => new OpenApiObject
            {
                ["status"] = new OpenApiInteger(404),
                ["title"] = new OpenApiString("Not Found"),
                ["detail"] = new OpenApiString("Ticket was not found."),
                ["errorCode"] = new OpenApiString("not_found"),
                ["traceId"] = new OpenApiString("0HNBP23Q4H6E7:00000001"),
                ["path"] = new OpenApiString("/api/tickets/8d2feee2-a92c-4568-af4a-8f09c3991137"),
                ["timestampUtc"] = new OpenApiString("2026-03-22T14:40:00Z")
            },
            var type when type == typeof(ValidationErrorResponseDto) => new OpenApiObject
            {
                ["status"] = new OpenApiInteger(400),
                ["title"] = new OpenApiString("Validation Failed"),
                ["detail"] = new OpenApiString("One or more validation errors occurred."),
                ["errorCode"] = new OpenApiString("validation_failed"),
                ["traceId"] = new OpenApiString("0HNBP23Q4H6E7:00000002"),
                ["path"] = new OpenApiString("/api/tickets"),
                ["timestampUtc"] = new OpenApiString("2026-03-22T14:41:00Z"),
                ["errors"] = new OpenApiObject
                {
                    ["Title"] = new OpenApiArray
                    {
                        new OpenApiString("Title must be between 5 and 200 characters.")
                    },
                    ["Description"] = new OpenApiArray
                    {
                        new OpenApiString("Description must be between 10 and 4000 characters.")
                    }
                }
            },
            _ when IsPaginatedTicketListResponse(context.Type) => new OpenApiObject
            {
                ["items"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("8d2feee2-a92c-4568-af4a-8f09c3991137"),
                        ["title"] = new OpenApiString("Cannot access payroll portal"),
                        ["priority"] = new OpenApiString("High"),
                        ["status"] = new OpenApiString("InProgress"),
                        ["createdByUserId"] = new OpenApiString("4f4ddf06-8e23-4ee2-b44f-85e2ff795d3d"),
                        ["createdByName"] = new OpenApiString("Uma User"),
                        ["assignedToUserId"] = new OpenApiString("9af6f7a0-f187-4d89-bbb8-fddbc5e64210"),
                        ["assignedToName"] = new OpenApiString("Alice Agent"),
                        ["commentCount"] = new OpenApiInteger(2),
                        ["createdAtUtc"] = new OpenApiString("2026-03-18T08:00:00Z"),
                        ["updatedAtUtc"] = new OpenApiString("2026-03-20T09:30:00Z")
                    }
                },
                ["pageNumber"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(10),
                ["totalCount"] = new OpenApiInteger(6),
                ["totalPages"] = new OpenApiInteger(1)
            },
            _ => schema.Example
        };
    }

    private static bool IsPaginatedTicketListResponse(Type type)
    {
        return type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(PaginatedResponseDto<>)
            && type.GetGenericArguments()[0] == typeof(TicketListResponseDto);
    }
}
