namespace AssignmentSystem.Application.Features.Subjects;

public record SubjectDto(Guid Id, string Name, string? Description, DateTime CreatedAtUtc);
