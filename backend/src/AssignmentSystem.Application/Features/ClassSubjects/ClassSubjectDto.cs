namespace AssignmentSystem.Application.Features.ClassSubjects;

public record ClassSubjectDto(Guid Id, Guid ClassId, string ClassName, Guid SubjectId, string SubjectName);
