using DigitalMenu.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalMenu.Infrastructure.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("tables");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.TableNumber)
            .HasColumnName("table_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(t => new { t.TenantId, t.TableNumber })
            .IsUnique()
            .HasDatabaseName("ux_tables_tenant_table_number");

        builder.Property(t => t.QrCodeUrl)
            .HasColumnName("qr_code_url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.IsWaiterCalled)
            .HasColumnName("is_waiter_called")
            .HasDefaultValue(false);

        builder.Property(t => t.IsBillRequested)
            .HasColumnName("is_bill_requested")
            .HasDefaultValue(false);

        builder.Property(t => t.LastRequestTime)
            .HasColumnName("last_request_time")
            .HasColumnType("timestamptz");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .HasPrincipalKey(tenant => tenant.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
