using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Application.Features.Assignments;

public class AssignmentService(
    IAssignmentRepository assignmentRepository,
    ITeacherAssignmentRepository teacherAssignmentRepository,
    ISubmissionRepository submissionRepository,
    IStudentEnrollmentRepository studentEnrollmentRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateAssignmentInput> createValidator,
    IValidator<UpdateAssignmentInput> updateValidator,
    ILogger<AssignmentService> logger) : IAssignmentService
{
    public async Task<Result<AssignmentDto>> CreateAsync(CreateAssignmentInput input, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AssignmentDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var isAssignedToTeach = await teacherAssignmentRepository.Query().AnyAsync(
            ta => ta.TeacherUserId == input.TeacherUserId && ta.ClassId == input.ClassId && ta.SubjectId == input.SubjectId,
            cancellationToken);

        if (!isAssignedToTeach)
        {
            return Result.Forbidden<AssignmentDto>("You are not assigned to teach this subject in this class.");
        }

        var assignmentResult = Domain.Entities.Assignment.Create(
            input.Title,
            input.Description,
            input.ClassId,
            input.SubjectId,
            input.TeacherUserId,
            input.DeadlineUtc,
            input.MaxMarks,
            DateTime.UtcNow);

        if (assignmentResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(assignmentResult.Error!);
        }

        assignmentRepository.Add(assignmentResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await assignmentRepository.Query()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstAsync(a => a.Id == assignmentResult.Value.Id, cancellationToken);

        logger.LogInformation(
            "Teacher {TeacherUserId} created assignment {AssignmentId} for class {ClassId}/subject {SubjectId}",
            input.TeacherUserId, created.Id, input.ClassId, input.SubjectId);

        return Result.Success(created.ToDto());
    }

    public async Task<Result<AssignmentDto>> UpdateAsync(UpdateAssignmentInput input, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AssignmentDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var assignment = await assignmentRepository.Query()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == input.Id, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound<AssignmentDto>("Assignment not found.");
        }

        if (assignment.TeacherUserId != input.RequestingUserId)
        {
            return Result.Forbidden<AssignmentDto>("You do not own this assignment.");
        }

        var hasSubmissions = await submissionRepository.Query().AnyAsync(s => s.AssignmentId == input.Id, cancellationToken);

        var updateResult = assignment.UpdateDetails(
            input.Title, input.Description, input.DeadlineUtc, input.MaxMarks, hasSubmissions, DateTime.UtcNow, input.RequestingUserId);

        if (updateResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(updateResult.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(assignment.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound("Assignment not found.");
        }

        if (assignment.TeacherUserId != requestingUserId)
        {
            return Result.Forbidden("You do not own this assignment.");
        }

        assignment.MarkDeleted(DateTime.UtcNow, requestingUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Teacher {TeacherUserId} deleted assignment {AssignmentId}", requestingUserId, id);

        return Result.Success();
    }

    public async Task<Result<AssignmentDto>> PublishAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound<AssignmentDto>("Assignment not found.");
        }

        if (assignment.TeacherUserId != requestingUserId)
        {
            return Result.Forbidden<AssignmentDto>("You do not own this assignment.");
        }

        var publishResult = assignment.Publish(DateTime.UtcNow, requestingUserId);
        if (publishResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(publishResult.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Teacher {TeacherUserId} published assignment {AssignmentId}", requestingUserId, id);

        return Result.Success(assignment.ToDto());
    }

    public async Task<Result<AssignmentDto>> UnpublishAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound<AssignmentDto>("Assignment not found.");
        }

        if (assignment.TeacherUserId != requestingUserId)
        {
            return Result.Forbidden<AssignmentDto>("You do not own this assignment.");
        }

        var unpublishResult = assignment.Unpublish(DateTime.UtcNow, requestingUserId);
        if (unpublishResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(unpublishResult.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Teacher {TeacherUserId} unpublished assignment {AssignmentId}", requestingUserId, id);

        return Result.Success(assignment.ToDto());
    }

    public async Task<Result<AssignmentDto>> MarkGradedAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound<AssignmentDto>("Assignment not found.");
        }

        if (assignment.TeacherUserId != requestingUserId)
        {
            return Result.Forbidden<AssignmentDto>("You do not own this assignment.");
        }

        var result = assignment.MarkGraded(DateTime.UtcNow, requestingUserId);
        if (result.IsFailure)
        {
            return Result.Failure<AssignmentDto>(result.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Teacher {TeacherUserId} marked assignment {AssignmentId} as graded", requestingUserId, id);

        return Result.Success(assignment.ToDto());
    }

    public async Task<Result<AssignmentDto>> ArchiveAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound<AssignmentDto>("Assignment not found.");
        }

        if (assignment.TeacherUserId != requestingUserId)
        {
            return Result.Forbidden<AssignmentDto>("You do not own this assignment.");
        }

        var result = assignment.Archive(DateTime.UtcNow, requestingUserId);
        if (result.IsFailure)
        {
            return Result.Failure<AssignmentDto>(result.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Teacher {TeacherUserId} archived assignment {AssignmentId}", requestingUserId, id);

        return Result.Success(assignment.ToDto());
    }

    public async Task<Result<AssignmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query()
            .AsNoTracking()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return assignment is null
            ? Result.NotFound<AssignmentDto>("Assignment not found.")
            : Result.Success(assignment.ToDto());
    }

    public async Task<Result<PagedResult<AssignmentDto>>> GetListAsync(
        Guid? teacherUserId, Guid? classId, AssignmentStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = assignmentRepository.Query()
            .AsNoTracking()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .AsQueryable();

        if (teacherUserId.HasValue)
        {
            query = query.Where(a => a.TeacherUserId == teacherUserId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(a => a.ClassId == classId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        query = query.OrderByDescending(a => a.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(page, pageSize, cancellationToken);

        var dtoPage = new PagedResult<AssignmentDto>(
            paged.Items.Select(a => a.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }

    public async Task<Result<PagedResult<AssignmentDto>>> GetPublishedForStudentAsync(
        Guid studentUserId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var classIds = await studentEnrollmentRepository.Query()
            .Where(e => e.StudentUserId == studentUserId)
            .Select(e => e.ClassId)
            .ToListAsync(cancellationToken);

        var query = assignmentRepository.Query()
            .AsNoTracking()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .Where(a => classIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published)
            .OrderByDescending(a => a.DeadlineUtc);

        var paged = await query.ToPagedResultAsync(page, pageSize, cancellationToken);

        var dtoPage = new PagedResult<AssignmentDto>(
            paged.Items.Select(a => a.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
