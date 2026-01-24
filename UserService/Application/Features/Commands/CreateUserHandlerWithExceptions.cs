using FluentValidation;
using UserService.Application.Common;
using UserService.Application.Domain;
using UserService.Application.Dtos;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Application.Features.CreateUser
{
    /// <summary>
    /// Versión alternativa del handler que usa excepciones personalizadas
    /// en lugar de Result pattern
    /// </summary>
    public class CreateUserHandlerWithExceptions
    {
        private readonly IUserRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateUserCommand> _validator;

        public CreateUserHandlerWithExceptions(
            IUserRepository repository,
            IUnitOfWork uof,
            IValidator<CreateUserCommand> validator)
        {
            _repository = repository;
            _unitOfWork = uof;
            _validator = validator;
        }

        public async Task<UserDto> Handle(CreateUserCommand command)
        {
            // Validar el comando - lanzará ValidationException si falla
            await _validator.ValidateAndThrowAsync(command);

            // Verificar si el usuario ya existe
            var existing = await _repository.GetAsync(x => x.Name == command.Name);

            if (existing is not null)
            {
                throw new ConflictException("USER_ALREADY_EXISTS", 
                    $"Ya existe un usuario con el nombre '{command.Name}'");
            }

            // Crear el usuario
            var user = new User(command.Name);

            await _repository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new UserDto(user.Id, user.Name);
        }
    }
}
