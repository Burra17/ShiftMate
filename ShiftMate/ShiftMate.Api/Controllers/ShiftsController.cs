using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.Shifts.Commands;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}