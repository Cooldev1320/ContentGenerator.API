using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects", "contentgenerator");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.TemplateId)
            .HasColumnName("template_id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CanvasDataJson)
            .HasColumnName("canvas_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(p => p.ThumbnailUrl)
            .HasColumnName("thumbnail_url");

        builder.Property(p => p.Width)
            .HasColumnName("width")
            .HasDefaultValue(1080);

        builder.Property(p => p.Height)
            .HasColumnName("height")
            .HasDefaultValue(1080);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasDefaultValue(ProjectStatus.Draft);

        builder.Property(p => p.ExportedAt)
            .HasColumnName("exported_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore the computed property
        builder.Ignore(p => p.CanvasData);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.UpdatedAt);

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Template)
            .WithMany(t => t.Projects)
            .HasForeignKey(p => p.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.HistoryEntries)
            .WithOne(h => h.Project)
            .HasForeignKey(h => h.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}