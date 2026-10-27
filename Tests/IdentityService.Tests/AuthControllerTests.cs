using IdentityService.Controllers;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace IdentityService_Tests;

[TestFixture]
public class AuthControllerTests
{
    private IdentityDbContext _db = null!;
    private IConfiguration _config = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TEST_SECRET_KEY_THAT_IS_LONG_ENOUGH_FOR_HMACSHA256",
                ["Jwt:Issuer"] = "OHMS",
                ["Jwt:Audience"] = "OHMS-Staff",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        var hasher = new PasswordHasher<object>();
        _db.Users.Add(new User
        {
            Username = "testuser",
            PasswordHash = hasher.HashPassword(new object(), "ValidPass@123"),
            FullName = "Test User",
            Role = "Receptionist",
            IsActive = true
        });
        _db.SaveChanges();

        var repo = new UserRepository(_db);
        var authService = new AuthService(repo, _config);
        _controller = new AuthController(authService);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var result = await _controller.Login(new LoginRequestDto { Username = "testuser", Password = "ValidPass@123" });
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var result = await _controller.Login(new LoginRequestDto { Username = "testuser", Password = "WrongPassword" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_UnknownUser_ReturnsUnauthorized()
    {
        var result = await _controller.Login(new LoginRequestDto { Username = "nobody", Password = "ValidPass@123" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_InactiveUser_ReturnsUnauthorized()
    {
        var hasher = new PasswordHasher<object>();
        _db.Users.Add(new User
        {
            Username = "inactive",
            PasswordHash = hasher.HashPassword(new object(), "ValidPass@123"),
            FullName = "Inactive",
            Role = "Receptionist",
            IsActive = false
        });
        await _db.SaveChangesAsync();

        var result = await _controller.Login(new LoginRequestDto { Username = "inactive", Password = "ValidPass@123" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
}
