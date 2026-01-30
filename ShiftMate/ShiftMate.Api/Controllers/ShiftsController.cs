using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.Shifts.Commands;
using System.Security.Claims; // <--- Behövs för att läsa "Claims" (IDt i token)
using ShiftMate.Application.Shifts.Queries;

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

        // POST: api/shifts
        [HttpPost]
        public async Task<IActionResult> Create(CreateShiftCommand command)
        {
            // Skicka kommandot till Application-lagret
            var shiftId = await _mediator.Send(command);

            // Returnera 200 OK med det nya ID:t
            return Ok(shiftId);
        }

        // GET: api/Shifts/mine
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyShifts()
        {
            // 1. Läs ut UserID från Token (Detta är det magiska steget!)
            // "NameIdentifier" är standardnamnet för ID i JWT-världen.
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
    }
}