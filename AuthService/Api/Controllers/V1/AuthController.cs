using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Contracts.Login;
using AuthService.Application.Features.Login;

namespace AuthService.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Autentica un usuario y devuelve un access token y refresh token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            [FromServices] LoginCommandHandler handler)
        {
            var command = new LoginCommand(request.Name, request.Password);
            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                if (string.Equals(result.Error, "INVALID_CREDENTIALS", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(result.Error);

                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Refresca el access token usando un refresh token válido
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            [FromServices] RefreshTokenCommandHandler handler)
        {
            _logger.LogInformation("Refresh token request");

            var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                // Map known errors to 401, others to 400
                var unauthorizedErrors = new[] { "REFRESH_TOKEN_NOT_FOUND", "REFRESH_TOKEN_INVALID", "ACCESS_TOKEN_INVALID", "USERID_NOT_FOUND" };
                if (result.Error != null && unauthorizedErrors.Contains(result.Error))
                    return Unauthorized(result.Error);

                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
