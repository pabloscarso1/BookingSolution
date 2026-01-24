using FluentValidation;
using AuthService.Application.Common;
using AuthService.Application.Dtos;
using AuthService.Application.Interfaces;

namespace AuthService.Application.Features.CreateVehicle
{
    public record CreateVehicleCommand(Guid UsuarioId, string Patent, string Model, int Year, string Color);

    public class CreateVehicleHandler
    {
        private readonly IVehicleRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateVehicleCommand> _validator;

        public CreateVehicleHandler(
            IVehicleRepository repository,
            IUnitOfWork uof,
            IValidator<CreateVehicleCommand> validator)
        {
            _repository = repository;
            _unitOfWork = uof;
            _validator = validator;
        }

        public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand command)
        {
            // Validar el comando
            return await _validator.ValidateAndExecuteAsync(command, async () =>
            {
                var existing = _repository.GetAsync(x => x.Patent == command.Patent);

                if (existing is not null)
                    return Result<VehicleDto>.Failure("VEHICLE_ALREADY_EXISTS");

                var vehicle = new Domain.Vehicle(command.Patent, command.Model, command.Year, command.Color);

                await _repository.AddAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                var vehicleDto = new VehicleDto(vehicle.Id, vehicle.UsuarioId, vehicle.Patent, vehicle.Model, vehicle.Year, vehicle.Color);
                return Result<VehicleDto>.Success(vehicleDto);
            });
        }
    }

    public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
    {
        public CreateVehicleCommandValidator()
        {
            RuleFor(x => x.UsuarioId)
               .NotEmpty()
               .WithMessage("El usuario es requerido");

            RuleFor(x => x.Patent)
                .NotEmpty()
                .WithMessage("La patente es requerida")
                .MaximumLength(7)
                .WithMessage("La patente no puede exceder los 7 caracteres");

            RuleFor(x => x.Model)
               .NotEmpty()
               .WithMessage("El Modelo es requerido");

            RuleFor(x => x.Year)
               .NotEmpty()
               .WithMessage("El año es requerido");

            RuleFor(x => x.Color)
               .NotEmpty()
               .WithMessage("El color es requerido");
        }
    }
}
