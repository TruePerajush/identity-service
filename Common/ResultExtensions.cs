using FluentResults;

namespace IdentityService.Common;

public static class ResultExtensions
{
    public static IResult ToProblem<T>(this Result<T> result)
    {
        var error = result.Errors.FirstOrDefault();
        var code = error?.Metadata.GetValueOrDefault("Code") as string ?? "Identity.Unknown";

        if (code == "Identity.ValidationFailed")
        {
            var errors = result.Errors.Select(e => new
            {
                message = e.Message,
                property = e.Metadata.GetValueOrDefault("Property") as string
            });
            return Results.ValidationProblem(
                errors: errors.GroupBy(e => e.property ?? "").ToDictionary(g => g.Key, g => g.Select(e => e.message).ToArray()));
        }

        if (code == "Identity.AccountLocked")
        {
            var lockedUntil = error?.Metadata.GetValueOrDefault("LockedUntil");
            return Results.Problem(detail: error?.Message, statusCode: StatusCodes.Status423Locked,
                extensions: new Dictionary<string, object?> { ["code"] = code, ["lockedUntil"] = lockedUntil });
        }

        var status = code switch
        {
            "Identity.EmailAlreadyExists" => StatusCodes.Status409Conflict,
            "Identity.InvalidCredentials" => StatusCodes.Status401Unauthorized,
            "Identity.RefreshTokenInvalid" => StatusCodes.Status401Unauthorized,
            "Identity.UserInactive" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(detail: error?.Message, statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
    }

    public static IResult ToProblem(this Result result)
    {
        var error = result.Errors.FirstOrDefault();
        var code = error?.Metadata.GetValueOrDefault("Code") as string ?? "Identity.Unknown";

        var status = code switch
        {
            "Identity.RefreshTokenInvalid" => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(detail: error?.Message, statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
    }
}