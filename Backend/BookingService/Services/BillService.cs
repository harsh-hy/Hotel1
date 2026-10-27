using BookingService.DTOs;
using BookingService.HttpClients;
using BookingService.Repositories;
using System.Net.Http.Json;

namespace BookingService.Services;

public class BillService(IReservationRepository reservationRepo, IHttpClientFactory httpFactory) : IBillService
{
    public async Task<ServiceResult<BillResponseDto>> GetBillAsync(int reservationId, string? jwtToken)
    {
        var r = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(reservationId);
        if (r is null)
            return ServiceResult<BillResponseDto>.Fail("Reservation not found.", 404);

        // Fetch room info from HotelService (best-effort)
        HotelRoomDto? room = null;
        try
        {
            var client = httpFactory.CreateClient("HotelService");
            if (!string.IsNullOrEmpty(jwtToken))
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", jwtToken);
            room = await client.GetFromJsonAsync<HotelRoomDto>($"/api/hotel/rooms/{r.RoomId}");
        }
        catch { /* room details will be partial; bill still renders */ }

        var nights = r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber;
        var totalPaid = r.Payments.Where(p => p.PaymentStatus == "Paid").Sum(p => p.Amount);
        var outstanding = r.TotalAmount - totalPaid;
        string paymentStatus = totalPaid >= r.TotalAmount ? "Paid"
            : totalPaid > 0 ? "Partially Paid"
            : "Pending";

        var bill = new BillResponseDto
        {
            BillReference = $"OHMS-{r.ReservationId:D6}",
            ReservationId = r.ReservationId,
            Guest = new BillGuestDto
            {
                GuestId = r.Guest!.GuestId,
                FullName = r.Guest.FullName,
                PhoneNumber = r.Guest.PhoneNumber,
                Email = r.Guest.Email,
                Address = r.Guest.Address
            },
            Room = new BillRoomDto
            {
                RoomId = r.RoomId,
                RoomNumber = room?.RoomNumber,
                RoomTypeName = room?.RoomTypeName,
                RoomTypeCapacity = room?.RoomTypeCapacity
            },
            CheckInDate = r.CheckInDate,
            CheckOutDate = r.CheckOutDate,
            Nights = nights,
            PricePerNightSnapshot = r.PricePerNightSnapshot,
            TotalRoomCharges = r.TotalAmount,
            PaymentsReceived = totalPaid,
            OutstandingBalance = outstanding,
            PaymentStatus = paymentStatus,
            Payments = r.Payments.OrderBy(p => p.PaymentDate).Select(p => new BillPaymentDto
            {
                PaymentId = p.PaymentId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentStatus = p.PaymentStatus,
                TransactionReference = p.TransactionReference,
                PaymentDate = p.PaymentDate
            }).ToList(),
            GeneratedAt = DateTime.UtcNow
        };

        return ServiceResult<BillResponseDto>.Ok(bill);
    }
}
