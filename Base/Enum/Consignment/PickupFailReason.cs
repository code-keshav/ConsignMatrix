namespace Base.Enum.Consignment;

public enum PickupFailReason
{
    CustomerNotAvailable = 1,
    AddressNotFound = 2,
    PackageNotReady = 3,
    WeightMismatch = 4,
    AccessRestricted = 5,
    WeatherConditions = 6,
    VehicleBreakdown = 7,
    CustomerRefused = 8,
    Other = 9
}
