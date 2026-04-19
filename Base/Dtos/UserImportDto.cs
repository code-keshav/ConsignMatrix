namespace Base.Dtos;

public class UserImportDto
{
    public int RowNumber { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ContactNo { get; set; }
    public string? Address { get; set; }
    public string BranchCode { get; set; }
    public string Role { get; set; }
    public string UserLevel { get; set; }  // Will be parsed to int
    public string? Password { get; set; }  // Optional - uses default if null
}

public class UserImportRequestDto
{
    public List<UserImportDto> Users { get; set; }
    public string DefaultPassword { get; set; }  // Fallback password
}

public class UserImportResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<UserImportErrorDto> Errors { get; set; } = new();
}

public class UserImportErrorDto
{
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; }
}
