namespace SecureHelpdesk.Application.Common;

public static class ErrorCodes
{
    public const string BadRequest = "bad_request";
    public const string Conflict = "conflict";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string ServerError = "server_error";
    public const string Unauthorized = "unauthorized";
    public const string ValidationFailed = "validation_failed";

    public const string AuthInvalidCredentials = "auth_invalid_credentials";
    public const string AuthUserNotFound = "auth_user_not_found";
    public const string TicketAlreadyAssigned = "ticket_already_assigned";
    public const string TicketInvalidStatusTransition = "ticket_invalid_status_transition";
    public const string TicketNoChangesDetected = "ticket_no_changes_detected";
    public const string TicketNotFound = "ticket_not_found";
    public const string TicketUnauthorizedAccess = "ticket_unauthorized_access";
    public const string UserAlreadyExists = "user_already_exists";
    public const string UserNotAgent = "user_not_agent";
    public const string UserNotFound = "user_not_found";
}
