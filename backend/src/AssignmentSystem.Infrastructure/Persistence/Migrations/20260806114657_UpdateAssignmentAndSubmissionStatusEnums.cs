using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssignmentAndSubmissionStatusEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AssignmentStatus gained Graded/Archived and SubmissionStatus gained LateSubmission and was
            // renamed ReturnedForRevision -> Returned. Both columns are stored as strings, so no schema
            // change is needed here, only a data rename for any existing rows.
            migrationBuilder.Sql(
                "UPDATE \"Submissions\" SET \"Status\" = 'Returned' WHERE \"Status\" = 'ReturnedForRevision';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Submissions\" SET \"Status\" = 'ReturnedForRevision' WHERE \"Status\" = 'Returned';");
        }
    }
}
