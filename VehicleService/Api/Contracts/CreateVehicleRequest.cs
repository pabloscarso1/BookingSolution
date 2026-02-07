using FluentValidation;

namespace VehicleService.Api.Contracts
{
    public record CreateVehicleRequest(Guid UserId, string Patent, string Model, int Year, string Color, decimal BookingCost);

    //public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
    //{
    //    public CreateVehicleRequestValidator()
    //    {
    //        RuleFor(x => x.Patent)
    //            .NotEmpty()
    //            .WithMessage("La patente es requerida")
    //            .MaximumLength(7)
    //            .WithMessage("La patente no puede exceder los 7 caracteres");
    //
    //        RuleFor(x => x.Model)
    //            .NotEmpty()
    //            .WithMessage("El modelo es requerido");
    //    }
    //}
}

