using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Application.Shifts.Queries;
using System.Security.Claims; // <--- Behövs för att läsa "Claims" (IDt i token)

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- Låset gäller nu för HELA controllern
    public class ShiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/shifts
        [HttpPost]
        public async Task<IActionResult> Create(CreateShiftCommand command)
        {
            try
            {
                // 1. Hämta ID från den inloggades Token
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Kunde inte identifiera användaren.");
                }

                // 2. Tvinga in ID:t i kommandot automatiskt
                // Användaren skickade bara tid, vi fyller i "vem".
                command.UserId = Guid.Parse(userIdString);

                // 3. Skicka vidare till Application-lagret
                var shiftId = await _mediator.Send(command);

                // Returnera snyggt meddelande
                return Ok(new { Id = shiftId, Message = "Passet skapat åt dig!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Shifts/mine
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyShifts()
        {
            // 1. Läs ut UserID från Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kunde inte hitta ditt ID i token.");
            }

            var userId = Guid.Parse(userIdString);

            // 2. Skicka frågan med rätt ID
            var query = new GetMyShiftsQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        // Lägg in denna i ShiftsController-klassen

        [HttpGet] // Svarar på GET /api/Shifts
        public async Task<ActionResult<List<ShiftDto>>> GetAll()
        {
            var shifts = await _mediator.Send(new GetAllShiftsQuery());
            return Ok(shifts);
        }
    }
}