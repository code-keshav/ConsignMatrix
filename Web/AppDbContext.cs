using System.Reflection;
using Acl.Configuration;
using Base.Configuration;
using Base.Constants;
using Base.Entities;
using Base.Entities.Consignment;
using Base.Entities.Interfaces;
using Base.MigrationHistoryOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Web;

public class AppDbContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("Default"))
            .ReplaceService<IHistoryRepository, MigrationHistory>();
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddBase();
        modelBuilder.AddAcl();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        modelBuilder.Entity<User>()
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.CreatedByUser)
            .WithMany()
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Employee - User: 1:0..1 with unique filtered index
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("user_id IS NOT NULL");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeCode)
            .IsUnique();

        // Employee - Branch
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.CurrentBranch)
            .WithMany()
            .HasForeignKey(e => e.CurrentBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Driver - Employee: 1:0..1
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.Employee)
            .WithOne(e => e.Driver)
            .HasForeignKey<Driver>(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Consignment multi-FK: all Restrict to prevent cascade conflicts
        modelBuilder.Entity<Consignment>()
            .HasOne(c => c.Sender)
            .WithMany()
            .HasForeignKey(c => c.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consignment>()
            .HasOne(c => c.SenderAddress)
            .WithMany()
            .HasForeignKey(c => c.SenderAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consignment>()
            .HasOne(c => c.Receiver)
            .WithMany()
            .HasForeignKey(c => c.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consignment>()
            .HasOne(c => c.ReceiverAddress)
            .WithMany()
            .HasForeignKey(c => c.ReceiverAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consignment>()
            .HasOne(c => c.OriginBranch)
            .WithMany()
            .HasForeignKey(c => c.OriginBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consignment>()
            .HasOne(c => c.DestinationBranch)
            .WithMany()
            .HasForeignKey(c => c.DestinationBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // PickupTask - Employee (Driver): SetNull on delete
        modelBuilder.Entity<PickupTask>()
            .HasOne(p => p.AssignedDriver)
            .WithMany()
            .HasForeignKey(p => p.AssignedDriverId)
            .OnDelete(DeleteBehavior.SetNull);

        // PickupTask - Vehicle: SetNull on delete
        modelBuilder.Entity<PickupTask>()
            .HasOne(p => p.AssignedVehicle)
            .WithMany()
            .HasForeignKey(p => p.AssignedVehicleId)
            .OnDelete(DeleteBehavior.SetNull);

    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        BeforeSaveChanges();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void BeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var entries = ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted).ToList();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is BaseEntity entity)
                    {
                        entity.RecDate = DateTime.UtcNow;
                        if (entity.RecById == 0 && entity.RecBranchId == 0)
                        {
                            var id = httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == "Id");
                            var user = GetUser(Convert.ToInt64(id.Value));
                            if (user != null)
                            {
                                entity.RecBy = user;
                                entity.RecBranchId = user.BranchId;
                            }
                        }
                    }

                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDelete delEntity)
                    {
                        entry.State = EntityState.Modified;
                        delEntity.RecStatus = RecStatusConstants.Deleted;
                    }
                    else
                    {
                        entry.State = EntityState.Deleted;
                    }

                    break;
            }
        }
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.RecStatus != RecStatusConstants.Deleted);
    }

    User? GetUser(long id)
    {
        object user;
        if (!httpContextAccessor.HttpContext.Items.TryGetValue("_current_logged_in_user", out user))
        {
            var fromDb = Set<User>().FirstOrDefault(x => x.Id == id);
            httpContextAccessor.HttpContext.Items["_current_logged_in_user"] = fromDb;
            return fromDb;
        }

        return user == null ? null : user as User;
    }
}