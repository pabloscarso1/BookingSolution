namespace BookingService.Application.Dtos
{
    public record ReservationDto(
        Guid Id,
        Guid UserId,
        Guid VehicleId,
        decimal ReservationCost,
        DateTime StartDate,
        DateTime EndDate,
        DateTime CreatedAt);
}
