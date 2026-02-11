using MediatR;
﻿using Microsoft.AspNetCore.Mvc;
﻿using ShiftMate.Application.Users.Commands;
﻿using ShiftMate.Application.Users.Queries;
﻿using ShiftMate.Api.Extensions;
﻿
﻿namespace ShiftMate.Api.Controllers
﻿{
﻿    [Route("api/[controller]")]
﻿    [ApiController]
﻿    public class UsersController : ControllerBase
﻿    {
﻿        private readonly IMediator _mediator;
﻿
﻿        // Här injicerar vi MediatR
﻿        public UsersController(IMediator mediator)
﻿        {
﻿            _mediator = mediator;
﻿        }
﻿
﻿        // GET: api/users
﻿        [HttpGet]
﻿        public async Task<IActionResult> GetAll()
﻿        {
﻿            // Skicka frågan "Ge mig alla användare" till Application-lagret
﻿            var query = new GetAllUsersQuery();
﻿            var result = await _mediator.Send(query);
﻿
﻿            return Ok(result);
﻿        }
﻿
﻿        // POST: api/Users/login
﻿        [HttpPost("login")]
﻿        public async Task<IActionResult> Login(LoginCommand command)
﻿        {
﻿            try
﻿            {
﻿                var token = await _mediator.Send(command);
﻿                // Vi returnerar token i ett JSON-objekt
﻿                return Ok(new { Token = token });
﻿            }
﻿            catch (Exception ex)
﻿            {
﻿                return Unauthorized(ex.Message); // Returnera 401 om inloggningen misslyckas
﻿            }
﻿        }
﻿
﻿        // POST: api/users/register
﻿        [HttpPost("register")]
﻿        public async Task<IActionResult> Register(RegisterUserCommand command)
﻿        {
﻿            try
﻿            {
﻿                var result = await _mediator.Send(command);
﻿                return Ok(result);
﻿            }
﻿            catch (Exception ex)
﻿            {
﻿                // Om handlern kastar ett undantag (t.ex. användare finns redan),
﻿                // returnera 400 Bad Request med felmeddelandet.
﻿                return BadRequest(ex.Message);
﻿            }
﻿        }
﻿
﻿        [HttpPut("profile")]
﻿        public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command)
﻿        {
﻿            var userId = User.GetUserId();
﻿            if (userId == null) return Unauthorized();

﻿            command.UserId = userId.Value;
﻿
﻿            try
﻿            {
﻿                await _mediator.Send(command);
﻿                return Ok(new { Message = "Profilen har uppdaterats!" });
﻿            }
﻿            catch (Exception ex)
﻿            {
﻿                return BadRequest(ex.Message);
﻿            }
﻿        }
﻿    }
﻿}
﻿