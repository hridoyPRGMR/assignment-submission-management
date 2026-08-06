using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class SubjectRepository(AppDbContext dbContext) : Repository<Subject>(dbContext), ISubjectRepository;
