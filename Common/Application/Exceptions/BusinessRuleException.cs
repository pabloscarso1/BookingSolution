namespace Common.Application.Exceptions
{
    public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string errorCode, string message)
            : base(errorCode, message)
        {
        }
    }
}
