using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Action).HasMaxLength(120).IsRequired();
        builder.Property(log => log.ResourceType).HasMaxLength(120).IsRequired();
        builder.Property(log => log.MetadataJson).HasMaxLength(10000);
        builder.HasIndex(log => new { log.ActorId, log.CreatedAt });
        builder.HasIndex(log => new { log.ResourceType, log.ResourceId, log.CreatedAt });
    }
}
