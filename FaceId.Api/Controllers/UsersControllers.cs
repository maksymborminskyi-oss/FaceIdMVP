using FaceId.Api.Data;
using FaceId.Api.DTOs;
using FaceId.Api.Models;
using FaceId.Api.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FaceId.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly FaceIdDbContext _db;

    public UsersController(FaceIdDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users.ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request,
        CreateUserRequestValidator validator)
    {
        if (!validator.IsValid(request))
        {
            return BadRequest(new
            {
                error = "Invalid request data."
            });
        }

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException postgresException
                && postgresException.SqlState == "23505")
        {
            return Conflict(new
            {
                error = "A user with this email already exists."
            });
        }

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id},
            user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        UpdateUserRequest request,
        UpdateUserRequestValidator validator)
    {
        if (!validator.IsValid(request))
        {
            return BadRequest(new
            {
                error = "Invalid reqest data." 
            });
        }  

        var user = await _db.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException postgresException
                && postgresException.SqlState == "23505")
        {
            return Conflict(new
            {
                error = "A user wiht this email already exists."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(
        int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        _db.Users.Remove(user);

        await _db.SaveChangesAsync();
        return NoContent();
    }
} 