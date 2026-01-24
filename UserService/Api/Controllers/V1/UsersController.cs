using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts;
using UserService.Application.Features.CreateUser;
using UserService.Application.Features.GetUser;
using UserService.Application.Features.Queries;

namespace UserService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request,
            [FromServices] CreateUserHandler handler)
        {
            var command = new CreateUserCommand(request.Name, request.Password);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("validate-credentials")]
        public async Task<IActionResult> ValidateCredentials(
            [FromBody] ValidateCredentialsRequest request,
            [FromServices] ValidateCredentialsHandler handler)
        {
            var query = new ValidateCredentialsQuery(request.Name, request.Password);

            var result = await handler.Handle(query);

            if (!result.IsSuccess)
                return Unauthorized(new { error = result.Error });

            return Ok(new { message = "Credenciales válidas", user = result.Value });
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromServices] GetUserByIdHandler handler)
        {
            var result = await handler.Handle(new GetUserByIdQuery(id));

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(
            string name,
            [FromServices] GetUserByNameHandler handler)
        {
            var result = await handler.Handle(new GetUserByNameQuery(name));

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
    }
}
