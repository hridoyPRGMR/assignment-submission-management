using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
