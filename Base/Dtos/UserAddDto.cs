using Base.Enum;

namespace Base.Dtos;

public class UserAddDto
{
    public string Name { get; set; }
    public string Password { get; set; }
    public string ContactNo { get; set; }
    public string? Address { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public UserLevel UserLevel { get; set; } = UserLevel.User;
    public bool IsActive { get; set; } = true;
}