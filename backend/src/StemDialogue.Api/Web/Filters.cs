using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using StemDialogue.Application.Abstractions;

namespace StemDialogue.Api.Web;

/// <summary>Runs the FluentValidation validator registered for each action argument before the action.</summary>
internal sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || argument is CancellationToken) continue;
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0) throw new ValidationException(failures);
        await next();
    }
}

/// <summary>After a successful CMS change, drops cached public responses so the site updates immediately.</summary>
internal sealed class PublicCacheEvictionFilter(IPublicCacheInvalidator cache) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method)) return;
        if (executed.Exception is not null && !executed.ExceptionHandled) return;

        await cache.InvalidateAsync(CancellationToken.None);
    }
}
