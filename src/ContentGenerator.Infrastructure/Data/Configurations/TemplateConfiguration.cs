using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Infrastructure.Data.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("templates", "contentgenerator");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .IsRequired();

        builder.Property(t => t.TemplateDataJson)
            .HasColumnName("template_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(t => t.IsPremium)
            .HasColumnName("is_premium")
            .HasDefaultValue(false);

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedById)
            .HasColumnName("created_by");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore the computed property
        builder.Ignore(t => t.TemplateData);

        // Indexes
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.IsPremium);
        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.CreatedAt);

        // Relationships
        builder.HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTemplates)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Projects)
            .WithOne(p => p.Template)
            .HasForeignKey(p => p.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}