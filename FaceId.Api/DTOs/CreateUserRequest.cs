using System.ComponentModel.DataAnnotations;

namespace FaceId.Api.DTOs;

public class CreateUserRequest
{
    [Required]
    public string FirstName{ get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}