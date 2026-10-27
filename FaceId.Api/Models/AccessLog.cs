namespace FaceId.Api.Models;

public class AccessLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public DateTime RecognizedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public AppUser? User { get; set; }
}