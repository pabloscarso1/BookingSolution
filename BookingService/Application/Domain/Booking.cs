namespace BookingService.Application.Domain
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public Guid VehicleId { get; set; }
        public decimal BookingCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        protected Booking() { }

        public Booking(Guid id, Guid userId, Guid vehicleId, DateTime startDate, DateTime endDate, DateTime createdAt, decimal bookingCost)
        {
            Id = id;
            UserId = userId;
            VehicleId = vehicleId;
            StartDate = startDate;
            EndDate = endDate;
            CreatedAt = createdAt;
            BookingCost = bookingCost;
        }
    }
}
