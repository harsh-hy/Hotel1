using IdentityService.Controllers;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IdentityService_Tests;

[TestFixture]
public class UsersControllerTests
{
    private IdentityDbContext _db = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options);
        var repo = new UserRepository(_db);
        var service = new UserService(repo);
        _controller = new UsersController(service);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Create_ValidData_Returns201()
    {
        var result = await _controller.Create(new CreateUserRequestDto
        {
            Username = "newstaff", Password = "SecurePass@1234",
            FullName = "New Staff", Role = "Receptionist"
        });
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_DuplicateUsername_Returns409()
    {
        _db.Users.Add(new User { Username = "existing", PasswordHash = "h", FullName = "Ex", Role = "Receptionist" });
        await _db.SaveChangesAsync();

        var result = await _controller.Create(new CreateUserRequestDto
        {
            Username = "existing", Password = "SecurePass@1234",
            FullName = "New", Role = "Receptionist"
        });
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task Create_OwnerRole_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateUserRequestDto
        {
            Username = "staff2", Password = "SecurePass@1234",
            FullName = "Name", Role = "Owner"
        });
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_ShortPassword_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateUserRequestDto
        {
            Username = "staff3", Password = "short",
            FullName = "Name", Role = "Manager"
        });
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetById_NotFound_Returns404()
    {
        var result = await _controller.GetById(999);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAll_ReturnsOk()
    {
        var result = await _controller.GetAll();
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
}
