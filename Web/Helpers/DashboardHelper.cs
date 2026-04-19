using Base.Enum.Consignment;

namespace Web.Helpers;

public static class DashboardHelper
{
    public static string GetStatusBadgeClass(ConsignmentStatusType? status) => status switch
    {
        ConsignmentStatusType.Booked => "bg-secondary",
        ConsignmentStatusType.PickupScheduled => "bg-info",
        ConsignmentStatusType.PickupAttempted => "bg-warning",
        ConsignmentStatusType.PickedUp => "bg-info",
        ConsignmentStatusType.ReceivedAtOrigin => "bg-info",
        ConsignmentStatusType.Sorted => "bg-info",
        ConsignmentStatusType.Bagged => "bg-info",
        ConsignmentStatusType.Dispatched => "bg-primary",
        ConsignmentStatusType.InTransit => "bg-primary",
        ConsignmentStatusType.ArrivedAtHub => "bg-primary",
        ConsignmentStatusType.DepartedHub => "bg-primary",
        ConsignmentStatusType.ReceivedAtDestination => "bg-primary",
        ConsignmentStatusType.OutForDelivery => "bg-primary",
        ConsignmentStatusType.DeliveryAttempted => "bg-warning",
        ConsignmentStatusType.Delivered => "bg-success",
        ConsignmentStatusType.HeldAtBranch => "bg-warning",
        ConsignmentStatusType.RtsInitiated => "bg-warning",
        ConsignmentStatusType.ReturnedToSender => "bg-secondary",
        ConsignmentStatusType.Damaged => "bg-danger",
        ConsignmentStatusType.Lost => "bg-danger",
        _ => "bg-secondary"
    };

    public static string GetPickupStatusBadgeClass(PickupTaskStatus status) => status switch
    {
        PickupTaskStatus.Pending => "bg-secondary",
        PickupTaskStatus.Assigned => "bg-info",
        PickupTaskStatus.InProgress => "bg-primary",
        PickupTaskStatus.Completed => "bg-success",
        PickupTaskStatus.Failed => "bg-danger",
        PickupTaskStatus.Cancelled => "bg-secondary",
        _ => "bg-secondary"
    };

    public static string GetTripStatusBadgeClass(TripStatus status) => status switch
    {
        TripStatus.Scheduled => "bg-info",
        TripStatus.InTransit => "bg-primary",
        TripStatus.Completed => "bg-success",
        TripStatus.Cancelled => "bg-secondary",
        _ => "bg-secondary"
    };

    public static string GetIssueBadgeClass(ConsignmentStatusType status) => status switch
    {
        ConsignmentStatusType.DeliveryAttempted => "bg-warning",
        ConsignmentStatusType.PickupAttempted => "bg-warning",
        ConsignmentStatusType.Damaged => "bg-danger",
        ConsignmentStatusType.Lost => "bg-danger",
        ConsignmentStatusType.HeldAtBranch => "bg-warning",
        ConsignmentStatusType.RtsInitiated => "bg-warning",
        _ => "bg-secondary"
    };

    public static string GetCapacityBarClass(int percent) => percent switch
    {
        <= 60 => "bg-success",
        <= 85 => "bg-warning",
        _ => "bg-danger"
    };

    public static string GetStatusDisplayName(ConsignmentStatusType status) => status switch
    {
        ConsignmentStatusType.PickupScheduled => "Pickup Scheduled",
        ConsignmentStatusType.PickupAttempted => "Pickup Attempted",
        ConsignmentStatusType.PickedUp => "Picked Up",
        ConsignmentStatusType.ReceivedAtOrigin => "Received at Origin",
        ConsignmentStatusType.InTransit => "In Transit",
        ConsignmentStatusType.ArrivedAtHub => "Arrived at Hub",
        ConsignmentStatusType.DepartedHub => "Departed Hub",
        ConsignmentStatusType.ReceivedAtDestination => "Received at Destination",
        ConsignmentStatusType.OutForDelivery => "Out for Delivery",
        ConsignmentStatusType.DeliveryAttempted => "Delivery Attempted",
        ConsignmentStatusType.HeldAtBranch => "Held at Branch",
        ConsignmentStatusType.RtsInitiated => "RTS Initiated",
        ConsignmentStatusType.ReturnedToSender => "Returned to Sender",
        _ => status.ToString()
    };

    public static string GetStatusChartColor(ConsignmentStatusType status) => status switch
    {
        ConsignmentStatusType.Booked => "#6c757d",
        ConsignmentStatusType.PickupScheduled => "#17a2b8",
        ConsignmentStatusType.PickupAttempted => "#ffc107",
        ConsignmentStatusType.PickedUp => "#17a2b8",
        ConsignmentStatusType.ReceivedAtOrigin => "#20c997",
        ConsignmentStatusType.Sorted => "#20c997",
        ConsignmentStatusType.Bagged => "#20c997",
        ConsignmentStatusType.Dispatched => "#0d6efd",
        ConsignmentStatusType.InTransit => "#0d6efd",
        ConsignmentStatusType.ArrivedAtHub => "#0d6efd",
        ConsignmentStatusType.DepartedHub => "#0d6efd",
        ConsignmentStatusType.ReceivedAtDestination => "#6610f2",
        ConsignmentStatusType.OutForDelivery => "#0dcaf0",
        ConsignmentStatusType.DeliveryAttempted => "#fd7e14",
        ConsignmentStatusType.Delivered => "#198754",
        ConsignmentStatusType.HeldAtBranch => "#ffc107",
        ConsignmentStatusType.RtsInitiated => "#fd7e14",
        ConsignmentStatusType.ReturnedToSender => "#6c757d",
        ConsignmentStatusType.Damaged => "#dc3545",
        ConsignmentStatusType.Lost => "#dc3545",
        _ => "#adb5bd"
    };
}
