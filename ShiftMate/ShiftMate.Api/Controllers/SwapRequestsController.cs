using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Application.SwapRequests.Queries;
using ShiftMate.Api.Extensions;

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
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kunde inte identifiera användaren." });
                }

                command.RequestingUserId = userId.Value;

                var swapRequestId = await _mediator.Send(command);
                return Ok(new { SwapRequestId = swapRequestId, Message = "Bytesförfrågan skapad!" });
            }
            catch (Exception ex)
            {
                // FIX: Skickar JSON-objekt så frontend kan läsa felet
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/SwapRequests/propose-direct
        [HttpPost("propose-direct")]
        public async Task<IActionResult> ProposeDirectSwap(ProposeDirectSwapCommand command)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kunde inte identifiera användaren." });
                }

                command.RequestingUserId = userId.Value;

                var swapRequestId = await _mediator.Send(command);
                return Ok(new { SwapRequestId = swapRequestId, Message = "Förslag om direktbyte har skickats!" });
            }
            catch (Exception ex)
            {
                // FIX: Skickar JSON-objekt så frontend kan läsa felet
                return BadRequest(new { message = ex.Message });
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

        // GET: api/SwapRequests/received
        [HttpGet("received")]
        public async Task<IActionResult> GetReceivedSwapRequests()
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Kunde inte identifiera användaren." });
            }

            var query = new GetReceivedSwapRequestsQuery
            {
                CurrentUserId = userId.Value
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
                var userId = User.GetUserId();
                if (userId == null) return Unauthorized(new { message = "Ingen behörighet." });

                command.CurrentUserId = userId.Value;

                await _mediator.Send(command);
                return Ok(new { Message = "Grattis! Bytet är genomfört och passet är nu ditt." });
            }
            catch (Exception ex)
            {
                // FIX: Skickar JSON-objekt så frontend kan läsa felet
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/SwapRequests/{id}/decline
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineSwapRequest(Guid id)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kunde inte identifiera användaren." });
                }

                var command = new DeclineSwapRequestCommand
                {
                    SwapRequestId = id,
                    CurrentUserId = userId.Value
                };

                await _mediator.Send(command);
                return Ok(new { Message = "Bytesförfrågan har nekats." });
            }
            catch (Exception ex)
            {
                // FIX: Skickar JSON-objekt så frontend kan läsa felet
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/SwapRequests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelSwapRequest(Guid id)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null) return Unauthorized(new { message = "Ingen behörighet." });

                await _mediator.Send(new CancelSwapRequestCommand(id, userId.Value));

                return NoContent();
            }
            catch (Exception ex)
            {
                // FIX: Skickar JSON-objekt så frontend kan läsa felet
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
