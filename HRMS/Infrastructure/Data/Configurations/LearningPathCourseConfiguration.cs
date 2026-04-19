using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LearningPathCourseConfiguration : IEntityTypeConfiguration<LearningPathCourse>
{
    public void Configure(EntityTypeBuilder<LearningPathCourse> builder)
    {
        builder.ToTable("LearningPathCourses");
        builder.HasKey(e => e.LearningPathCourseId);
        builder.Property(e => e.LearningPathCourseId).ValueGeneratedOnAdd();
        builder.Property(e => e.SortOrder).HasDefaultValue(0);
        builder.HasOne(e => e.LearningPath).WithMany(lp => lp.Courses)
               .HasForeignKey(e => e.LearningPathId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Course).WithMany(c => c.LearningPathCourses)
               .HasForeignKey(e => e.LmsCourseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.LearningPathId, e.LmsCourseId }).IsUnique();
    }
}
