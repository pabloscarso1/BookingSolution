using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts;
using UserService.Application.Features.GetUser;

namespace UserService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        //[HttpPost]
        //public async Task<IActionResult> Create(
        //    [FromBody] CreateUserRequest request,
        //    [FromServices] CreateUserHandler handler)
        //{
        //    var command = new CreateUserCommand(request.Name);

        //    var result = await handler.Handle(command);

        //    if (!result.IsSuccess)
        //        return BadRequest(result.Error);

        //    return Ok(result.Value?.Id);
        //}

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
