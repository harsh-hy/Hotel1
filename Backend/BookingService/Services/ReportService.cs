using BookingService.Data;
using BookingService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Services;

public class ReportService(BookingDbContext db) : IReportService
{
    public async Task<ServiceResult<RevenueReportResponseDto>> GetRevenueReportAsync(DateOnly? from, DateOnly? to)
    {
        var fromDt = (from ?? new DateOnly(DateTime.UtcNow.Year, 1, 1)).ToDateTime(TimeOnly.MinValue);
        var toDt = (to ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToDateTime(TimeOnly.MaxValue);

        var payments = await db.Payments.AsNoTracking()
            .Where(p => p.PaymentStatus == "Paid" && p.PaymentDate >= fromDt && p.PaymentDate <= toDt)
            .ToListAsync();

        var byMethod = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new RevenueByMethodDto
            {
                PaymentMethod = g.Key,
                Total = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return ServiceResult<RevenueReportResponseDto>.Ok(new RevenueReportResponseDto
        {
            From = from,
            To = to,
            TotalRevenue = payments.Sum(p => p.Amount),
            TransactionCount = payments.Count,
            ByPaymentMethod = byMethod
        });
    }

    public async Task<ServiceResult<ReservationReportResponseDto>> GetReservationReportAsync(
        DateOnly? from, DateOnly? to, string? status)
    {
        var fromDate = from ?? new DateOnly(DateTime.UtcNow.Year, 1, 1);
        var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Guest)
            .Where(r => r.CheckInDate >= fromDate && r.CheckInDate <= toDate);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.ReservationStatus == status);

        var list = await query.OrderByDescending(r => r.CheckInDate).ToListAsync();

        var items = list.Select(r => new ReservationReportItemDto
        {
            ReservationId = r.ReservationId,
            GuestName = r.Guest?.FullName,
            RoomId = r.RoomId,
            CheckInDate = r.CheckInDate,
            CheckOutDate = r.CheckOutDate,
            Nights = r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber,
            PricePerNightSnapshot = r.PricePerNightSnapshot,
            TotalAmount = r.TotalAmount,
            ReservationStatus = r.ReservationStatus,
            CreatedAt = r.CreatedAt
        }).ToList();

        var byStatus = list
            .GroupBy(r => r.ReservationStatus)
            .Select(g => new ReservationStatusCountDto { Status = g.Key, Count = g.Count() })
            .ToList();

        return ServiceResult<ReservationReportResponseDto>.Ok(new ReservationReportResponseDto
        {
            From = fromDate,
            To = toDate,
            StatusFilter = status,
            TotalCount = items.Count,
            TotalRevenue = list.Where(r => r.ReservationStatus != "Cancelled").Sum(r => r.TotalAmount),
            ByStatus = byStatus,
            Reservations = items
        });
    }
}
