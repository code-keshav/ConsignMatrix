using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Admin.ViewModels;

public class UserImportVm
{
    [Required(ErrorMessage = "Please select a file")]
    public IFormFile File { get; set; }

    public string? DefaultPassword { get; set; } = string.Empty;
}

public class UserImportResultVm
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<UserImportErrorVm> Errors { get; set; } = new();
}

public class UserImportErrorVm
{
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; }
}
