using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Admin.ViewModels;

public class BranchImportVm
{
    [Required(ErrorMessage = "Please select a file")]
    public IFormFile File { get; set; }
}

public class BranchImportResultVm
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<BranchImportErrorVm> Errors { get; set; } = new();
}

public class BranchImportErrorVm
{
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; }
}
