namespace BaseService.Application.Domain
{
    public class Base
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        protected Base() { }

        public Base(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}
