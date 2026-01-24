namespace UserService.Application.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        protected User() { }

        public User(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}
