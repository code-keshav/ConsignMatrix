using Base.Entities;
using Base.Entities.Consignment;
using Microsoft.EntityFrameworkCore;

namespace Base.Configuration;

public static class EntityRegisterer
{
    public static ModelBuilder AddBase(this ModelBuilder builder)
    {
        builder.Entity<User>();
        builder.Entity<Branch>();
        builder.Entity<Organization>();
        builder.Entity<Role>();
        builder.Entity<UserRole>(); 
        builder.Entity<UserBranchTransfer>();
        builder.Entity<BranchPinCode>();
        builder.Entity<Employee>();
        builder.Entity<Driver>();
        builder.Entity<Vehicle>();
        builder.Entity<VehicleAssignment>();
        builder.Entity<Customer>()
            .HasMany(c => c.CustomerAddresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<CustomerAddress>();
        builder.Entity<Consignment>()
            .HasMany(c => c.Packages)
            .WithOne(p => p.Consignment)
            .HasForeignKey(p => p.ConsignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Consignment>()
            .HasMany(c => c.StatusLogs)
            .WithOne(s => s.Consignment)
            .HasForeignKey(s => s.ConsignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Consignment>()
            .HasIndex(c => c.TrackingNumber)
            .IsUnique();
        builder.Entity<Package>()
            .HasIndex(p => p.Barcode)
            .IsUnique();
        builder.Entity<Package>();
        builder.Entity<ConsignmentStatusLog>();
        builder.Entity<PickupTask>();
        builder.Entity<Consignment>()
            .HasMany(c => c.PickupTasks)
            .WithOne(p => p.Consignment)
            .HasForeignKey(p => p.ConsignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Trip>()
            .HasIndex(t => t.TripNumber).IsUnique();
        builder.Entity<Trip>()
            .HasMany(t => t.TripConsignments)
            .WithOne(tc => tc.Trip)
            .HasForeignKey(tc => tc.TripId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<TripConsignment>();
        return builder;
    }
}