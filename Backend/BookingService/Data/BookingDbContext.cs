using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Data;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Unique constraint: one transaction reference per payment
        builder.Entity<Payment>()
            .HasIndex(p => p.TransactionReference)
            .IsUnique();

        // Money columns: decimal(10,2) — never float/double
        builder.Entity<Reservation>()
            .Property(r => r.PricePerNightSnapshot)
            .HasPrecision(10, 2);

        builder.Entity<Reservation>()
            .Property(r => r.TotalAmount)
            .HasPrecision(10, 2);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        // FK: Guest (1) → Reservations (many)
        // Cascade: deleting a guest removes their reservations
        builder.Entity<Guest>()
            .HasMany(g => g.Reservations)
            .WithOne(r => r.Guest)
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: Reservation (1) → Payments (many)
        // Cascade: deleting a reservation removes its payment records
        builder.Entity<Reservation>()
            .HasMany(r => r.Payments)
            .WithOne(p => p.Reservation)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
