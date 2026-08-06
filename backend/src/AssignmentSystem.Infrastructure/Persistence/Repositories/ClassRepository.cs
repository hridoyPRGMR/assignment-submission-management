using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class ClassRepository(AppDbContext dbContext) : Repository<Class>(dbContext), IClassRepository;
