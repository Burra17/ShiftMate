using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Application.Users.Queries;
using ShiftMate.Api.Extensions;

namespace ShiftMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/users (med valfri paginering)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
        {
            var orgId = User.GetOrganizationId();
            if (orgId == null) return Unauthorized();

            var query = new GetAllUsersQuery(orgId.Value, page, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        // POST: api/users/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
        {
            try
            {
                await _mediator.Send(command);
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch
            {
                // Returnera alltid samma svar oavsett om e-posten finns eller inte (anti-enumeration)
            }

            return Ok(new { Message = "Om e-postadressen finns i systemet har vi skickat en återställningslänk." });
        }

        // POST: api/users/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            try
            {
                await _mediator.Send(command);
                return Ok(new { Message = "Lösenordet har återställts! Du kan nu logga in." });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            try
            {
                var token = await _mediator.Send(command);
                return Ok(new { Token = token });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message, Code = "EMAIL_NOT_VERIFIED" });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // POST: api/users/verify-email
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command)
        {
            try
            {
                await _mediator.Send(command);
                return Ok(new { Message = "E-postadressen har verifierats! Du kan nu logga in." });
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // POST: api/users/resend-verification
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationCommand command)
        {
            try
            {
                await _mediator.Send(command);
            }
            catch
            {
                // Anti-enumeration: returnera alltid samma svar
            }

            return Ok(new { Message = "Om kontot finns och inte är verifierat har vi skickat ett nytt verifieringsmail." });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            command.UserId = userId.Value;

            try
            {
                await _mediator.Send(command);
                return Ok(new { Message = "Profilen har uppdaterats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // PUT: api/Users/change-password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            command.UserId = userId.Value;

            try
            {
                await _mediator.Send(command);
                return Ok(new { Message = "Lösenordet har ändrats!" });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var requestingUserId = User.GetUserId();
            var orgId = User.GetOrganizationId();
            if (requestingUserId == null || orgId == null) return Unauthorized();

            try
            {
                await _mediator.Send(new DeleteUserCommand
                {
                    TargetUserId = id,
                    RequestingUserId = requestingUserId.Value,
                    OrganizationId = orgId.Value
                });
                return Ok(new { Message = "Användaren har tagits bort." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }

        // PUT: api/users/{id}/role
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleCommand command)
        {
            var requestingUserId = User.GetUserId();
            var orgId = User.GetOrganizationId();
            if (requestingUserId == null || orgId == null) return Unauthorized();

            command = command with
            {
                TargetUserId = id,
                RequestingUserId = requestingUserId.Value,
                OrganizationId = orgId.Value
            };

            try
            {
                await _mediator.Send(command);
                return Ok(new { Message = "Rollen har uppdaterats." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = true, Message = ex.Message });
            }
        }
    }
}
