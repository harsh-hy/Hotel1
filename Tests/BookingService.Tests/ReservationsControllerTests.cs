using BookingService.Controllers;
using BookingService.Data;
using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net;

namespace BookingService_Tests;

[TestFixture]
public class ReservationsControllerTests
{
    private BookingDbContext _db = null!;
    private int _guestId;

    private static IHttpClientFactory MakeHotelFactory(int capacity = 4, decimal price = 200m, string status = "Available")
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            roomId = 1, roomNumber = "R001", roomTypeId = 1,
            roomTypeName = "Standard", roomTypeCapacity = capacity,
            pricePerNight = price, status, description = (string?)null
        });
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:5102") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("HotelService")).Returns(client);
        return factory.Object;
    }

    private static IHttpClientFactory MakeFailingFactory()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("unavailable"));
        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:5102") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("HotelService")).Returns(client);
        return factory.Object;
    }

    private ReservationsController MakeController(IHttpClientFactory? factory = null)
    {
        var repo = new ReservationRepository(_db);
        var service = new ReservationService(repo, factory ?? MakeHotelFactory());
        var ctrl = new ReservationsController(service);
        ctrl.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return ctrl;
    }

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new BookingDbContext(options);

        var guest = new Guest { FullName = "Test Guest", PhoneNumber = "0000000001" };
        _db.Guests.Add(guest);
        await _db.SaveChangesAsync();
        _guestId = guest.GuestId;
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private DateOnly Future(int days) => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

    [Test]
    public async Task Create_Valid_Returns201()
    {
        var result = await MakeController().Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(3),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_CheckOutBeforeCheckIn_ReturnsBadRequest()
    {
        var result = await MakeController().Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(5), CheckOutDate = Future(3),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_PastDate_ReturnsBadRequest()
    {
        var result = await MakeController().Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(-2), CheckOutDate = Future(3),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_InvalidGuest_ReturnsBadRequest()
    {
        var result = await MakeController().Create(new CreateReservationRequestDto
        {
            GuestId = 9999, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(3),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_HotelUnavailable_ReturnsBadRequest()
    {
        var result = await MakeController(MakeFailingFactory()).Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(3),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_OverlapConflict_Returns409()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(4),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 200m, TotalAmount = 600m,
            ReservationStatus = "Booked"
        });
        await _db.SaveChangesAsync();

        var result = await MakeController().Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(2), CheckOutDate = Future(5),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Create_CancelledDoesNotBlock_Returns201()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(4),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 200m, TotalAmount = 600m,
            ReservationStatus = "Cancelled"
        });
        await _db.SaveChangesAsync();

        var result = await MakeController().Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(4),
            NumberOfAdults = 2, NumberOfChildren = 0
        });
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Cancel_Booked_Returns204()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(2), CheckOutDate = Future(4),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 100m, TotalAmount = 200m,
            ReservationStatus = "Booked"
        });
        await _db.SaveChangesAsync();
        var res = await _db.Reservations.FirstAsync();

        var result = await MakeController().Cancel(res.ReservationId);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Cancel_CheckedIn_Returns409()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(0), CheckOutDate = Future(2),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 100m, TotalAmount = 200m,
            ReservationStatus = "CheckedIn"
        });
        await _db.SaveChangesAsync();
        var res = await _db.Reservations.FirstAsync();

        var result = await MakeController().Cancel(res.ReservationId);
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task CheckIn_Booked_Returns204()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(0), CheckOutDate = Future(2),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 100m, TotalAmount = 100m,
            ReservationStatus = "Booked"
        });
        await _db.SaveChangesAsync();
        var res = await _db.Reservations.FirstAsync();

        var result = await MakeController().CheckIn(res.ReservationId);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task CheckOut_CheckedIn_Returns204()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(-1), CheckOutDate = Future(1),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 100m, TotalAmount = 200m,
            ReservationStatus = "CheckedIn"
        });
        await _db.SaveChangesAsync();
        var res = await _db.Reservations.FirstAsync();

        var result = await MakeController().CheckOut(res.ReservationId);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task CheckOut_NotCheckedIn_Returns409()
    {
        _db.Reservations.Add(new Reservation
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(3),
            NumberOfAdults = 1, NumberOfChildren = 0,
            PricePerNightSnapshot = 100m, TotalAmount = 200m,
            ReservationStatus = "Booked"
        });
        await _db.SaveChangesAsync();
        var res = await _db.Reservations.FirstAsync();

        var result = await MakeController().CheckOut(res.ReservationId);
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Create_ExceedsCapacity_ReturnsBadRequest()
    {
        var result = await MakeController(MakeHotelFactory(capacity: 2)).Create(new CreateReservationRequestDto
        {
            GuestId = _guestId, RoomId = 1,
            CheckInDate = Future(1), CheckOutDate = Future(3),
            NumberOfAdults = 4, NumberOfChildren = 1 // 5 guests, capacity 2
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }
}
