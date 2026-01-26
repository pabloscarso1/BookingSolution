using FluentValidation;
using UserService.Application.Common;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.Features.GetUser
{
    public record GetUserByNameQuery(string Name);

    public class GetUserByNameHandler
    {
        private readonly IUserRepository _repository;
        private readonly IValidator<GetUserByNameQuery> _validator;

        public GetUserByNameHandler(
            IUserRepository repository,
            IValidator<GetUserByNameQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<UserDto>> Handle(GetUserByNameQuery query)
        {
            // Validar el query
            return await _validator.ValidateAndExecuteAsync(query, async () =>
            {
                var user = await _repository.GetAsync(x => x.Name == query.Name);

                if (user is null)
                    return Result<UserDto>.Failure("USER_NOT_FOUND");

                var dto = new UserDto(user.Id, user.Name);

                return Result<UserDto>.Success(dto);
            });
        }
    }

    public class GetUserByNameQueryValidator : AbstractValidator<GetUserByNameQuery>
    {
        public GetUserByNameQueryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre del usuario es requerido")
                .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres");
        }
    }
}
