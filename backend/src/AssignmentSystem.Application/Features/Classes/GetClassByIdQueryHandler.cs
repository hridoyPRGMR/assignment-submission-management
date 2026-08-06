using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Classes;

public class GetClassByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetClassByIdQuery, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        var classEntity = await dbContext.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return classEntity is null
            ? Result.Failure<ClassDto>("Class not found.")
            : Result.Success(classEntity.ToDto());
    }
}
