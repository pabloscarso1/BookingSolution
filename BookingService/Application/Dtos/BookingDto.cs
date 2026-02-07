namespace BookingService.Application.Dtos
{
    public record BookingDto(
        Guid Id,
        Guid UserId,
        Guid VehicleId,
        decimal BookingCost,
        DateTime StartDate,
        DateTime EndDate,
        DateTime CreatedAt);
}
