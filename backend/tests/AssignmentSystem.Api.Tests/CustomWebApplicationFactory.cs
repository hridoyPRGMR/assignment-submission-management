using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AssignmentSystem.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin@12345";
    public const string TeacherEmail = "teacher@test.local";
    public const string TeacherPassword = "Teacher@12345";
    public const string OtherTeacherEmail = "other.teacher@test.local";
    public const string OtherTeacherPassword = "OtherTeacher@12345";
    public const string StudentEmail = "student@test.local";
    public const string StudentPassword = "Student@12345";

    public Guid ClassId { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid PublishedAssignmentId { get; private set; }
    public Guid DraftAssignmentId { get; private set; }

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "AssignmentSystemTests",
                ["Jwt:Audience"] = "AssignmentSystemTestsClient",
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-characters-long",
                ["Jwt:AccessTokenMinutes"] = "30",
                ["Jwt:RefreshTokenDays"] = "7",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        dbContext.Database.EnsureCreated();
        Seed(dbContext, passwordHasher);

        return host;
    }

    private void Seed(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        var now = DateTime.UtcNow;

        var admin = User.Create(AdminEmail, passwordHasher.Hash(AdminPassword), "Test Admin", UserRole.Admin, now).Value;
        var teacher = User.Create(TeacherEmail, passwordHasher.Hash(TeacherPassword), "Test Teacher", UserRole.Teacher, now).Value;
        var otherTeacher = User.Create(OtherTeacherEmail, passwordHasher.Hash(OtherTeacherPassword), "Other Teacher", UserRole.Teacher, now).Value;
        var student = User.Create(StudentEmail, passwordHasher.Hash(StudentPassword), "Test Student", UserRole.Student, now).Value;

        dbContext.Users.AddRange(admin, teacher, otherTeacher, student);

        var schoolClass = Class.Create("Test Class", null, now).Value;
        var subject = Subject.Create("Test Subject", null, now).Value;

        dbContext.Classes.Add(schoolClass);
        dbContext.Subjects.Add(subject);
        dbContext.ClassSubjects.Add(ClassSubject.Create(schoolClass.Id, subject.Id, now));
        dbContext.TeacherAssignments.Add(TeacherAssignment.Create(teacher.Id, schoolClass.Id, subject.Id, now));
        dbContext.StudentEnrollments.Add(StudentEnrollment.Create(student.Id, schoolClass.Id, now));

        var draftAssignment = AssignmentSystem.Domain.Entities.Assignment.Create(
            "Draft Assignment", "Draft description", schoolClass.Id, subject.Id, teacher.Id, now.AddDays(7), 100, now).Value;

        var publishedAssignment = AssignmentSystem.Domain.Entities.Assignment.Create(
            "Published Assignment", "Published description", schoolClass.Id, subject.Id, teacher.Id, now.AddDays(7), 100, now).Value;
        publishedAssignment.Publish(now);

        dbContext.Assignments.AddRange(draftAssignment, publishedAssignment);

        dbContext.SaveChanges();

        ClassId = schoolClass.Id;
        SubjectId = subject.Id;
        PublishedAssignmentId = publishedAssignment.Id;
        DraftAssignmentId = draftAssignment.Id;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
