using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShiftMate.Application.Demo.Commands.StartDemo;

// CONTROLLER FÖR DEMO-SANDLÅDAN (besökare från portfolion – ingen inloggning krävs)
namespace ShiftMate.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DemoController : ControllerBase
{
    // Namnet på rate limit-policyn som registreras i Program.cs
    public const string RateLimitPolicy = "demo";

    private readonly IMediator _mediator;

    public DemoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/demo/start — skapar en egen sandlåda och returnerar token för vald roll
    [HttpPost("start")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    public async Task<IActionResult> Start(StartDemoCommand command)
    {
        var token = await _mediator.Send(command);
        return Ok(new { Token = token });
    }
}
