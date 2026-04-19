using Base.Dtos.Consignment;
using Base.Entities.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface ITripService
{
    Task<Trip> Create(TripCreateDto dto);
    Task StartTrip(long tripId);
    Task CompleteTrip(long tripId);
    Task CancelTrip(long tripId);
    Task ReceiveTripItem(TripReceiveItemDto dto);
    Task MarkDelivered(MarkDeliveredDto dto);
    Task MarkDeliveryFailed(MarkDeliveryFailedDto dto);
}
