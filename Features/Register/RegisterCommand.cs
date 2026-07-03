
using FluentResults;
using MediatR;

namespace IdentityService.Features.Register;

public sealed record RegisterCommand(string Email, string Password, string Name)
    : IRequest<Result<RegisterResponse>>;

public sealed record RegisterRequestDto(string Email, string Password, string Name);
public sealed record RegisterResponse(Guid UserId, string Email);