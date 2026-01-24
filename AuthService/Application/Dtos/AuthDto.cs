namespace AuthService.Application.Dtos
{
    public class AuthDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } // en segundos
        public string TokenType { get; set; } = "Bearer";
    }
}
