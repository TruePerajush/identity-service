using System.Collections.Concurrent;
using System.Reflection;
using FluentResults;
using FluentValidation;
using MediatR;

namespace IdentityService.Common;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> GenericFailMethodCache = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures
            .Select(f => (IError)new Error(f.ErrorMessage)
                .WithMetadata("Code", "Identity.ValidationFailed")
                .WithMetadata("Property", f.PropertyName))
            .ToList();

        return BuildFailedResult(errors);
    }

    private static TResponse BuildFailedResult(List<IError> errors)
    {
        if (!typeof(TResponse).IsGenericType)
        {
            if (typeof(TResponse) != typeof(Result))
                throw new InvalidOperationException(
                    $"ValidationBehavior expects TResponse to be Result or Result<T>, got {typeof(TResponse)}.");

            return (TResponse)(object)Result.Fail(errors);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failMethod = GenericFailMethodCache.GetOrAdd(valueType, t =>
            typeof(Result)
                .GetMethods()
                .Single(m => m.Name == nameof(Result.Fail)
                             && m.IsGenericMethodDefinition
                             && m.GetParameters().Length == 1
                             && m.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(List<IError>)))
                .MakeGenericMethod(t));

        return (TResponse)failMethod.Invoke(null, [errors])!;
    }
}