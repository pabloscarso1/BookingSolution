using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingService.Api.Contracts;
using BookingService.Application.Features.Commands;

namespace BookingService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IMapper _mapper;

        public BookingController(IMapper mapper) => _mapper = mapper;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateBookingRequest request,
            [FromServices] CreateBookingHandler handler)
        {
            var command = _mapper.Map<CreateBookingCommand>(request);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value?.Id);
        }

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
