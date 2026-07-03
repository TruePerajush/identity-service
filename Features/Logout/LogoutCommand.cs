using FluentResults;
using MediatR;

namespace IdentityService.Features.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
public sealed record LogoutRequestDto(string RefreshToken);