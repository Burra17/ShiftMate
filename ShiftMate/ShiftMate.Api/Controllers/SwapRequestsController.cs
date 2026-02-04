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
                // Hämta vem som är inloggad från Token
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Kunde inte identifiera användaren.");
                }

                // Stoppa in ID:t i kommandot "bakom kulisserna"
                command.RequestingUserId = Guid.Parse(userIdString);

                var swapRequestId = await _mediator.Send(command);
                return Ok(new { SwapRequestId = swapRequestId, Message = "Bytesförfrågan skapad!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SwapRequests/propose-direct
        [HttpPost("propose-direct")]
        public async Task<IActionResult> ProposeDirectSwap(ProposeDirectSwapCommand command)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Kunde inte identifiera användaren.");
                }

                command.RequestingUserId = Guid.Parse(userIdString);

                var swapRequestId = await _mediator.Send(command);
                return Ok(new { SwapRequestId = swapRequestId, Message = "Förslag om direktbyte har skickats!" });
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

        // NYTT: Hämta förfrågningar som skickats TILL mig
        // GET: api/SwapRequests/received
        [HttpGet("received")]
        public async Task<IActionResult> GetReceivedSwapRequests()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kunde inte identifiera användaren.");
            }

            var query = new GetReceivedSwapRequestsQuery
            {
                CurrentUserId = Guid.Parse(userIdString)
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // POST: api/SwapRequests/accept
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptSwap(AcceptSwapCommand command)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

                command.CurrentUserId = Guid.Parse(userIdString);

                await _mediator.Send(command);
                return Ok(new { Message = "Grattis! Bytet är genomfört och passet är nu ditt." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // NYTT: Neka en bytesförfrågan
        // POST: api/SwapRequests/{id}/decline
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineSwapRequest(Guid id)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Kunde inte identifiera användaren.");
                }

                var command = new DeclineSwapRequestCommand
                {
                    SwapRequestId = id,
                    CurrentUserId = Guid.Parse(userIdString)
                };

                await _mediator.Send(command);
                return Ok(new { Message = "Bytesförfrågan har nekats." });
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