using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace UserService.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        [HttpGet("token-info")]
        public IActionResult GetTokenInfo()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            _logger.LogInformation("Authorization header: {AuthHeader}", authHeader);

            if (string.IsNullOrEmpty(authHeader))
            {
                return BadRequest(new { error = "No Authorization header provided" });
            }

            if (!authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new { error = "Authorization header must start with 'Bearer '" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return BadRequest(new { error = "Invalid token format" });
                }

                return Ok(new
                {
                    issuer = jsonToken.Issuer,
                    audience = string.Join(",", jsonToken.Audiences),
                    expires = jsonToken.ValidTo,
                    claims = jsonToken.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading token");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
