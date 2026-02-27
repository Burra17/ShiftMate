using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Api.Extensions;
using FluentValidation;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. SKAPA PASS
        [HttpPost]
        public async Task<IActionResult> Create(CreateShiftCommand command)
        {
            try
            {
                var userId = User.GetUserId();
                var orgId = User.GetOrganizationId();
                if (userId == null || orgId == null)
                {
                    return Unauthorized("Kunde inte identifiera användaren från token.");
                }

                command.UserId = userId.Value;
                command.OrganizationId = orgId.Value;
                var shiftId = await _mediator.Send(command);

                return Ok(new { Id = shiftId, Message = "Passet har skapats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }

        // 2. HÄMTA MINA PASS
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyShifts()
        {
            var userId = User.GetUserId();
            var orgId = User.GetOrganizationId();
            if (userId == null || orgId == null) return Unauthorized();

            var query = new GetMyShiftsQuery(userId.Value, orgId.Value);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        // 3. HÄMTA ALLA PASS
        [HttpGet]
        public async Task<ActionResult<List<ShiftDto>>> GetAll([FromQuery] bool onlyWithUsers = false)
        {
            var orgId = User.GetOrganizationId();
            if (orgId == null) return Unauthorized();

            var shifts = await _mediator.Send(new GetAllShiftsQuery(orgId.Value, onlyWithUsers));
            return Ok(shifts);
        }

        // 4. HÄMTA LEDIGA PASS (MarketPlace)
        [HttpGet("claimable")]
        public async Task<ActionResult<List<ShiftDto>>> GetClaimableShifts()
        {
            var orgId = User.GetOrganizationId();
            if (orgId == null) return Unauthorized();

            var shifts = await _mediator.Send(new GetClaimableShiftsQuery(orgId.Value));
            return Ok(shifts);
        }

        // 5. TA ETT PASS
        [HttpPut("{id}/take")]
        public async Task<IActionResult> TakeShift(Guid id)
        {
            var userId = User.GetUserId();
            var orgId = User.GetOrganizationId();
            if (userId == null || orgId == null) return Unauthorized();

            var command = new TakeShiftCommand
            {
                ShiftId = id,
                UserId = userId.Value,
                OrganizationId = orgId.Value
            };

            try
            {
                var result = await _mediator.Send(command);
                if (result) return Ok(new { Message = "Passet har tagits!" });

                return NotFound(new { Error = true, Message = "Kunde inte ta passet." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // 6. ÅNGRA MARKNADSFÖRING AV PASS
        [HttpPut("{id}/cancel-swap")]
        public async Task<IActionResult> CancelSwap(Guid id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var command = new CancelShiftSwapCommand
            {
                ShiftId = id,
                UserId = userId.Value
            };

            try
            {
                var result = await _mediator.Send(command);
                if (result) return Ok(new { Message = "Ditt pass är inte längre tillgängligt för byte." });

                return NotFound(new { Error = true, Message = "Kunde inte ångra bytet." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // 7. MANAGER UPPDATERA PASS
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateShift(Guid id, UpdateShiftCommand command)
        {
            try
            {
                var orgId = User.GetOrganizationId();
                if (orgId == null) return Unauthorized();

                command.ShiftId = id;
                command.OrganizationId = orgId.Value;
                await _mediator.Send(command);
                return Ok(new { Message = "Passet har uppdaterats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }

        // 8. MANAGER RADERA PASS
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteShift(Guid id)
        {
            try
            {
                var orgId = User.GetOrganizationId();
                if (orgId == null) return Unauthorized();

                await _mediator.Send(new DeleteShiftCommand(id, orgId.Value));
                return Ok(new { Message = "Passet har raderats!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }

        // 9. ADMIN SKAPA PASS
        [HttpPost("admin")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AdminCreate(CreateShiftCommand command)
        {
            try
            {
                var orgId = User.GetOrganizationId();
                if (orgId == null) return Unauthorized();

                command.OrganizationId = orgId.Value;
                var shiftId = await _mediator.Send(command);
                return Ok(new { Id = shiftId, Message = "Administratör: Passet har skapats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
            }
        }
    }
}
