using FluentValidation;
using Common.Application.Common;
using BaseService.Application.Dtos;
using BaseService.Application.Interfaces;

namespace BaseService.Application.Features.GetUser
{
    public record GetBaseByIdQuery(Guid id);

    public class GetBaseByIdQueryHandler
    {
        private readonly IBaseRepository _repository;
        private readonly IValidator<GetBaseByIdQuery> _validator;

        public GetBaseByIdQueryHandler(
            IBaseRepository repository,
            IValidator<GetBaseByIdQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<BaseDto>> Handle(GetBaseByIdQuery query)
        {
            // Validar el query
            return await _validator.ValidateAndExecuteAsync(query, async () =>
            {
                var obj = await _repository.GetAsync(x => x.Id == query.id);

                if (obj is null)
                    return Result<BaseDto>.Failure("USER_NOT_FOUND");

                var dto = new BaseDto(obj.Id, obj.Name);

                return Result<BaseDto>.Success(dto);
            });
        }
    }
}
