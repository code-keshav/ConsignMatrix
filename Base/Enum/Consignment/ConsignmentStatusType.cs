namespace Base.Enum.Consignment;

public enum ConsignmentStatusType
{
    Booked = 1,
    PickupScheduled = 2,
    PickupAttempted = 3,
    PickedUp = 4,
    ReceivedAtOrigin = 5,
    Sorted = 6,
    Bagged = 7,
    Dispatched = 8,
    InTransit = 9,
    ArrivedAtHub = 10,
    DepartedHub = 11,
    ReceivedAtDestination = 12,
    OutForDelivery = 13,
    DeliveryAttempted = 14,
    Delivered = 15,
    HeldAtBranch = 16,
    RtsInitiated = 17,
    ReturnedToSender = 18,
    Damaged = 19,
    Lost = 20
}
