using FluentResults;
using MediatR;

namespace IdentityService.Features.Login;

public sealed record LoginCommand(string Email, string Password, string? Ip)
    : IRequest<Result<TokenResponse>>;
public sealed record LoginRequestDto(string Email, string Password);
public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
