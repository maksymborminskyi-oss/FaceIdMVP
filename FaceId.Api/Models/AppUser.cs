namespace FaceId.Api.Models;

public class AppUser
{
    public ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();

    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Face? Face { get; set; }
}