using FluentValidation;

namespace UserService.Api.Contracts
{
    public record CreateUserRequest(string Name, string Password);
}

