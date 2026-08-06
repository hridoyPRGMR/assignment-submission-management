using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Infrastructure.Persistence.Repositories;

public class StudentEnrollmentRepository(AppDbContext dbContext)
    : Repository<StudentEnrollment>(dbContext), IStudentEnrollmentRepository;
