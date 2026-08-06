using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext dbContext) : Repository<User>(dbContext), IUserRepository;
