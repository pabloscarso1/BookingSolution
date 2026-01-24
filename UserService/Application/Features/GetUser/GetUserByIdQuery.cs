using FluentValidation;
using UserService.Application.Common;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.Features.GetUser
{
    public record GetUserByIdQuery(Guid id);

    public class GetUserByIdHandler
    {
        private readonly IUserRepository _repository;
        private readonly IValidator<GetUserByIdQuery> _validator;

        public GetUserByIdHandler(
            IUserRepository repository,
            IValidator<GetUserByIdQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery query)
        {
            // Validar el query
            return await _validator.ValidateAndExecuteAsync(query, async () =>
            {
                var user = await _repository.GetAsync(x => x.Id == query.id);

                if (user is null)
                    return Result<UserDto>.Failure("USER_NOT_FOUND");

                var dto = new UserDto(user.Id, user.Name);

                return Result<UserDto>.Success(dto);
            });
        }
    }

    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.id)
                .NotEmpty()
                .WithMessage("El ID del usuario es requerido");
        }
    }
}
