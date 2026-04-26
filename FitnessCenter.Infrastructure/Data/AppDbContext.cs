using FitnessCenterr.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<ClassBooking> ClassBookings { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Center> Centers { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<VendingMachine> VendingMachines { get; set; }
    public DbSet<VendingMachineStock> VendingMachineStocks { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("app_user");
            e.HasKey(u => u.UserID);
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Trainer>(e =>
        {
            e.ToTable("trainer");
            e.HasKey(t => t.TrainerID);
        });

        modelBuilder.Entity<Member>(e =>
        {
            e.ToTable("member");
            e.HasKey(m => m.MemberID);
            e.HasIndex(m => m.Email).IsUnique();
            e.HasOne(m => m.Trainer).WithMany(t => t.Members)
             .HasForeignKey(m => m.TrainerID).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.ToTable("subscription");
            e.HasKey(s => s.SubscriptionID);
            e.Property(s => s.Price).HasPrecision(8, 2);
        });

        modelBuilder.Entity<Membership>(e =>
        {
            e.ToTable("membership");
            e.HasKey(ms => ms.MembershipID);
            e.HasOne(ms => ms.Member).WithMany(m => m.Memberships)
             .HasForeignKey(ms => ms.MemberID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ms => ms.Subscription).WithMany(s => s.Memberships)
             .HasForeignKey(ms => ms.SubscriptionID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Class>(e =>
        {
            e.ToTable("class");
            e.HasKey(c => c.ClassID);
            e.HasOne(c => c.Trainer).WithMany(t => t.Classes)
             .HasForeignKey(c => c.TrainerID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Hall).WithMany(h => h.Classes)
             .HasForeignKey(c => c.HallID).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(c => c.Location).WithMany(l => l.Classes)
             .HasForeignKey(c => c.LocationID).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClassBooking>(e =>
        {
            e.ToTable("classbooking");
            e.HasKey(cb => cb.BookingID);
            e.HasOne(cb => cb.Member).WithMany(m => m.ClassBookings)
             .HasForeignKey(cb => cb.MemberID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cb => cb.Class).WithMany(c => c.ClassBookings)
             .HasForeignKey(cb => cb.ClassID).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.ToTable("Location");
            e.HasKey(l => l.LocationID);
        });

        modelBuilder.Entity<Center>(e =>
        {
            e.ToTable("Center");
            e.HasKey(c => c.CenterID);
            e.HasOne(c => c.Location).WithMany(l => l.Centers)
             .HasForeignKey(c => c.LocationID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Hall>(e =>
        {
            e.ToTable("Hall");
            e.HasKey(h => h.HallID);
            e.HasOne(h => h.Center).WithMany(c => c.Halls)
             .HasForeignKey(h => h.CenterID).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Equipment>(e =>
        {
            e.ToTable("Equipment");
            e.HasKey(eq => eq.EquipmentID);
        });

        modelBuilder.Entity<VendingMachine>(e =>
        {
            e.ToTable("VendingMachine");
            e.HasKey(v => v.VendingMachineID);
        });

        modelBuilder.Entity<VendingMachineStock>(e =>
        {
            e.ToTable("VendingMachineStock");
            e.HasKey(vs => vs.StockID);
            e.Property(vs => vs.Price).HasPrecision(8, 2);
            e.HasOne(vs => vs.VendingMachine).WithMany(v => v.Stocks)
             .HasForeignKey(vs => vs.VendingMachineID).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Staff>(e =>
        {
            e.ToTable("Staff");
            e.HasKey(s => s.StaffID);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("Payment");
            e.HasKey(p => p.PaymentID);
            e.Property(p => p.Amount).HasPrecision(8, 2);
            e.HasOne(p => p.Member).WithMany(m => m.Payments)
             .HasForeignKey(p => p.MemberID).OnDelete(DeleteBehavior.Cascade);
        });
    }
}