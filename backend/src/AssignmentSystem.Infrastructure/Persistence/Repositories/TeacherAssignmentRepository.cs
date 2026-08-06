using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class TeacherAssignmentRepository(AppDbContext dbContext)
    : Repository<TeacherAssignment>(dbContext), ITeacherAssignmentRepository;
