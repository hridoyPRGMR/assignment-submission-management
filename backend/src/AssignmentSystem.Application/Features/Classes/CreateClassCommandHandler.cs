using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Classes;

public class CreateClassCommandHandler(IAppDbContext dbContext) : IRequestHandler<CreateClassCommand, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var classResult = Domain.Entities.Class.Create(request.Name, request.Description, DateTime.UtcNow);
        if (classResult.IsFailure)
        {
            return Result.Failure<ClassDto>(classResult.Error!);
        }

        dbContext.Classes.Add(classResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(classResult.Value.ToDto());
    }
}
