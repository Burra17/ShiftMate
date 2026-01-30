using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.Users.Queries; // Se till att denna matchar din namespace

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Här injicerar vi MediatR
        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Skicka frågan "Ge mig alla användare" till Application-lagret
            var query = new GetAllUsersQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}