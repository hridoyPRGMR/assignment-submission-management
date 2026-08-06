using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Content).HasMaxLength(8000).IsRequired();
        builder.Property(s => s.FileUrl).HasMaxLength(1000);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(s => s.Feedback).HasMaxLength(4000);
        builder.Property(s => s.SubmittedAtUtc).IsRequired();
        builder.Property(s => s.IsLate).IsRequired();

        builder.HasIndex(s => new { s.AssignmentId, s.StudentUserId }).IsUnique();

        builder.HasOne(s => s.Assignment)
            .WithMany()
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.StudentUser)
            .WithMany()
            .HasForeignKey(s => s.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(s => s.GradedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
