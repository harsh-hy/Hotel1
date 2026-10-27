using HotelService.Controllers;
using HotelService.Data;
using HotelService.DTOs;
using HotelService.Models;
using HotelService.Repositories;
using HotelService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace HotelService_Tests;

[TestFixture]
public class RoomTypesControllerTests
{
    private HotelDbContext _db = null!;
    private RoomTypesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new HotelDbContext(options);
        var repo = new RoomTypeRepository(_db);
        var service = new RoomTypeService(repo);
        _controller = new RoomTypesController(service);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Create_Valid_Returns201()
    {
        var result = await _controller.Create(new CreateRoomTypeRequestDto { TypeName = "Deluxe", Capacity = 2 });
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_DuplicateName_Returns409()
    {
        _db.RoomTypes.Add(new RoomType { TypeName = "Suite", Capacity = 3 });
        await _db.SaveChangesAsync();

        var result = await _controller.Create(new CreateRoomTypeRequestDto { TypeName = "Suite", Capacity = 2 });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Create_ZeroCapacity_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateRoomTypeRequestDto { TypeName = "Economy", Capacity = 0 });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Delete_WithRooms_Returns409()
    {
        var rt = new RoomType { TypeName = "TypeWithRooms", Capacity = 2 };
        _db.RoomTypes.Add(rt);
        await _db.SaveChangesAsync();
        _db.Rooms.Add(new Room { RoomNumber = "101", RoomTypeId = rt.RoomTypeId, PricePerNight = 100, Status = "Available" });
        await _db.SaveChangesAsync();

        var result = await _controller.Delete(rt.RoomTypeId);
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Delete_NoRooms_Returns204()
    {
        var rt = new RoomType { TypeName = "EmptyType", Capacity = 2 };
        _db.RoomTypes.Add(rt);
        await _db.SaveChangesAsync();

        var result = await _controller.Delete(rt.RoomTypeId);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}
