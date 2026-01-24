namespace UserService.Application.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        protected User() { }

        public User(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public User(string name, string passwordHash)
        {
            Id = Guid.NewGuid();
            Name = name;
            PasswordHash = passwordHash;
        }
    }
}
