using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Classes;

public class ClassService(
    IClassRepository classRepository,
    IAssignmentRepository assignmentRepository,
    IStudentEnrollmentRepository studentEnrollmentRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateClassInput> createValidator,
    IValidator<UpdateClassInput> updateValidator) : IClassService
{
    public async Task<Result<ClassDto>> CreateAsync(CreateClassInput input, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<ClassDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var classResult = Domain.Entities.Class.Create(input.Name, input.Description, DateTime.UtcNow);
        if (classResult.IsFailure)
        {
            return Result.Failure<ClassDto>(classResult.Error!);
        }

        classRepository.Add(classResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(classResult.Value.ToDto());
    }

    public async Task<Result<ClassDto>> UpdateAsync(UpdateClassInput input, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<ClassDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var classEntity = await classRepository.Query().FirstOrDefaultAsync(c => c.Id == input.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<ClassDto>("Class not found.");
        }

        classEntity.Update(input.Name, input.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(classEntity.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var classEntity = await classRepository.Query().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure("Class not found.");
        }

        var hasAssignments = await assignmentRepository.Query().AnyAsync(a => a.ClassId == id, cancellationToken);
        var hasEnrollments = await studentEnrollmentRepository.Query().AnyAsync(e => e.ClassId == id, cancellationToken);

        if (hasAssignments || hasEnrollments)
        {
            return Result.Failure("Cannot delete a class that has assignments or enrolled students.");
        }

        classRepository.Remove(classEntity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ClassDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var classEntity = await classRepository.Query().AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return classEntity is null
            ? Result.Failure<ClassDto>("Class not found.")
            : Result.Success(classEntity.ToDto());
    }

    public async Task<Result<PagedResult<ClassDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = classRepository.Query().AsNoTracking().OrderBy(c => c.Name);

        var paged = await query.ToPagedResultAsync(page, pageSize, cancellationToken);

        var dtoPage = new PagedResult<ClassDto>(
            paged.Items.Select(c => c.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
