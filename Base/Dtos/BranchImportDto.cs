namespace Base.Dtos;

public class BranchImportDto
{
    public int RowNumber { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? BranchType { get; set; }
    public string Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string ContactNo { get; set; }
    public string? Email { get; set; }
}

public class BranchImportResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<BranchImportErrorDto> Errors { get; set; } = new();
}

public class BranchImportErrorDto
{
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; }
}
