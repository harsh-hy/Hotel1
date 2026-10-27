namespace BookingService.DTOs;

public class RevenueByMethodDto
{
    public string PaymentMethod { get; set; } = "";
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class RevenueReportResponseDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TransactionCount { get; set; }
    public List<RevenueByMethodDto> ByPaymentMethod { get; set; } = [];
}
