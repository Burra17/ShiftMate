using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Application.SwapRequests.Queries;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                var swapRequestId = await _mediator.Send(command);
                return Ok(new { SwapRequestId = swapRequestId, Message = "Bytesförfrågan skapad!" });
            }
            catch (Exception ex)
            {
                // Om något gick fel (t.ex. fel användare), returnera 400 Bad Request
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
    }
}