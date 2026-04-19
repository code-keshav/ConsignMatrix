namespace Web.Areas.Admin.Requests;

public class UserResetPasswordVm
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string NewPassword { get; set; }
    
}