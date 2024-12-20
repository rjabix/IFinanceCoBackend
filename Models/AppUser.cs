using Microsoft.AspNetCore.Identity;

namespace IFinanceCoBackend.Models;

public class AppUser : IdentityUser
{
    public UserStatus Status { get; set; } = UserStatus.Free;
}

public enum UserStatus
{
    Free,
    Pro,
    Inactive
}