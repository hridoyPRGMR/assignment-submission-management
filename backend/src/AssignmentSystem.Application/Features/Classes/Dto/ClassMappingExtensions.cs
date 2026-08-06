namespace AssignmentSystem.Application.Features.Classes;

public static class ClassMappingExtensions
{
    public static ClassDto ToDto(this Domain.Entities.Class classEntity) =>
        new(classEntity.Id, classEntity.Name, classEntity.Description, classEntity.CreatedAtUtc);
}
