using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleService.Api.Contracts;
using VehicleService.Application.Features.CreateVehicle;
using VehicleService.Application.Features.GetVehicle;

namespace VehicleService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IMapper _mapper;

        public VehicleController(IMapper mapper) => _mapper = mapper;

        [HttpPost]
        [Authorize]
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

        [Authorize]
        [HttpGet("GetAllByUserId/{userId:guid}")]
        public async Task<IActionResult> GetAllByUserId(
            Guid userId,
            [FromServices] GetAllByUserIdQueryHandler handler)
        {
            var result = await handler.Handle(new GetAllByUserIdQuery(userId));

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
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
