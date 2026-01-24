using System.Text;
using System.Text.Json;

namespace AuthService.Application.ExternalServices
{
    public record UserCredentialsResponse(string Message, UserInfo User);
    public record UserInfo(Guid Id, string Name);

    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;
        private readonly string _userServiceUrl;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _userServiceUrl = configuration["ExternalServices:UserService:Url"] ?? "http://localhost:8081";
        }

        public async Task<UserCredentialsResponse?> ValidateCredentialsAsync(string name, string password)
        {
            try
            {
                var request = new { name, password };
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_userServiceUrl}/api/v1/users/validate-credentials",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService validation failed with status {StatusCode}", response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<UserCredentialsResponse>(responseContent, options);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials with UserService");
                return null;
            }
        }
    }
}
