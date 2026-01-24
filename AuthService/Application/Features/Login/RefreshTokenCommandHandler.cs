using AuthService.Application.Common;
using AuthService.Application.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;

namespace AuthService.Application.Features.Login
{
    public class RefreshTokenCommandHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtService _jwtService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            JwtService jwtService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<Result<AuthDto>> Handle(RefreshTokenCommand command)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

            if (refreshToken == null)
                return Result<AuthDto>.Failure("REFRESH_TOKEN_NOT_FOUND");

            if (!refreshToken.IsValid())
                return Result<AuthDto>.Failure("REFRESH_TOKEN_INVALID");

            var principal = _jwtService.GetClaimsPrincipalFromExpiredToken(command.AccessToken);
            if (principal == null)
                return Result<AuthDto>.Failure("ACCESS_TOKEN_INVALID");

            var userIdClaim = principal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Result<AuthDto>.Failure("USERID_NOT_FOUND");

            var newAccessToken = _jwtService.GenerateAccessToken(userId);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            refreshToken.Token = newRefreshToken;
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            _logger.LogInformation("Tokens refresheados exitosamente para usuario {UserId}", userId);

            var auth = new AuthDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 900,
                TokenType = "Bearer"
            };

            return Result<AuthDto>.Success(auth);
        }
    }
}
