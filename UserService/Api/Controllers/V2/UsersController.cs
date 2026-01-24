using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts;
using UserService.Application.Features.GetUser;

namespace UserService.Api.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Obtiene un usuario por ID (Versión 2.0 - Con información adicional)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromServices] GetUserByIdHandler handler)
        {
            var result = await handler.Handle(new GetUserByIdQuery(id));

            if (!result.IsSuccess)
                return NotFound(new { 
                    error = result.Error,
                    message = "Usuario no encontrado",
                    timestamp = DateTime.UtcNow,
                    version = "2.0"
                });

            // V2 incluye metadata adicional
            return Ok(new
            {
                data = result.Value,
                metadata = new
                {
                    version = "2.0",
                    timestamp = DateTime.UtcNow,
                    retrievedAt = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Obtiene un usuario por nombre (Versión 2.0 - Con paginación)
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(
            string name,
            [FromServices] GetUserByNameHandler handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await handler.Handle(new GetUserByNameQuery(name));

            if (!result.IsSuccess)
                return NotFound(new { 
                    error = result.Error,
                    message = "Usuario no encontrado",
                    timestamp = DateTime.UtcNow,
                    version = "2.0"
                });

            // V2 incluye metadata de paginación
            return Ok(new
            {
                data = new[] { result.Value },
                pagination = new
                {
                    page,
                    pageSize,
                    totalItems = 1,
                    totalPages = 1
                },
                metadata = new
                {
                    version = "2.0",
                    timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Endpoint nuevo solo en V2
        /// </summary>
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string? query)
        {
            return Ok(new
            {
                message = "Este endpoint solo está disponible en V2",
                query,
                version = "2.0",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
