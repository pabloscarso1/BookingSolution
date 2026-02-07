namespace VehicleService.Application.Domain
{
    public class Vehicle
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public string Patent { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public decimal BookingCost { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        protected Vehicle() { }

        public Vehicle(Guid userId, string patent, string model, int year, string color, decimal bookingCost)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Patent = patent;
            Model = model;
            Year = year;
            Color = color;
            BookingCost = bookingCost;
        }

        public Vehicle(Guid id, Guid usuarioId, string patent, string model, int year, string color)
        {
            Id = id;
            UserId = usuarioId;
            Patent = patent;
            Model = model;
            Year = year;
            Color = color;
        }
    }
}
