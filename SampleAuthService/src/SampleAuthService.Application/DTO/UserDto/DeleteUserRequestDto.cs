using System.ComponentModel.DataAnnotations;

namespace SampleAuthService.Application.DTO.UserDto;

public class DeleteUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
