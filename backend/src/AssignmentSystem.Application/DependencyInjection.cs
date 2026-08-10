using System.Reflection;
using AssignmentSystem.Application.Features.Assignments;
using AssignmentSystem.Application.Features.Auth;
using AssignmentSystem.Application.Features.ClassSubjects;
using AssignmentSystem.Application.Features.Classes;
using AssignmentSystem.Application.Features.Dashboard;
using AssignmentSystem.Application.Features.StudentEnrollments;
using AssignmentSystem.Application.Features.Subjects;
using AssignmentSystem.Application.Features.Submissions;
using AssignmentSystem.Application.Features.TeacherAssignments;
using AssignmentSystem.Application.Features.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IClassSubjectService, ClassSubjectService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDeadlineTransitionService, DeadlineTransitionService>();
        services.AddScoped<IStudentEnrollmentService, StudentEnrollmentService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
