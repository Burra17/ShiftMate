using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.Organizations.Commands;
using ShiftMate.Application.Organizations.Queries;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/organizations — Publik endpoint för registreringssidan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllOrganizationsQuery());
            return Ok(result);
        }

        // GET: api/organizations/admin — SuperAdmin: alla organisationer med detaljer
        [HttpGet("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllDetail()
        {
            var result = await _mediator.Send(new GetAllOrganizationsDetailQuery());
            return Ok(result);
        }

        // POST: api/organizations — SuperAdmin: skapa ny organisation
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return Ok(new { Id = id, Message = "Organisationen har skapats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }

        // PUT: api/organizations/{id} — SuperAdmin: uppdatera organisation
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateOrganizationCommand(id, request.Name));
                return Ok(new { Message = "Organisationen har uppdaterats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }

        // DELETE: api/organizations/{id} — SuperAdmin: radera organisation
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteOrganizationCommand(id));
                return Ok(new { Message = "Organisationen har raderats!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }
    }

    // Request-klass för PUT (undviker Id-konflikt mellan route och body)
    public class UpdateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
