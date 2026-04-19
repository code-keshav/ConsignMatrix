namespace Base.Dtos;

public class BranchPinCodeDto
{
    public long BranchId { get; set; }
    public string PinCode { get; set; }
    public bool IsActive { get; set; } = true;
}
