using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetClaimsPrincipalFromExpiredToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateAccessToken(Guid userId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurado")));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15")),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetClaimsPrincipalFromExpiredToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "")),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Token validation failed: invalid security token or algorithm");
                    return null;
                }

                // Asegurar que los claims del JWT se incluyan en el principal
                if (jwtSecurityToken.Claims != null && jwtSecurityToken.Claims.Any())
                {
                    var claimsIdentity = principal.Identity as ClaimsIdentity;
                    if (claimsIdentity != null)
                    {
                        // Agregar claims del token al principal si no están presentes
                        foreach (var claim in jwtSecurityToken.Claims)
                        {
                            if (!claimsIdentity.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                            {
                                claimsIdentity.AddClaim(claim);
                            }
                        }
                    }
                }

                // Log del claim 'sub' para debugging
                var subClaim = principal?.FindFirst("sub")?.Value;
                _logger.LogInformation("Token claims extracted. SubClaim: {SubClaim}, Total Claims: {ClaimsCount}", 
                    subClaim ?? "NOT FOUND", principal?.Claims.Count() ?? 0);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al validar token expirado");
                return null;
            }
        }
    }
}
