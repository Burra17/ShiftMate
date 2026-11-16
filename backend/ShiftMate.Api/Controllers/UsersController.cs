using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftMate.Api.Extensions;
using ShiftMate.Application.Users.Commands.ChangePassword;
using ShiftMate.Application.Users.Commands.DeleteUser;
using ShiftMate.Application.Users.Commands.ForgotPassword;
using ShiftMate.Application.Users.Commands.Login;
using ShiftMate.Application.Users.Commands.RegisterUser;
using ShiftMate.Application.Users.Commands.ResendVerification;
using ShiftMate.Application.Users.Commands.ResetPassword;
using ShiftMate.Application.Users.Commands.UpdateProfile;
using ShiftMate.Application.Users.Commands.UpdateUserRole;
using ShiftMate.Application.Users.Commands.VerifyEmail;
using ShiftMate.Application.Users.Queries.GetAllUsers;

// CONTROLLER FÖR ANVÄNDARE
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

            var result = await _mediator.Send(new GetAllUsersQuery(orgId.Value, page, pageSize));
            return Ok(result);
        }

        // POST: api/users/forgot-password
        // Anti-enumeration: sväljer alla fel så svaret är samma oavsett om e-posten finns.
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
        {
            try { await _mediator.Send(command); }
            catch { /* tyst — anti-enumeration */ }

            return Ok(new { Message = "Om e-postadressen finns i systemet har vi skickat en återställningslänk." });
        }

        // POST: api/users/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Lösenordet har återställts! Du kan nu logga in." });
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var token = await _mediator.Send(command);
            return Ok(new { Token = token });
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // POST: api/users/verify-email
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { Message = "E-postadressen har verifierats! Du kan nu logga in." });
        }

        // POST: api/users/resend-verification
        // Anti-enumeration: sväljer alla fel så svaret är samma oavsett om kontot finns.
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationCommand command)
        {
            try { await _mediator.Send(command); }
            catch { /* tyst — anti-enumeration */ }

            return Ok(new { Message = "Om kontot finns och inte är verifierat har vi skickat ett nytt verifieringsmail." });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            command.UserId = userId.Value;
            await _mediator.Send(command);
            return Ok(new { Message = "Profilen har uppdaterats!" });
        }

        // PUT: api/Users/change-password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            command.UserId = userId.Value;
            await _mediator.Send(command);
            return Ok(new { Message = "Lösenordet har ändrats!" });
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var requestingUserId = User.GetUserId();
            var orgId = User.GetOrganizationId();
            if (requestingUserId == null || orgId == null) return Unauthorized();

            await _mediator.Send(new DeleteUserCommand
            {
                TargetUserId = id,
                RequestingUserId = requestingUserId.Value,
                OrganizationId = orgId.Value
            });
            return Ok(new { Message = "Användaren har inaktiverats." });
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

            await _mediator.Send(command);
            return Ok(new { Message = "Rollen har uppdaterats." });
        }
    }
}
