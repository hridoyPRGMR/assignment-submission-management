using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Subjects;

public class SubjectService(
    ISubjectRepository subjectRepository,
    IAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateSubjectInput> createValidator,
    IValidator<UpdateSubjectInput> updateValidator) : ISubjectService
{
    public async Task<Result<SubjectDto>> CreateAsync(CreateSubjectInput input, Guid actorUserId, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SubjectDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var subjectResult = Domain.Entities.Subject.Create(input.Name, input.Description, DateTime.UtcNow, actorUserId);
        if (subjectResult.IsFailure)
        {
            return Result.Failure<SubjectDto>(subjectResult.Error!);
        }

        subjectRepository.Add(subjectResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subjectResult.Value.ToDto());
    }

    public async Task<Result<SubjectDto>> UpdateAsync(UpdateSubjectInput input, Guid actorUserId, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SubjectDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var subject = await subjectRepository.Query().FirstOrDefaultAsync(s => s.Id == input.Id, cancellationToken);

        if (subject is null)
        {
            return Result.Failure<SubjectDto>("Subject not found.");
        }

        subject.Update(input.Name, input.Description, DateTime.UtcNow, actorUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subject.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.Query().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (subject is null)
        {
            return Result.Failure("Subject not found.");
        }

        var hasAssignments = await assignmentRepository.Query().AnyAsync(a => a.SubjectId == id, cancellationToken);

        if (hasAssignments)
        {
            return Result.Failure("Cannot delete a subject that has existing assignments.");
        }

        subjectRepository.Remove(subject);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<SubjectDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.Query().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return subject is null
            ? Result.Failure<SubjectDto>("Subject not found.")
            : Result.Success(subject.ToDto());
    }

    public async Task<Result<PagedResult<SubjectDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = subjectRepository.Query().AsNoTracking().OrderBy(s => s.Name);

        var paged = await query.ToPagedResultAsync(page, pageSize, cancellationToken);

        var dtoPage = new PagedResult<SubjectDto>(
            paged.Items.Select(s => s.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
