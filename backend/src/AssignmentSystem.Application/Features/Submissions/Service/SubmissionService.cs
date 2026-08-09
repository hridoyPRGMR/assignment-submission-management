using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Application.Features.Submissions;

public class SubmissionService(
    ISubmissionRepository submissionRepository,
    IAssignmentRepository assignmentRepository,
    IStudentEnrollmentRepository studentEnrollmentRepository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    IValidator<CreateSubmissionInput> createValidator,
    IValidator<GradeSubmissionInput> gradeValidator,
    IValidator<ReturnSubmissionForRevisionInput> returnForRevisionValidator,
    IValidator<UpdateSubmissionInput> updateValidator,
    ILogger<SubmissionService> logger) : ISubmissionService
{
    public async Task<Result<SubmissionDto>> CreateAsync(CreateSubmissionInput input, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SubmissionDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var assignment = await assignmentRepository.Query().FirstOrDefaultAsync(a => a.Id == input.AssignmentId, cancellationToken);

        if (assignment is null || assignment.Status != AssignmentStatus.Published)
        {
            return Result.NotFound<SubmissionDto>("Assignment not found.");
        }

        var isEnrolled = await studentEnrollmentRepository.Query().AnyAsync(
            e => e.StudentUserId == input.StudentUserId && e.ClassId == assignment.ClassId, cancellationToken);

        if (!isEnrolled)
        {
            return Result.Forbidden<SubmissionDto>("You are not enrolled in the class this assignment belongs to.");
        }

        var alreadySubmitted = await submissionRepository.Query().AnyAsync(
            s => s.AssignmentId == input.AssignmentId && s.StudentUserId == input.StudentUserId, cancellationToken);

        if (alreadySubmitted)
        {
            return Result.Failure<SubmissionDto>("You have already submitted this assignment.");
        }

        var submissionResult = Domain.Entities.Submission.Create(
            input.AssignmentId, input.StudentUserId, input.Content,
            input.FileName, input.StoredFileName, input.FileContentType, input.FileSizeBytes,
            assignment.DeadlineUtc, DateTime.UtcNow);

        if (submissionResult.IsFailure)
        {
            return Result.Failure<SubmissionDto>(submissionResult.Error!);
        }

        submissionRepository.Add(submissionResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await submissionRepository.Query()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstAsync(s => s.Id == submissionResult.Value.Id, cancellationToken);

        logger.LogInformation(
            "Student {StudentUserId} submitted assignment {AssignmentId} (submission {SubmissionId}, late: {IsLate})",
            input.StudentUserId, input.AssignmentId, created.Id, created.IsLate);

        return Result.Success(created.ToDto());
    }

    public async Task<Result<SubmissionDto>> UpdateAsync(UpdateSubmissionInput input, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SubmissionDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var submission = await submissionRepository.Query()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == input.Id, cancellationToken);

        if (submission is null)
        {
            return Result.NotFound<SubmissionDto>("Submission not found.");
        }

        if (submission.StudentUserId != input.RequestingStudentUserId)
        {
            return Result.Forbidden<SubmissionDto>("You do not own this submission.");
        }

        var updateResult = submission.Update(
            input.Content, input.ReplaceAttachment, input.FileName, input.StoredFileName, input.FileContentType, input.FileSizeBytes,
            submission.Assignment.DeadlineUtc, submission.Assignment.Status, DateTime.UtcNow, input.RequestingStudentUserId);

        if (updateResult.IsFailure)
        {
            return Result.Failure<SubmissionDto>(updateResult.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (updateResult.Value is { } orphanedStoredFileName)
        {
            fileStorageService.Delete(orphanedStoredFileName);
        }

        logger.LogInformation(
            "Student {StudentUserId} updated submission {SubmissionId} (late: {IsLate})",
            input.RequestingStudentUserId, submission.Id, submission.IsLate);

        return Result.Success(submission.ToDto());
    }

    public async Task<Result<SubmissionDto>> GradeAsync(GradeSubmissionInput input, CancellationToken cancellationToken)
    {
        var validation = await gradeValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SubmissionDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var submission = await submissionRepository.Query()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == input.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.NotFound<SubmissionDto>("Submission not found.");
        }

        if (submission.Assignment.TeacherUserId != input.RequestingTeacherUserId)
        {
            return Result.Forbidden<SubmissionDto>("You do not own this assignment.");
        }

        var result = submission.Grade(
            input.Marks, input.Feedback, submission.Assignment.MaxMarks, input.RequestingTeacherUserId, DateTime.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<SubmissionDto>(result.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Teacher {TeacherUserId} graded submission {SubmissionId} with {Marks}/{MaxMarks}",
            input.RequestingTeacherUserId, submission.Id, input.Marks, submission.Assignment.MaxMarks);

        return Result.Success(submission.ToDto());
    }

    public async Task<Result<SubmissionDto>> ReturnForRevisionAsync(
        ReturnSubmissionForRevisionInput input, CancellationToken cancellationToken)
    {
        var validation = await returnForRevisionValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SubmissionDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var submission = await submissionRepository.Query()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == input.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.NotFound<SubmissionDto>("Submission not found.");
        }

        if (submission.Assignment.TeacherUserId != input.RequestingTeacherUserId)
        {
            return Result.Forbidden<SubmissionDto>("You do not own this assignment.");
        }

        var result = submission.Return(input.Feedback, DateTime.UtcNow, input.RequestingTeacherUserId);
        if (result.IsFailure)
        {
            return Result.Failure<SubmissionDto>(result.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Teacher {TeacherUserId} returned submission {SubmissionId} for revision",
            input.RequestingTeacherUserId, submission.Id);

        return Result.Success(submission.ToDto());
    }

    public async Task<Result<SubmissionDto>> StartReviewAsync(
        Guid submissionId, Guid requestingTeacherUserId, CancellationToken cancellationToken)
    {
        var submission = await submissionRepository.Query()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            return Result.NotFound<SubmissionDto>("Submission not found.");
        }

        if (submission.Assignment.TeacherUserId != requestingTeacherUserId)
        {
            return Result.Forbidden<SubmissionDto>("You do not own this assignment.");
        }

        var result = submission.StartReview(DateTime.UtcNow, requestingTeacherUserId);
        if (result.IsFailure)
        {
            return Result.Failure<SubmissionDto>(result.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Teacher {TeacherUserId} started reviewing submission {SubmissionId}",
            requestingTeacherUserId, submission.Id);

        return Result.Success(submission.ToDto());
    }

    public async Task<Result<IReadOnlyList<SubmissionDto>>> GetMySubmissionsAsync(
        Guid studentUserId, CancellationToken cancellationToken)
    {
        var submissions = await submissionRepository.Query()
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .Where(s => s.StudentUserId == studentUserId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SubmissionDto>>(submissions.Select(s => s.ToDto()).ToList());
    }

    public async Task<Result<SubmissionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var submission = await submissionRepository.Query()
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return submission is null
            ? Result.NotFound<SubmissionDto>("Submission not found.")
            : Result.Success(submission.ToDto());
    }

    public async Task<Result<SubmissionDto?>> GetForAssignmentAsync(
        Guid assignmentId, Guid studentUserId, CancellationToken cancellationToken)
    {
        var submission = await submissionRepository.Query()
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(
                s => s.AssignmentId == assignmentId && s.StudentUserId == studentUserId, cancellationToken);

        return Result.Success(submission?.ToDto());
    }

    public async Task<Result<IReadOnlyList<SubmissionDto>>> GetListForAssignmentAsync(
        Guid assignmentId, Guid requestingUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.Query().AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

        if (assignment is null)
        {
            return Result.NotFound<IReadOnlyList<SubmissionDto>>("Assignment not found.");
        }

        if (!isAdmin && assignment.TeacherUserId != requestingUserId)
        {
            return Result.Forbidden<IReadOnlyList<SubmissionDto>>("You do not own this assignment.");
        }

        var submissions = await submissionRepository.Query()
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderBy(s => s.StudentUser.FullName)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SubmissionDto>>(submissions.Select(s => s.ToDto()).ToList());
    }
}
