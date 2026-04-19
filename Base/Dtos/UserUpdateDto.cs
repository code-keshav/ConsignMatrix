namespace Base.Dtos;

public class UserUpdateDto
{
    public string Name { get; set; }
    public string ContactNo { get; set; }
    public string? Address { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}