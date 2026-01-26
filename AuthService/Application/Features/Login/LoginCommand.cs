using FluentValidation;
using AuthService.Application.Common;
using AuthService.Application.Dtos;
using AuthService.Application.ExternalServices;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Application.Domain;

namespace AuthService.Application.Features.Login
{
    public record LoginCommand(string Name, string Password);

    public class LoginCommandHandler
    {
        private readonly UserServiceClient _userServiceClient;
        private readonly JwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IValidator<LoginCommand> _validator;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IConfiguration _configuration;

        public LoginCommandHandler(
            UserServiceClient userServiceClient,
            JwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            IValidator<LoginCommand> validator,
            ILogger<LoginCommandHandler> logger,
            IConfiguration configuration)
        {
            _userServiceClient = userServiceClient;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _validator = validator;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Result<AuthDto>> Handle(LoginCommand command)
        {
            return await _validator.ValidateAndExecuteAsync<AuthDto, LoginCommand>(command, async () =>
            {
                // Validar credenciales con UserService
                var userValidation = await _userServiceClient.ValidateCredentialsAsync(command.Name, command.Password);
                
                if (userValidation?.User == null)
                {
                    _logger.LogWarning("Failed login attempt for user {UserName}", command.Name);
                    return Result<AuthDto>.Failure("INVALID_CREDENTIALS");
                }

                var userId = userValidation.User.Id;

                // Generar tokens
                var accessToken = _jwtService.GenerateAccessToken(userId);
                var refreshTokenValue = _jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddDays(7); // Refresh token válido por 7 días

                // Guardar refresh token
                var refreshToken = new RefreshToken()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = refreshTokenValue,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };
                
                await _refreshTokenRepository.AddAsync(refreshToken);

                _logger.LogInformation("User {UserName} logged in successfully", command.Name);
                
                // Calcular ExpiresIn en segundos basado en la configuración
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
                var expiresIn = expirationMinutes * 60; // convertir a segundos
                
                var auth = new AuthDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenValue,
                    ExpiresIn = expiresIn,
                    TokenType = "Bearer",
                    UserId = userId
                };

                return Result<AuthDto>.Success(auth);
            });
        }
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");
        }
    }
}
