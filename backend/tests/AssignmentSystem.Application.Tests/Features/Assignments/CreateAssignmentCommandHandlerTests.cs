using AssignmentSystem.Application.Features.Assignments;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Assignments;

public class CreateAssignmentCommandHandlerTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [Fact]
    public async Task Create_when_teacher_is_not_assigned_to_class_and_subject_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        var handler = new CreateAssignmentCommandHandler(dbContext);

        var command = new CreateAssignmentCommand(teacherId, "Homework", "Description", classId, subjectId, Now.AddDays(7), 100);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not assigned to teach");
    }

    [Fact]
    public async Task Create_when_teacher_is_assigned_to_class_and_subject_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subjectEntity = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subjectEntity);
        dbContext.Users.Add(teacher);
        dbContext.TeacherAssignments.Add(TeacherAssignment.Create(teacher.Id, classEntity.Id, subjectEntity.Id));
        await dbContext.SaveChangesAsync();

        var handler = new CreateAssignmentCommandHandler(dbContext);

        var command = new CreateAssignmentCommand(
            teacher.Id, "Homework", "Description", classEntity.Id, subjectEntity.Id, Now.AddDays(7), 100);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Homework");
        result.Value.Status.Should().Be("Draft");
    }
}
