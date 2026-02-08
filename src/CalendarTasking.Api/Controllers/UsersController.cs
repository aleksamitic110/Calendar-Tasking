using CalendarTasking.Api.Contracts;
using CalendarTasking.Api.Data;
using CalendarTasking.Api.Models;
using CalendarTasking.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalendarTasking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(CalendarTaskingDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.UserId)
            .ToListAsync();

        return Ok(users.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.ToResponse());
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail))
        {
            return Conflict("A user with this email already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "UTC" : request.TimeZoneId.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user.ToResponse());
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResponse>> Login([FromBody] LoginUserRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash) || !user.IsActive)
        {
            return Unauthorized("Invalid credentials.");
        }

        return Ok(new LoginUserResponse(user.UserId, user.Email, $"{user.FirstName} {user.LastName}"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (user is null)
        {
            return NotFound();
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        if (await dbContext.Users.AnyAsync(x => x.UserId != id && x.Email == normalizedEmail))
        {
            return Conflict("A user with this email already exists.");
        }

        user.Email = normalizedEmail;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "UTC" : request.TimeZoneId.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(user.ToResponse());
    }

    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (user is null)
        {
            return NotFound();
        }

        if (!PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Current password is incorrect.");
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (user is null)
        {
            return NotFound();
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
