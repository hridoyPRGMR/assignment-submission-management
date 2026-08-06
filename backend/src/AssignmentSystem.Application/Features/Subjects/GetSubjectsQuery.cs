using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Subjects;

public record GetSubjectsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<SubjectDto>>>;
