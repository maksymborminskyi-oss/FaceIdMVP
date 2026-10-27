namespace FaceId.Api.Models;

public class FacePhoto
{
    public int Id { get; set; }
    public int FaceId { get; set; }
    public string PhotoPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Face Face { get; set; } = null!;
}