using FluentValidation;

namespace BookingService.Api.Contracts
{
    public record CreateBaseRequest(string Name);

    public class CreateBaseRequestValidator : AbstractValidator<CreateBaseRequest>
    {
        public CreateBaseRequestValidator()
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

