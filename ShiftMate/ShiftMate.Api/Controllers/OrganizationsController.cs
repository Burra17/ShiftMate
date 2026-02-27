using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.Organizations.Queries;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/organizations — Publik endpoint för registreringssidan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllOrganizationsQuery());
            return Ok(result);
        }
    }
}
