using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.SwapRequests.Commands;

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
    }
}