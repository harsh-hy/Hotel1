using HotelService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelService.Data;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Room>()
            .HasIndex(r => r.RoomNumber)
            .IsUnique();

        builder.Entity<RoomType>()
            .HasIndex(t => t.TypeName)
            .IsUnique();

        builder.Entity<Room>()
            .Property(r => r.PricePerNight)
            .HasPrecision(10, 2);

        builder.Entity<Room>()
            .HasOne(r => r.RoomType)
            .WithMany(t => t.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
