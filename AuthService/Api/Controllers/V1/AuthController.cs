using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Contracts;
using AuthService.Application.Features.CreateVehicle;
using AuthService.Application.Features.GetVehicle;

namespace AuthService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;

        public AuthController(IMapper mapper) => _mapper = mapper;

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateVehicleRequest request,
            [FromServices] CreateVehicleHandler handler)
        {
            var command = _mapper.Map<CreateVehicleCommand>(request);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value?.Id);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromServices] GetVehicleByIdQueryHandler handler)
        {
            var result = await handler.Handle(new GetVehicleByIdQuery(id));

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
    }
}
