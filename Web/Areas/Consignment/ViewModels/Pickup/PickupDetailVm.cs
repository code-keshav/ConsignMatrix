using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Pickup;

public class PickupDetailVm
{
    public long Id { get; set; }
    public long ConsignmentId { get; set; }
    public string TrackingNumber { get; set; }

    // Sender info
    public string SenderName { get; set; }
    public string SenderPhone { get; set; }

    // Pickup details
    public string PickupAddress { get; set; }
    public string ContactPhone { get; set; }
    public string? ContactName { get; set; }
    public DateTime PickupDate { get; set; }
    public PickupSlot PickupSlot { get; set; }
    public PickupTaskStatus TaskStatus { get; set; }

    // Assignment
    public string? AssignedDriverName { get; set; }
    public string? AssignedVehicleNumber { get; set; }

    // Execution
    public int AttemptCount { get; set; }
    public DateTime? PickupTime { get; set; }
    public decimal? VerifiedWeight { get; set; }
    public PickupFailReason? FailReason { get; set; }
    public string? Remarks { get; set; }

    // Consignment summary
    public decimal ConsignmentWeight { get; set; }
    public int PackageCount { get; set; }
    public string? ServiceType { get; set; }
    public string? PaymentMode { get; set; }

    public DateTime RecDate { get; set; }

    // Attempt history
    public List<PickupAttemptVm> AttemptHistory { get; set; } = new();

    // Whether there's an active pickup (for showing action buttons)
    public bool HasActivePickup { get; set; }
}

public class PickupAttemptVm
{
    public long Id { get; set; }
    public DateTime PickupDate { get; set; }
    public PickupSlot PickupSlot { get; set; }
    public PickupTaskStatus TaskStatus { get; set; }
    public string? DriverName { get; set; }
    public PickupFailReason? FailReason { get; set; }
    public string? Remarks { get; set; }
    public DateTime RecDate { get; set; }
}
