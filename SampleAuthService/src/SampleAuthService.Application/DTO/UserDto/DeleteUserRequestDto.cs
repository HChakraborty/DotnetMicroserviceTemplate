using System.ComponentModel.DataAnnotations;

namespace SampleAuthService.Application.DTO.UserDto;

public class DeleteUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
