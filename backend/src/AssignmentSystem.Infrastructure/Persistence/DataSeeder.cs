using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var admin = User.Create("admin@school.edu", passwordHasher.Hash("Admin@12345"), "System Administrator", UserRole.Admin, now).Value;
        var teacher = User.Create("teacher@school.edu", passwordHasher.Hash("Teacher@12345"), "Jane Smith", UserRole.Teacher, now).Value;
        var student = User.Create("student@school.edu", passwordHasher.Hash("Student@12345"), "John Doe", UserRole.Student, now).Value;

        dbContext.Users.AddRange(admin, teacher, student);

        var schoolClass = Class.Create("Grade 10 - Section A", "Homeroom class for grade 10, section A", now).Value;
        var subject = Subject.Create("Mathematics", "Core mathematics curriculum", now).Value;

        dbContext.Classes.Add(schoolClass);
        dbContext.Subjects.Add(subject);

        var classSubject = ClassSubject.Create(schoolClass.Id, subject.Id, now);
        dbContext.ClassSubjects.Add(classSubject);

        var teacherAssignment = TeacherAssignment.Create(teacher.Id, schoolClass.Id, subject.Id, now);
        dbContext.TeacherAssignments.Add(teacherAssignment);

        var studentEnrollment = StudentEnrollment.Create(student.Id, schoolClass.Id, now);
        dbContext.StudentEnrollments.Add(studentEnrollment);

        var draftAssignment = Domain.Entities.Assignment.Create(
            "Algebra Practice Set 1",
            "Complete problems 1 through 20 in chapter 3.",
            schoolClass.Id,
            subject.Id,
            teacher.Id,
            now.AddDays(10),
            50,
            now).Value;

        var publishedAssignment = Domain.Entities.Assignment.Create(
            "Geometry Homework 1",
            "Prove the given theorems and submit your working.",
            schoolClass.Id,
            subject.Id,
            teacher.Id,
            now.AddDays(14),
            100,
            now).Value;
        publishedAssignment.Publish(now);

        dbContext.Assignments.AddRange(draftAssignment, publishedAssignment);

        var submission = Submission.Create(
            publishedAssignment.Id,
            student.Id,
            "Here is my proof for theorem 1: ...",
            null,
            publishedAssignment.DeadlineUtc,
            now).Value;

        dbContext.Submissions.Add(submission);

        await dbContext.SaveChangesAsync();
    }
}
