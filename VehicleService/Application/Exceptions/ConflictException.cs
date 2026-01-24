namespace VehicleService.Application.Exceptions
{
    public class ConflictException : DomainException
    {
        public ConflictException(string message)
            : base("CONFLICT", message)
        {
        }

        public ConflictException(string errorCode, string message)
            : base(errorCode, message)
        {
        }
    }
}
