using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Api.Extensions;
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

        // GET: api/organizations/admin — SuperAdmin: alla organisationer med detaljer
        [HttpGet("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllDetail()
        {
            var result = await _mediator.Send(new GetAllOrganizationsDetailQuery());
            return Ok(result);
        }

        // GET: api/organizations/my-invite-code — Manager: visa sin organisations inbjudningskod
        [HttpGet("my-invite-code")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMyInviteCode()
        {
            var orgId = User.GetOrganizationId();
            if (orgId == null) return Unauthorized();

            try
            {
                var result = await _mediator.Send(new GetOrganizationInviteCodeQuery(orgId.Value));
                return Ok(new { result.InviteCode, result.OrganizationName, result.GeneratedAt });
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

        // POST: api/organizations/{id}/regenerate-invite-code — Manager/SuperAdmin: generera ny kod
        [HttpPost("{id}/regenerate-invite-code")]
        [Authorize(Roles = "Manager,SuperAdmin")]
        public async Task<IActionResult> RegenerateInviteCode(Guid id)
        {
            try
            {
                var newCode = await _mediator.Send(new RegenerateInviteCodeCommand(id));
                return Ok(new { InviteCode = newCode, Message = "Ny inbjudningskod har genererats!" });
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
