using BookingService.Data;
using BookingService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Services;

public class DashboardService(BookingDbContext db) : IDashboardService
{
    public async Task<ServiceResult<DashboardResponseDto>> GetDashboardAsync(string role)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var dto = new DashboardResponseDto
        {
            CheckedInGuests = await db.Reservations.CountAsync(r => r.ReservationStatus == "CheckedIn"),
            TodayCheckIns = await db.Reservations.CountAsync(r => r.CheckInDate == today && r.ReservationStatus != "Cancelled"),
            TodayCheckOuts = await db.Reservations.CountAsync(r => r.CheckOutDate == today && r.ReservationStatus == "CheckedIn"),
            UpcomingBookings = await db.Reservations.CountAsync(r => r.ReservationStatus == "Booked")
        };

        if (role is "Owner" or "Manager")
        {
            var thisMonthStart = new DateOnly(today.Year, today.Month, 1).ToDateTime(TimeOnly.MinValue);

            dto.TotalGuests = await db.Guests.CountAsync();
            dto.TotalReservations = await db.Reservations.CountAsync();
            dto.MonthlyRevenue = await db.Payments
                .Where(p => p.PaymentStatus == "Paid" && p.PaymentDate >= thisMonthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
            dto.TotalRevenue = await db.Payments
                .Where(p => p.PaymentStatus == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        }

        return ServiceResult<DashboardResponseDto>.Ok(dto);
    }
}
