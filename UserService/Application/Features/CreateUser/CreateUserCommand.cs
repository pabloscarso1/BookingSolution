using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using UserService.Application.Common;
using UserService.Application.Domain;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.Features.CreateUser
{
    public record CreateUserCommand(string Name);

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
                var existing = _repository.GetAsync(x => x.Name == command.Name);

                if (existing is not null)
                    return Result<UserDto>.Failure("USER_ALREADY_EXISTS");

                var user = new User(command.Name);

                await _repository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<UserDto>.Success(new UserDto(user.Id, user.Name));
            });
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
        }
    }
}
