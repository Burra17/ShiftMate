using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Users.Commands
{
    public record UpdateProfileCommand : IRequest
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand>
    {
        private readonly IAppDbContext _context;

        public UpdateProfileHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null) throw new Exception("Användaren hittades inte.");

            // Uppdatera fälten
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}