using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Infrastructure.Data.Configurations;

public class HistoryConfiguration : IEntityTypeConfiguration<History>
{
    public void Configure(EntityTypeBuilder<History> builder)
    {
        builder.ToTable("history", "contentgenerator");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(h => h.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(h => h.ProjectId)
            .HasColumnName("project_id");

        builder.Property(h => h.ActionType)
            .HasColumnName("action_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.ActionDataJson)
            .HasColumnName("action_data")
            .HasColumnType("jsonb");

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore UpdatedAt for History since it's immutable
        builder.Ignore(h => h.UpdatedAt);

        // Ignore the computed property
        builder.Ignore(h => h.ActionData);

        // Indexes
        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => h.ActionType);
        builder.HasIndex(h => h.CreatedAt);

        // Relationships
        builder.HasOne(h => h.User)
            .WithMany(u => u.HistoryEntries)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Project)
            .WithMany(p => p.HistoryEntries)
            .HasForeignKey(h => h.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}