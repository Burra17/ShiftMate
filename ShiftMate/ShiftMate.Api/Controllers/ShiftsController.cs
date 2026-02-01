using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Application.Shifts.Queries;
using System.Security.Claims; // Behövs för att läsa "Claims" (IDt i token)

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Låset gäller nu för HELA controllern
    public class ShiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Skapar ett nytt arbetspass kopplat till den inloggade användaren.
        // POST: api/shifts
        [HttpPost]
        public async Task<IActionResult> Create(CreateShiftCommand command)
        {
            try
            {
                // 1. Hämta ID från den inloggades Token (Claim)
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Kunde inte identifiera användaren från token.");
                }

                // 2. Säkerställ att passet skapas för rätt användare
                command.UserId = Guid.Parse(userIdString);

                // 3. Skicka kommandot till Application-lagret för hantering
                var shiftId = await _mediator.Send(command);

                return Ok(new { Id = shiftId, Message = "Passet har skapats!" });
            }
            catch (Exception ex)
            {
                // Returnera ett mer informativt felmeddelande vid problem
                return BadRequest($"Kunde inte skapa passet: {ex.Message}");
            }
        }

        // Hämtar alla arbetspass som tillhör den inloggade användaren.
        // GET: api/shifts/mine
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyShifts()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kunde inte hitta ditt ID i token.");
            }

            var userId = Guid.Parse(userIdString);

            var query = new GetMyShiftsQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        // Hämtar en lista över alla arbetspass i systemet.
        // GET: api/shifts
        [HttpGet]
        public async Task<ActionResult<List<ShiftDto>>> GetAll()
        {
            var shifts = await _mediator.Send(new GetAllShiftsQuery());
            return Ok(shifts);
        }

        // Låter en inloggad användare "ta" ett ledigt arbetspass.
        // id: ID för det pass som ska tas.
        // PUT: api/shifts/{id}/take
        [HttpPut("{id}/take")]
        public async Task<IActionResult> TakeShift(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Användare inte identifierad.");
            }

            var command = new TakeShiftCommand
            {
                ShiftId = id,
                UserId = Guid.Parse(userIdString)
            };

            try
            {
                var result = await _mediator.Send(command);
                if (result)
                {
                    return Ok(new { Message = "Passet har tagits!" });
                }
                // Detta fall inträffar sällan om inte cachen är osynkad.
                return NotFound(new { Message = "Kunde inte ta passet, det kanske redan är taget." });
            }
            catch (Exception ex)
            {
                // Generisk felhantering för t.ex. om passet inte finns eller inte är ledigt
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}