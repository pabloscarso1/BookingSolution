using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BookingController : ControllerBase
    {
        //[HttpGet("{id:guid}")]
        //public async Task<IActionResult> GetById(
        //    Guid id,
        //    [FromServices] GetBaseByIdQueryHandler handler)
        //{
        //    var result = await handler.Handle(new GetBaseByIdQuery(id));

        //    if (!result.IsSuccess)
        //        return NotFound(result.Error);

        //    return Ok(result.Value);
        //}
    }
}
