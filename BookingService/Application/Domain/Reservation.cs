namespace BookingService.Application.Domain
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public Guid VehicleId { get; set; }
        public decimal ReservationCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        protected Reservation() { }

        public Reservation(Guid id, Guid userId, Guid vehicleId, DateTime startDate, DateTime endDate, DateTime createdAt, decimal reservationCost)
        {
            Id = id;
            UserId = userId;
            VehicleId = vehicleId;
            StartDate = startDate;
            EndDate = endDate;
            CreatedAt = createdAt;
            ReservationCost = reservationCost;
        }
    }
}
