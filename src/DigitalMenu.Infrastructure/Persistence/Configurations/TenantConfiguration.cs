using DigitalMenu.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalMenu.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.TenantId)
            .IsUnique()
            .HasDatabaseName("ux_tenants_tenant_id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(t => t.PrimaryColor)
            .HasColumnName("primary_color")
            .HasMaxLength(16);

        builder.Property(t => t.SecondaryColor)
            .HasColumnName("secondary_color")
            .HasMaxLength(16);

        builder.Property(t => t.SelectedTemplate)
            .HasColumnName("selected_template")
            .HasMaxLength(32);

        builder.Property(t => t.TrendyolApiKey)
            .HasColumnName("trendyol_api_key")
            .HasMaxLength(500);

        builder.Property(t => t.TrendyolApiSecret)
            .HasColumnName("trendyol_api_secret")
            .HasMaxLength(500);

        builder.Property(t => t.TrendyolSellerId)
            .HasColumnName("trendyol_seller_id")
            .HasMaxLength(100);

        builder.Property(t => t.TrendyolStoreId)
            .HasColumnName("trendyol_store_id")
            .HasMaxLength(100);

        builder.Property(t => t.TrendyolAgentName)
            .HasColumnName("trendyol_agent_name")
            .HasMaxLength(200);

        builder.Property(t => t.TrendyolExecutorUser)
            .HasColumnName("trendyol_executor_user")
            .HasMaxLength(200);

        builder.Property(t => t.TrendyolHost)
            .HasColumnName("trendyol_host")
            .HasMaxLength(200);

        builder.Property(t => t.PriceMultiplier)
            .HasColumnName("price_multiplier")
            .HasPrecision(18, 4)
            .HasDefaultValue(1.0m);

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
    }
}
