namespace AssignmentSystem.Application.Features.Subjects;

public static class SubjectMappingExtensions
{
    public static SubjectDto ToDto(this Domain.Entities.Subject subject) =>
        new(subject.Id, subject.Name, subject.Description, subject.CreatedAtUtc);
}
