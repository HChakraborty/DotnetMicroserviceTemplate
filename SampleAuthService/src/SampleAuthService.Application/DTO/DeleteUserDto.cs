using System.ComponentModel.DataAnnotations;

namespace SampleAuthService.Application.DTO;

public class DeleteUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
