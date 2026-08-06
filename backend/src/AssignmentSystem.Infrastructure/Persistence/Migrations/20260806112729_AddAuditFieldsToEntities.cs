using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TeacherAssignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TeacherAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "TeacherAssignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "TeacherAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Submissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Submissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Subjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Subjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Subjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "StudentEnrollments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "StudentEnrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "StudentEnrollments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "StudentEnrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ClassSubjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ClassSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ClassSubjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ClassSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Classes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Classes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Classes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Assignments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "TeacherAssignments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TeacherAssignments");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "TeacherAssignments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TeacherAssignments");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ClassSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ClassSubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ClassSubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ClassSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Assignments");
        }
    }
}
