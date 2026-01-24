using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using BaseService.Api.Contracts;
using BaseService.Application.Features.GetUser;

namespace BaseService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromServices] GetBaseByIdQueryHandler handler)
        {
            var result = await handler.Handle(new GetBaseByIdQuery(id));

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
    }
}
