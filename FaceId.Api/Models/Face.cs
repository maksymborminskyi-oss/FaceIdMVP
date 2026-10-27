namespace FaceId.Api.Models;

public class Face
{
    public ICollection<FacePhoto> Photos { get; set; } = new List<FacePhoto>();

    public int Id { get; set; }
    public int AppUserId { get; set; }
    public float[]? Embedding { get; set; }
    public DateTime CreatedAt { get; set; }
    public AppUser User { get; set; } = null!;
}