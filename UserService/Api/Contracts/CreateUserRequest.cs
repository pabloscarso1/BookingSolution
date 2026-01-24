using FluentValidation;

namespace UserService.Api.Contracts
{
    public record CreateUserRequest(string Name);

    //public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    //{
    //    public CreateUserRequestValidator()
    //    {
    //        RuleFor(x => x.Name)
    //            .NotEmpty()
    //            .WithMessage("El nombre es requerido")
    //            .MinimumLength(3)
    //            .WithMessage("El nombre debe tener al menos 3 caracteres")
    //            .MaximumLength(100)
    //            .WithMessage("El nombre no puede exceder los 100 caracteres");
    //    }
    //}
}

