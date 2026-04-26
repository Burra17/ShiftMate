using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Api.Extensions;
using ShiftMate.Application.SwapRequests.Commands.AcceptSwap;
using ShiftMate.Application.SwapRequests.Commands.CancelSwapRequest;
using ShiftMate.Application.SwapRequests.Commands.DeclineSwapRequest;
using ShiftMate.Application.SwapRequests.Commands.InitiateSwap;
using ShiftMate.Application.SwapRequests.Commands.ProposeDirectSwap;
using ShiftMate.Application.SwapRequests.Queries.GetAvailableSwaps;
using ShiftMate.Application.SwapRequests.Queries.GetReceivedSwapRequests;
using ShiftMate.Application.SwapRequests.Queries.GetSentSwapRequests;

// CONTROLLER FÖR BYTESFÖRFRÅGNINGAR
namespace ShiftMate.Api.Controllers;

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
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        command.RequestingUserId = userId.Value;

        var swapRequestId = await _mediator.Send(command);
        return Ok(new { SwapRequestId = swapRequestId, Message = "Bytesförfrågan skapad!" });
    }

    // POST: api/SwapRequests/propose-direct
    [HttpPost("propose-direct")]
    public async Task<IActionResult> ProposeDirectSwap(ProposeDirectSwapCommand command)
    {
        var userId = User.GetUserId();
        var orgId = User.GetOrganizationId();
        if (userId == null || orgId == null) return Unauthorized();

        command.RequestingUserId = userId.Value;
        command.OrganizationId = orgId.Value;

        var swapRequestId = await _mediator.Send(command);
        return Ok(new { SwapRequestId = swapRequestId, Message = "Förslag om direktbyte har skickats!" });
    }

    // GET: api/SwapRequests/available
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSwaps()
    {
        var orgId = User.GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var result = await _mediator.Send(new GetAvailableSwapsQuery(orgId.Value));
        return Ok(result);
    }

    // GET: api/SwapRequests/received
    [HttpGet("received")]
    public async Task<IActionResult> GetReceivedSwapRequests()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetReceivedSwapRequestsQuery { CurrentUserId = userId.Value });
        return Ok(result);
    }

    // GET: api/SwapRequests/sent
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentSwapRequests()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetSentSwapRequestsQuery { CurrentUserId = userId.Value });
        return Ok(result);
    }

    // POST: api/SwapRequests/accept
    [HttpPost("accept")]
    public async Task<IActionResult> AcceptSwap(AcceptSwapCommand command)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        command.CurrentUserId = userId.Value;

        await _mediator.Send(command);
        return Ok(new { Message = "Grattis! Bytet är genomfört och passet är nu ditt." });
    }

    // POST: api/SwapRequests/{id}/decline
    [HttpPost("{id}/decline")]
    public async Task<IActionResult> DeclineSwapRequest(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        await _mediator.Send(new DeclineSwapRequestCommand
        {
            SwapRequestId = id,
            CurrentUserId = userId.Value
        });
        return Ok(new { Message = "Bytesförfrågan har nekats." });
    }

    // DELETE: api/SwapRequests/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelSwapRequest(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        await _mediator.Send(new CancelSwapRequestCommand(id, userId.Value));
        return NoContent();
    }
}
