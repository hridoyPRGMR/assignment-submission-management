using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class TeacherAssignmentConfiguration : IEntityTypeConfiguration<TeacherAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherAssignment> builder)
    {
        builder.ToTable("TeacherAssignments");
        builder.HasKey(ta => ta.Id);

        builder.HasIndex(ta => new { ta.TeacherUserId, ta.ClassId, ta.SubjectId }).IsUnique();

        builder.HasOne(ta => ta.TeacherUser)
            .WithMany()
            .HasForeignKey(ta => ta.TeacherUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Class)
            .WithMany()
            .HasForeignKey(ta => ta.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.Subject)
            .WithMany()
            .HasForeignKey(ta => ta.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
