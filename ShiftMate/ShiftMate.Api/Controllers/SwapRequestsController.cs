using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Application.SwapRequests.Queries;
using System.Security.Claims;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SwapRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SwapRequestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/SwapRequests/initiate
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateSwap(InitiateSwapCommand command)
        {
            try
            {
                // --- HÄR ÄR NYHETEN ---
                // 1. Vi hämtar vem som är inloggad från Token
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Kunde inte identifiera användaren.");
                }

                // 2. Vi stoppar in ID:t i kommandot "bakom kulisserna"
                command.RequestingUserId = Guid.Parse(userIdString);
                // -----------------------

                var swapRequestId = await _mediator.Send(command);
                return Ok(new { SwapRequestId = swapRequestId, Message = "Bytesförfrågan skapad!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SwapRequests/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSwaps()
        {
            var query = new GetAvailableSwapsQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        // POST: api/SwapRequests/accept
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptSwap(AcceptSwapCommand command)
        {
            try
            {
                await _mediator.Send(command);
                return Ok(new { Message = "Grattis! Bytet är genomfört och passet är nu ditt." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/SwapRequests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelSwapRequest(Guid id)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

                var userId = Guid.Parse(userIdString);

                await _mediator.Send(new CancelSwapRequestCommand(id, userId));

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}