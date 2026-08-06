using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Classes;

public class UpdateClassCommandHandler(IAppDbContext dbContext) : IRequestHandler<UpdateClassCommand, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<ClassDto>("Class not found.");
        }

        classEntity.Update(request.Name, request.Description);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(classEntity.ToDto());
    }
}
