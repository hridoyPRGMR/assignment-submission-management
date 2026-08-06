using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
