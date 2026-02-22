using System.ComponentModel.DataAnnotations;
using SampleAuthService.Domain.Enums;

namespace SampleAuthService.Application.DTO.UserDto;

public class RegisterUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; } = UserRole.ReadUser;
}
