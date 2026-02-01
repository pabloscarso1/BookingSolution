using FluentValidation;
using VehicleService.Application.Common;
using VehicleService.Application.Dtos;
using VehicleService.Application.Exceptions;
using VehicleService.Application.Interfaces;

namespace VehicleService.Application.Features.GetVehicle
{
    public record GetVehicleByIdQuery(Guid id);

    public class GetVehicleByIdQueryHandler
    {
        private readonly IVehicleRepository _repository;
        private readonly IValidator<GetVehicleByIdQuery> _validator;

        public GetVehicleByIdQueryHandler(
            IVehicleRepository repository,
            IValidator<GetVehicleByIdQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery query)
        {
            // Validar el query
            return await _validator.ValidateAndExecuteAsync(query, async () =>
            {
                var obj = await _repository.GetAsync(x => x.Id == query.id);

                if (obj is null)
                    return Result<VehicleDto>.Failure(new NotFoundException("vehicle").Message);

                var dto = new VehicleDto(obj.Id, obj.UserId, obj.Patent, obj.Model, obj.Year, obj.Color);

                return Result<VehicleDto>.Success(dto);
            });
        }
    }

    public class GetVehicleByIdQueryValidator : AbstractValidator<GetVehicleByIdQuery>
    {
        public GetVehicleByIdQueryValidator()
        {
            RuleFor(x => x.id)
                .NotEmpty()
                .WithMessage("El ID del vehículo es requerido");
        }
    }
}
