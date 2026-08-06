using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext dbContext) : Repository<RefreshToken>(dbContext), IRefreshTokenRepository;
