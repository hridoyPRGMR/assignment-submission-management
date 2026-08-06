using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("StudentEnrollments");
        builder.HasKey(se => se.Id);

        builder.HasIndex(se => new { se.StudentUserId, se.ClassId }).IsUnique();

        builder.HasOne(se => se.StudentUser)
            .WithMany()
            .HasForeignKey(se => se.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(se => se.Class)
            .WithMany()
            .HasForeignKey(se => se.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
