using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class AssignmentRepository(AppDbContext dbContext) : Repository<Assignment>(dbContext), IAssignmentRepository;
