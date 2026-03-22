using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Api.Extensions;
using SecureHelpdesk.Application.Common;

namespace SecureHelpdesk.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected UserContext CurrentUser => User.ToUserContext();
}
