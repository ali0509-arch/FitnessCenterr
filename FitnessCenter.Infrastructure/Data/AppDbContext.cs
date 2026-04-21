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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User ──────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.UserID);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("User");
        });

        // ── Trainer ───────────────────────────────────────────────
        modelBuilder.Entity<Trainer>(e =>
        {
            e.ToTable("trainer");
            e.HasKey(t => t.TrainerID);
        });

        // ── Member ────────────────────────────────────────────────
        modelBuilder.Entity<Member>(e =>
        {
            e.ToTable("member");
            e.HasKey(m => m.MemberID);
            e.HasIndex(m => m.Email).IsUnique();
            e.HasOne(m => m.Trainer)
             .WithMany(t => t.Members)
             .HasForeignKey(m => m.TrainerID)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Subscription ──────────────────────────────────────────
        modelBuilder.Entity<Subscription>(e =>
        {
            e.ToTable("subscription");
            e.HasKey(s => s.SubscriptionID);
            e.Property(s => s.Price).HasPrecision(8, 2);
        });

        // ── Membership ────────────────────────────────────────────
        modelBuilder.Entity<Membership>(e =>
        {
            e.ToTable("membership");
            e.HasKey(ms => ms.MembershipID);
            e.HasOne(ms => ms.Member)
             .WithMany(m => m.Memberships)
             .HasForeignKey(ms => ms.MemberID)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ms => ms.Subscription)
             .WithMany(s => s.Memberships)
             .HasForeignKey(ms => ms.SubscriptionID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Class ─────────────────────────────────────────────────
        modelBuilder.Entity<Class>(e =>
        {
            e.ToTable("class");
            e.HasKey(c => c.ClassID);
            e.HasOne(c => c.Trainer)
             .WithMany(t => t.Classes)
             .HasForeignKey(c => c.TrainerID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ClassBooking ──────────────────────────────────────────
        modelBuilder.Entity<ClassBooking>(e =>
        {
            e.ToTable("classbooking");
            e.HasKey(cb => cb.BookingID);
            e.HasOne(cb => cb.Member)
             .WithMany(m => m.ClassBookings)
             .HasForeignKey(cb => cb.MemberID)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cb => cb.Class)
             .WithMany(c => c.ClassBookings)
             .HasForeignKey(cb => cb.ClassID)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
