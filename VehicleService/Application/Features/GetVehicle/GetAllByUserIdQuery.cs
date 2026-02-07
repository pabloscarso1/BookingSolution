using FluentValidation;
using Common.Application.Common;
using VehicleService.Application.Dtos;
using Common.Application.Exceptions;
using VehicleService.Application.Interfaces;

namespace VehicleService.Application.Features.GetVehicle
{
    public record GetAllByUserIdQuery(Guid userId);

    public class GetAllByUserIdQueryHandler
    {
        private readonly IVehicleRepository _repository;
        private readonly IValidator<GetAllByUserIdQuery> _validator;

        public GetAllByUserIdQueryHandler(
            IVehicleRepository repository,
            IValidator<GetAllByUserIdQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<List<VehicleDto>>> Handle(GetAllByUserIdQuery query)
        {
            // Validar el query
            return await _validator.ValidateAndExecuteAsync(query, async () =>
            {
                var list = await _repository.ListAsync(x => x.UserId == query.userId);

                List<VehicleDto> dtoList = [];

                foreach (var item in list)
                    dtoList.Add(new VehicleDto(item.Id, item.UserId, item.Patent, item.Model, item.Year, item.Color));

                return Result<List<VehicleDto>>.Success(dtoList);
            });
        }
    }

    public class GetAllByUserIdQueryValidator : AbstractValidator<GetAllByUserIdQuery>
    {
        public GetAllByUserIdQueryValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty()
                .WithMessage("El ID del usuario es requerido");
        }
    }
}
