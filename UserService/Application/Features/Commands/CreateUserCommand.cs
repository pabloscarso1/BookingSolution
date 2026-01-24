using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using UserService.Application.Common;
using UserService.Application.Domain;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.Features.CreateUser
{
    public record CreateUserCommand(string Name, string Password);

    public class CreateUserHandler
    {
        private readonly IUserRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateUserCommand> _validator;

        public CreateUserHandler(
            IUserRepository repository,
            IUnitOfWork uof,
            IValidator<CreateUserCommand> validator)
        {
            _repository = repository;
            _unitOfWork = uof;
            _validator = validator;
        }

        public async Task<Result<UserDto>> Handle(CreateUserCommand command)
        {
            // Validar el comando
            return await _validator.ValidateAndExecuteAsync(command, async () =>
            {
                var existing = await _repository.GetAsync(x => x.Name == command.Name);

                if (existing is not null)
                    return Result<UserDto>.Failure("USER_ALREADY_EXISTS");

                // Hashear la contraseña
                var passwordHash = HashPassword(command.Password);

                var user = new User(command.Name, passwordHash);

                await _repository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<UserDto>.Success(new UserDto(user.Id, user.Name));
            });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre es requerido")
                .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder los 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("La contraseña es requerida")
                .MinimumLength(5)
                .WithMessage("La contraseña debe tener al menos 5 caracteres");
        }
    }
}
