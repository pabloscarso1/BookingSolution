namespace BaseService.Application.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key)
            : base("NOT_FOUND", $"{entityName} con identificador '{key}' no fue encontrado")
        {
        }

        public NotFoundException(string message)
            : base("NOT_FOUND", message)
        {
        }
    }
}
