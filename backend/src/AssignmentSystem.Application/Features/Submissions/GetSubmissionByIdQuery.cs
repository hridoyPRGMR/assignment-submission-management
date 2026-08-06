using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record GetSubmissionByIdQuery(Guid Id) : IRequest<Result<SubmissionDto>>;
