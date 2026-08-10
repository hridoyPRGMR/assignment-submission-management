using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Application.Features.Assignments;

public class DeadlineTransitionService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    IStudentEnrollmentRepository studentEnrollmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeadlineTransitionService> logger) : IDeadlineTransitionService
{
    public async Task<int> ProcessDeadlinesAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var overdueAssignments = await assignmentRepository.Query()
            .Where(a =>
                a.Status == AssignmentStatus.Published &&
                a.DeadlineUtc <= nowUtc &&
                a.DeadlineProcessedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var assignment in overdueAssignments)
        {
            if (assignment.GetLifecycleStatus(nowUtc) != AssignmentLifecycleStatus.DeadlinePassed)
            {
                continue;
            }

            var totalSubmissions = await submissionRepository.Query()
                .CountAsync(s => s.AssignmentId == assignment.Id, cancellationToken);

            var lateSubmissions = await submissionRepository.Query()
                .CountAsync(s => s.AssignmentId == assignment.Id && s.IsLate, cancellationToken);

            var enrolledStudents = await studentEnrollmentRepository.Query()
                .CountAsync(e => e.ClassId == assignment.ClassId, cancellationToken);

            var missingSubmissions = Math.Max(0, enrolledStudents - totalSubmissions);

            assignment.MarkDeadlineProcessed(nowUtc);

            logger.LogInformation(
                "Assignment {AssignmentId} ({Title}) deadline passed at {DeadlineUtc}; {LateSubmissionCount}/{TotalSubmissionCount} submissions late, {MissingCount} students have not submitted",
                assignment.Id, assignment.Title, assignment.DeadlineUtc, lateSubmissions, totalSubmissions, missingSubmissions);
        }

        if (overdueAssignments.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return overdueAssignments.Count;
    }
}
