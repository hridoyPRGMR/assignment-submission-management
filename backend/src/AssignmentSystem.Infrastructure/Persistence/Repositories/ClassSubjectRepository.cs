using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class ClassSubjectRepository(AppDbContext dbContext) : Repository<ClassSubject>(dbContext), IClassSubjectRepository;
