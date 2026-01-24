using FluentValidation;

namespace AuthService.Application.Features.Login
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken);

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("El access token es requerido");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("El refresh token es requerido");
        }
    }
}
