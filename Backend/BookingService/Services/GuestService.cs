using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;

namespace BookingService.Services;

public class GuestService(IGuestRepository guestRepo) : IGuestService
{
    public async Task<ServiceResult<List<GuestResponseDto>>> GetAllAsync()
    {
        var guests = await guestRepo.GetAllAsync();
        return ServiceResult<List<GuestResponseDto>>.Ok(guests.Select(Map).ToList());
    }

    public async Task<ServiceResult<GuestResponseDto>> GetByIdAsync(int id)
    {
        var g = await guestRepo.GetByIdAsync(id);
        return g is null
            ? ServiceResult<GuestResponseDto>.Fail("Guest not found.", 404)
            : ServiceResult<GuestResponseDto>.Ok(Map(g));
    }

    public async Task<ServiceResult<GuestResponseDto>> CreateAsync(CreateGuestRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResult<GuestResponseDto>.Fail("Full name is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return ServiceResult<GuestResponseDto>.Fail("Phone number is required.", 400);

        var g = new Guest
        {
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address?.Trim()
        };
        await guestRepo.AddAsync(g);
        await guestRepo.SaveChangesAsync();
        return ServiceResult<GuestResponseDto>.Ok(Map(g));
    }

    public async Task<ServiceResult<GuestResponseDto>> UpdateAsync(int id, UpdateGuestRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResult<GuestResponseDto>.Fail("Full name is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return ServiceResult<GuestResponseDto>.Fail("Phone number is required.", 400);

        var g = await guestRepo.GetByIdAsync(id);
        if (g is null) return ServiceResult<GuestResponseDto>.Fail("Guest not found.", 404);

        g.FullName = dto.FullName.Trim();
        g.PhoneNumber = dto.PhoneNumber.Trim();
        g.Email = dto.Email?.Trim();
        g.Address = dto.Address?.Trim();
        await guestRepo.SaveChangesAsync();
        return ServiceResult<GuestResponseDto>.Ok(Map(g));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var g = await guestRepo.GetByIdAsync(id);
        if (g is null) return ServiceResult<bool>.Fail("Guest not found.", 404);

        if (await guestRepo.HasReservationsAsync(id))
            return ServiceResult<bool>.Fail("Cannot delete guest: they have active reservations.", 409);

        // Guests with only cancelled reservations can be deleted
        // For safety, we don't hard-delete - we still allow it here since spec says no Bills table
        // and guest records are not referenced by anything outside BookingDB
        return ServiceResult<bool>.Fail("Guest deletion is not permitted; guest records are retained for audit.", 409);
    }

    private static GuestResponseDto Map(Guest g) => new()
    {
        GuestId = g.GuestId,
        FullName = g.FullName,
        PhoneNumber = g.PhoneNumber,
        Email = g.Email,
        Address = g.Address,
        CreatedAt = g.CreatedAt
    };
}
