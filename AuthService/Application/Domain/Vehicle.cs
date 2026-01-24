namespace AuthService.Application.Domain
{
    public class Vehicle
    {
        public Guid Id { get; private set; }
        public Guid UsuarioId { get; set; }
        public string Patent { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;

        protected Vehicle() { }

        public Vehicle(string patent, string model, int year, string color)
        {
            Patent = patent;
            Model = model;
            Year = year;
            Color = color;
        }

        public Vehicle(Guid id, Guid usuarioId, string patent, string model, int year, string color)
        {
            Id = id;
            UsuarioId = usuarioId;
            Patent = patent;
            Model = model;
            Year = year;
            Color = color;
        }
    }
}
