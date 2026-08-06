namespace AssignmentSystem.Application.Features.Assignments;

public record GetPublishedAssignmentsForStudentInput(Guid StudentUserId, int Page, int PageSize);
