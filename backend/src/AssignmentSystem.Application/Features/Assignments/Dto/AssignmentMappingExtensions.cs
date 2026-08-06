namespace AssignmentSystem.Application.Features.Assignments;

public static class AssignmentMappingExtensions
{
    public static AssignmentDto ToDto(this Domain.Entities.Assignment assignment) => new(
        assignment.Id,
        assignment.Title,
        assignment.Description,
        assignment.ClassId,
        assignment.Class.Name,
        assignment.SubjectId,
        assignment.Subject.Name,
        assignment.TeacherUserId,
        assignment.TeacherUser.FullName,
        assignment.DeadlineUtc,
        assignment.MaxMarks,
        assignment.Status.ToString(),
        assignment.GetLifecycleStatus(DateTime.UtcNow).ToString(),
        assignment.CreatedAtUtc,
        assignment.UpdatedAtUtc);
}
