namespace BookingService.Api.Contracts
{
    public record CreateBookingRequest(
        Guid UserId,
        Guid VehicleId,
        DateTime StartDate,
        DateTime EndDate,
        decimal BookingCost);
}
