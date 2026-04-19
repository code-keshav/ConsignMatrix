using Base.Entities;

namespace Base.Dtos;

public class RoleDto
{
    public string Name { get; set; }
    public long? Priority { get; set; }
    public Branch Branch { get; set; }
    public bool IsGlobal { get; set; }
}