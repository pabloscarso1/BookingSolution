namespace VehicleService.Application.Dtos
{
    public record VehicleDto(Guid Id, Guid UsuarioId, string Patent, string Model, int Year, string Color);
}
