using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Classes;

public record GetClassesQuery(int Page, int PageSize) : IRequest<Result<PagedResult<ClassDto>>>;
