namespace AssignmentSystem.Application.Features.Subjects;

public record UpdateSubjectInput(Guid Id, string Name, string? Description);
