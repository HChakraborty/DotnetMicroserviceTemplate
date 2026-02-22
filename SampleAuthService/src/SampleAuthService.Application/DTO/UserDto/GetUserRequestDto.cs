using SampleAuthService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAuthService.Application.DTO.UserDto;

public class GetUserDto
{
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.ReadUser;
}
