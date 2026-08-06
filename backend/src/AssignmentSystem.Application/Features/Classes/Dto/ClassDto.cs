namespace AssignmentSystem.Application.Features.Classes;

public record ClassDto(Guid Id, string Name, string? Description, DateTime CreatedAtUtc);
