using HotelService.Controllers;
using HotelService.Data;
using HotelService.DTOs;
using HotelService.Models;
using HotelService.Repositories;
using HotelService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net;

namespace HotelService_Tests;

[TestFixture]
public class RoomsControllerTests
{
    private HotelDbContext _db = null!;
    private RoomsController _controller = null!;
    private int _roomTypeId;

    private static IHttpClientFactory MakeFactory(bool hasActive = false)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(hasActive);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:5103") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("BookingService")).Returns(client);
        return factory.Object;
    }

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new HotelDbContext(options);

        var rt = new RoomType { TypeName = "Standard", Capacity = 2 };
        _db.RoomTypes.Add(rt);
        await _db.SaveChangesAsync();
        _roomTypeId = rt.RoomTypeId;

        var roomRepo = new RoomRepository(_db);
        var typeRepo = new RoomTypeRepository(_db);
        var service = new RoomService(roomRepo, typeRepo, MakeFactory());
        _controller = new RoomsController(service);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Create_Valid_Returns201()
    {
        var result = await _controller.Create(new CreateRoomRequestDto
        {
            RoomNumber = "101", RoomTypeId = _roomTypeId, PricePerNight = 150m, Status = "Available"
        });
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_DuplicateNumber_Returns409()
    {
        _db.Rooms.Add(new Room { RoomNumber = "101", RoomTypeId = _roomTypeId, PricePerNight = 100, Status = "Available" });
        await _db.SaveChangesAsync();

        var result = await _controller.Create(new CreateRoomRequestDto
        {
            RoomNumber = "101", RoomTypeId = _roomTypeId, PricePerNight = 150m, Status = "Available"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Create_NegativePrice_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateRoomRequestDto
        {
            RoomNumber = "102", RoomTypeId = _roomTypeId, PricePerNight = -10m, Status = "Available"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_InvalidStatus_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateRoomRequestDto
        {
            RoomNumber = "103", RoomTypeId = _roomTypeId, PricePerNight = 100m, Status = "BadStatus"
        });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetById_NotFound_Returns404()
    {
        var result = await _controller.GetById(999);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdatePrice_NegativePrice_ReturnsBadRequest()
    {
        var room = new Room { RoomNumber = "201", RoomTypeId = _roomTypeId, PricePerNight = 100, Status = "Available" };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        var result = await _controller.UpdatePrice(room.RoomId, new UpdateRoomPriceRequestDto { PricePerNight = -5m });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }
}
