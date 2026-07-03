using FluentResults;
using IdentityService.Features.Login;
using MediatR;

namespace IdentityService.Features.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, string? Ip) : IRequest<Result<TokenResponse>>;
public sealed record RefreshTokenRequestDto(string RefreshToken);
