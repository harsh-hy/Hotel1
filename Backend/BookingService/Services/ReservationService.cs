using BookingService.DTOs;
using BookingService.HttpClients;
using BookingService.Models;
using BookingService.Repositories;
using System.Net.Http.Json;

namespace BookingService.Services;

public class ReservationService(
    IReservationRepository reservationRepo,
    IHttpClientFactory httpFactory) : IReservationService
{
    public async Task<ServiceResult<List<ReservationResponseDto>>> GetAllAsync()
    {
        var list = await reservationRepo.GetAllWithGuestAsync();
        return ServiceResult<List<ReservationResponseDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ServiceResult<ReservationResponseDto>> GetByIdAsync(int id)
    {
        var r = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(id);
        return r is null
            ? ServiceResult<ReservationResponseDto>.Fail("Reservation not found.", 404)
            : ServiceResult<ReservationResponseDto>.Ok(Map(r));
    }

    public async Task<ServiceResult<List<ReservationResponseDto>>> GetTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var list = await reservationRepo.GetTodayReservationsAsync(today);
        return ServiceResult<List<ReservationResponseDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ServiceResult<ReservationResponseDto>> CreateAsync(
        CreateReservationRequestDto dto, string? jwtToken)
    {
        if (dto.CheckOutDate <= dto.CheckInDate)
            return ServiceResult<ReservationResponseDto>.Fail("Check-out must be after check-in.", 400);
        if (dto.CheckInDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return ServiceResult<ReservationResponseDto>.Fail("Check-in date cannot be in the past.", 400);
        if (dto.NumberOfAdults < 1)
            return ServiceResult<ReservationResponseDto>.Fail("At least 1 adult is required.", 400);
        if (dto.NumberOfChildren < 0)
            return ServiceResult<ReservationResponseDto>.Fail("Number of children cannot be negative.", 400);
        if (!await reservationRepo.GuestExistsAsync(dto.GuestId))
            return ServiceResult<ReservationResponseDto>.Fail("Guest does not exist.", 400);

        // Validate room via HotelService
        var room = await FetchRoomAsync(dto.RoomId, jwtToken);
        if (room is null)
            return ServiceResult<ReservationResponseDto>.Fail($"Room {dto.RoomId} not found or HotelService unavailable.", 400);
        if (room.Status != "Available")
            return ServiceResult<ReservationResponseDto>.Fail($"Room is not available for booking (status: {room.Status}).", 409);

        var totalGuests = dto.NumberOfAdults + dto.NumberOfChildren;
        if (room.RoomTypeCapacity < totalGuests)
            return ServiceResult<ReservationResponseDto>.Fail(
                $"Room capacity ({room.RoomTypeCapacity}) is less than total guests ({totalGuests}).", 400);

        var nights = dto.CheckOutDate.DayNumber - dto.CheckInDate.DayNumber;
        var priceSnapshot = room.PricePerNight;
        var total = priceSnapshot * nights;

        // Concurrency-safe conflict check using serializable transaction
        using var tx = await reservationRepo.BeginSerializableTransactionAsync();
        try
        {
            if (await reservationRepo.HasOverlapAsync(dto.RoomId, dto.CheckInDate, dto.CheckOutDate))
            {
                await tx.RollbackAsync();
                return ServiceResult<ReservationResponseDto>.Fail(
                    "Room already has an overlapping reservation for those dates.", 409);
            }

            var reservation = new Reservation
            {
                GuestId = dto.GuestId,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                NumberOfAdults = dto.NumberOfAdults,
                NumberOfChildren = dto.NumberOfChildren,
                PricePerNightSnapshot = priceSnapshot,
                TotalAmount = total
            };
            await reservationRepo.AddAsync(reservation);
            await reservationRepo.SaveChangesAsync();
            await tx.CommitAsync();

            // Reload with guest for response
            var created = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(reservation.ReservationId);
            return ServiceResult<ReservationResponseDto>.Ok(Map(created!));
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<ServiceResult<ReservationResponseDto>> UpdateAsync(
        int id, UpdateReservationRequestDto dto, string? jwtToken)
    {
        if (dto.CheckOutDate <= dto.CheckInDate)
            return ServiceResult<ReservationResponseDto>.Fail("Check-out must be after check-in.", 400);
        if (dto.NumberOfAdults < 1)
            return ServiceResult<ReservationResponseDto>.Fail("At least 1 adult is required.", 400);
        if (dto.NumberOfChildren < 0)
            return ServiceResult<ReservationResponseDto>.Fail("Number of children cannot be negative.", 400);

        var reservation = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(id);
        if (reservation is null)
            return ServiceResult<ReservationResponseDto>.Fail("Reservation not found.", 404);
        if (reservation.ReservationStatus != "Booked")
            return ServiceResult<ReservationResponseDto>.Fail("Only Booked reservations can be updated.", 409);

        // Validate capacity
        var room = await FetchRoomAsync(reservation.RoomId, jwtToken);
        if (room is not null)
        {
            var totalGuests = dto.NumberOfAdults + dto.NumberOfChildren;
            if (room.RoomTypeCapacity < totalGuests)
                return ServiceResult<ReservationResponseDto>.Fail(
                    $"Room capacity ({room.RoomTypeCapacity}) is less than total guests ({totalGuests}).", 400);
        }

        using var tx = await reservationRepo.BeginSerializableTransactionAsync();
        try
        {
            if (await reservationRepo.HasOverlapAsync(reservation.RoomId, dto.CheckInDate, dto.CheckOutDate, excludeReservationId: id))
            {
                await tx.RollbackAsync();
                return ServiceResult<ReservationResponseDto>.Fail(
                    "Room already has an overlapping reservation for those dates.", 409);
            }

            var nights = dto.CheckOutDate.DayNumber - dto.CheckInDate.DayNumber;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.NumberOfAdults = dto.NumberOfAdults;
            reservation.NumberOfChildren = dto.NumberOfChildren;
            reservation.TotalAmount = reservation.PricePerNightSnapshot * nights;
            reservation.UpdatedAt = DateTime.UtcNow;
            await reservationRepo.SaveChangesAsync();
            await tx.CommitAsync();

            return ServiceResult<ReservationResponseDto>.Ok(Map(reservation));
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<ServiceResult<bool>> CancelAsync(int id)
    {
        var r = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(id);
        if (r is null) return ServiceResult<bool>.Fail("Reservation not found.", 404);
        if (r.ReservationStatus != "Booked")
            return ServiceResult<bool>.Fail("Only Booked reservations can be cancelled.", 409);
        r.ReservationStatus = "Cancelled";
        r.UpdatedAt = DateTime.UtcNow;
        await reservationRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> CheckInAsync(int id)
    {
        var r = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(id);
        if (r is null) return ServiceResult<bool>.Fail("Reservation not found.", 404);
        if (r.ReservationStatus != "Booked")
            return ServiceResult<bool>.Fail("Only Booked reservations can be checked in.", 409);
        r.ReservationStatus = "CheckedIn";
        r.UpdatedAt = DateTime.UtcNow;
        await reservationRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> CheckOutAsync(int id)
    {
        var r = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(id);
        if (r is null) return ServiceResult<bool>.Fail("Reservation not found.", 404);
        if (r.ReservationStatus != "CheckedIn")
            return ServiceResult<bool>.Fail("Only CheckedIn reservations can be checked out.", 409);
        r.ReservationStatus = "CheckedOut";
        r.UpdatedAt = DateTime.UtcNow;
        await reservationRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private async Task<HotelRoomDto?> FetchRoomAsync(int roomId, string? jwtToken)
    {
        try
        {
            var client = httpFactory.CreateClient("HotelService");
            if (!string.IsNullOrEmpty(jwtToken))
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", jwtToken);
            return await client.GetFromJsonAsync<HotelRoomDto>($"/api/hotel/rooms/{roomId}");
        }
        catch
        {
            return null;
        }
    }

    private static ReservationResponseDto Map(Reservation r) => new()
    {
        ReservationId = r.ReservationId,
        GuestId = r.GuestId,
        GuestName = r.Guest?.FullName,
        RoomId = r.RoomId,
        CheckInDate = r.CheckInDate,
        CheckOutDate = r.CheckOutDate,
        Nights = r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber,
        NumberOfAdults = r.NumberOfAdults,
        NumberOfChildren = r.NumberOfChildren,
        PricePerNightSnapshot = r.PricePerNightSnapshot,
        TotalAmount = r.TotalAmount,
        ReservationStatus = r.ReservationStatus,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
