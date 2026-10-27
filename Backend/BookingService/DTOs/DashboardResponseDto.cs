namespace BookingService.DTOs;

public class DashboardResponseDto
{
    public int CheckedInGuests { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int UpcomingBookings { get; set; }

    // Owner/Manager only — null for Receptionist
    public int? TotalGuests { get; set; }
    public int? TotalReservations { get; set; }
    public decimal? MonthlyRevenue { get; set; }
    public decimal? TotalRevenue { get; set; }
}
