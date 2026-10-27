using BookingService.Controllers;
using BookingService.Data;
using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BookingService_Tests;

[TestFixture]
public class PaymentsControllerTests
{
    private BookingDbContext _db = null!;
    private PaymentsController _controller = null!;
    private int _reservationId;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new BookingDbContext(options);

        var guest = new Guest { FullName = "Test Guest", PhoneNumber = "1234567890" };
        _db.Guests.Add(guest);
        await _db.SaveChangesAsync();

        var reservation = new Reservation
        {
            GuestId = guest.GuestId,
            RoomId = 1,
            CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            CheckOutDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            NumberOfAdults = 2, NumberOfChildren = 0,
            PricePerNightSnapshot = 200m, TotalAmount = 400m,
            ReservationStatus = "Booked"
        };
        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();
        _reservationId = reservation.ReservationId;

        var paymentRepo = new PaymentRepository(_db);
        var reservationRepo = new ReservationRepository(_db);
        var service = new PaymentService(paymentRepo, reservationRepo);
        _controller = new PaymentsController(service);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Pay_Valid_Returns201()
    {
        var result = await _controller.MockPayment(new MockPaymentRequestDto
        {
            ReservationId = _reservationId, Amount = 200m, PaymentMethod = "Cash"
        });
        Assert.That(result, Is.InstanceOf<CreatedResult>());
    }

    [Test]
    public async Task Pay_ExceedsBalance_Returns409()
    {
        var result = await _controller.MockPayment(new MockPaymentRequestDto
        {
            ReservationId = _reservationId, Amount = 500m, PaymentMethod = "Cash"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Pay_ZeroAmount_ReturnsBadRequest()
    {
        var result = await _controller.MockPayment(new MockPaymentRequestDto
        {
            ReservationId = _reservationId, Amount = 0m, PaymentMethod = "Cash"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Pay_InvalidMethod_ReturnsBadRequest()
    {
        var result = await _controller.MockPayment(new MockPaymentRequestDto
        {
            ReservationId = _reservationId, Amount = 100m, PaymentMethod = "Bitcoin"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Pay_CancelledReservation_Returns409()
    {
        var r = await _db.Reservations.FindAsync(_reservationId);
        r!.ReservationStatus = "Cancelled";
        await _db.SaveChangesAsync();

        var result = await _controller.MockPayment(new MockPaymentRequestDto
        {
            ReservationId = _reservationId, Amount = 100m, PaymentMethod = "Cash"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Pay_NotFound_Returns404()
    {
        var result = await _controller.MockPayment(new MockPaymentRequestDto
        {
            ReservationId = 9999, Amount = 100m, PaymentMethod = "Cash"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetPayments_ValidReservation_ReturnsOk()
    {
        var result = await _controller.GetByReservation(_reservationId);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetPayments_MissingReservation_Returns404()
    {
        var result = await _controller.GetByReservation(9999);
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(404));
    }
}
