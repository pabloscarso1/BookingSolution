using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Common.Application.Common;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.Features.Queries
{
    public record ValidateCredentialsQuery(string Name, string Password);

    public class ValidateCredentialsHandler
    {
        private readonly IUserRepository _repository;
        private readonly IValidator<ValidateCredentialsQuery> _validator;

        public ValidateCredentialsHandler(
            IUserRepository repository,
            IValidator<ValidateCredentialsQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<UserDto>> Handle(ValidateCredentialsQuery query)
        {
            // Validar el query
            return await _validator.ValidateAndExecuteAsync(query, async () =>
            {
                var user = await _repository.GetAsync(x => x.Name == query.Name);

                if (user is null)
                    return Result<UserDto>.Failure("INVALID_CREDENTIALS");

                // Validar que la contraseña coincida
                var passwordHash = HashPassword(query.Password);
                if (user.PasswordHash != passwordHash)
                    return Result<UserDto>.Failure("INVALID_CREDENTIALS");

                var dto = new UserDto(user.Id, user.Name);
                return Result<UserDto>.Success(dto);
            });
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public class ValidateCredentialsValidator : AbstractValidator<ValidateCredentialsQuery>
    {
        public ValidateCredentialsValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre es requerido");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("La contraseña es requerida");
        }
    }
}
