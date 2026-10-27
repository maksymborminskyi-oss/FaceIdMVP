using FaceId.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaceId.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessLogsController : ControllerBase
{
    private readonly FaceIdDbContext _db;

    public AccessLogsController(FaceIdDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var logs = await _db.AccessLogs
            .AsNoTracking()
            .Include(log => log.User)
            .OrderByDescending(log => log.RecognizedAt)
            .Select(log => new
            {
                id = log.Id,
                userId = log.UserId,
                recognizedAt = log.RecognizedAt,
                status = log.Status,
                user = log.User == null
                    ? null
                    : new
                    {
                        id = log.User.Id,
                        firstName = log.User.FirstName,
                        lastName = log.User.LastName,
                        email = log.User.Email
                    }
            })
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }
}