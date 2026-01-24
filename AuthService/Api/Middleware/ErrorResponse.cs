namespace AuthService.Api.Middleware
{
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public Dictionary<string, object>? Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;

        public ErrorResponse()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
