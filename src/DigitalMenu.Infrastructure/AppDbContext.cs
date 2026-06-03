using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DigitalMenu.Core.Models;
using DigitalMenu.Core.Services;

namespace DigitalMenu.Infrastructure;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Table> Tables { get; set; }

    /// <summary>
    /// EF Core global filter ifadesi yalnızca DbContext property'sine referans verebilir (field değil).
    /// </summary>
    private string? CurrentTenantId => _tenantProvider.GetTenantId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Multi-Tenant Filtrelemesi: ITenantEntity arayüzünü uygulayan tüm tablolara otomatik filtre koy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateTenantFilterExpression(entityType.ClrType));
            }
        }
    }

    // Dinamik LINQ Expression: x => x.TenantId == CurrentTenantId
    private LambdaExpression CreateTenantFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "x");
        var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
        var currentTenantId = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
        var condition = Expression.Equal(tenantIdProperty, currentTenantId);
        return Expression.Lambda(condition, parameter);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyChangeTracking();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    // SaveChanges tetiklendiğinde TenantId'yi otomatik set etme
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyChangeTracking();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyChangeTracking()
    {
        var currentTenantId = _tenantProvider.GetTenantId();

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            // Seed data gibi senaryolarda TenantId önceden atanmışsa korunur (ITenantProvider boş olabilir)
            if (entry.State == EntityState.Added && string.IsNullOrWhiteSpace(entry.Entity.TenantId))
            {
                entry.Entity.TenantId = currentTenantId
                    ?? throw new InvalidOperationException("TenantId bulunamadı!");
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
