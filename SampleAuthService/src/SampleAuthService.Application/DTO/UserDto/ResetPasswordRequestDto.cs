using System.ComponentModel.DataAnnotations;

namespace SampleAuthService.Application.DTO.UserDto;

public class ResetPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
