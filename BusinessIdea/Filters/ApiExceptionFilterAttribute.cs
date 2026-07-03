using BusinessIdea.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationException = BusinessIdea.Application.Common.Exceptions.ValidationException;

namespace BusinessIdea.Web.Filters;

/// <summary>
/// Translates Application-layer exceptions into proper HTTP responses so
/// controllers stay free of try/catch plumbing. Registered globally for MVC
/// controllers only (Razor Pages are unaffected).
/// </summary>
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly Dictionary<Type, Action<ExceptionContext>> _handlers;

    public ApiExceptionFilterAttribute()
    {
        _handlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException }
        };
    }

    public override void OnException(ExceptionContext context)
    {
        var type = context.Exception.GetType();
        if (_handlers.TryGetValue(type, out var handler))
        {
            handler.Invoke(context);
            context.ExceptionHandled = true;
        }

        base.OnException(context);
    }

    private static void HandleValidationException(ExceptionContext context)
    {
        var exception = (ValidationException)context.Exception;
        var details = new ValidationProblemDetails(
            exception.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };

        context.Result = new BadRequestObjectResult(details);
    }

    private static void HandleNotFoundException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "The requested resource was not found.",
            Detail = context.Exception.Message
        };

        context.Result = new NotFoundObjectResult(details);
    }

    private static void HandleForbiddenAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "You are not allowed to perform this action.",
            Detail = context.Exception.Message
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
