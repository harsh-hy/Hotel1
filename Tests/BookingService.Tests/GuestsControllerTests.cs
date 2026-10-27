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
public class GuestsControllerTests
{
    private BookingDbContext _db = null!;
    private GuestsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new BookingDbContext(options);
        var repo = new GuestRepository(_db);
        var service = new GuestService(repo);
        _controller = new GuestsController(service);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Create_Valid_Returns201()
    {
        var result = await _controller.Create(new CreateGuestRequestDto { FullName = "Alice", PhoneNumber = "9876543210" });
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_MissingName_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateGuestRequestDto { FullName = "", PhoneNumber = "9876543210" });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_MissingPhone_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateGuestRequestDto { FullName = "Alice", PhoneNumber = "" });
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetById_Existing_ReturnsOk()
    {
        _db.Guests.Add(new Guest { FullName = "Bob", PhoneNumber = "1234567890" });
        await _db.SaveChangesAsync();
        var guest = await _db.Guests.FirstAsync();

        var result = await _controller.GetById(guest.GuestId);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_Missing_Returns404()
    {
        var result = await _controller.GetById(999);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Update_Valid_ReturnsOk()
    {
        _db.Guests.Add(new Guest { FullName = "Charlie", PhoneNumber = "1111111111" });
        await _db.SaveChangesAsync();
        var guest = await _db.Guests.FirstAsync();

        var result = await _controller.Update(guest.GuestId,
            new UpdateGuestRequestDto { FullName = "Charlie Updated", PhoneNumber = "2222222222" });
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
}
