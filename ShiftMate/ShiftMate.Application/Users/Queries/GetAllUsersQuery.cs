using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces; // <--- Använd interfacet
using ShiftMate.Domain;

namespace ShiftMate.Application.Users.Queries
{
    public record GetAllUsersQuery : IRequest<List<User>>;

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<User>>
    {
        private readonly IAppDbContext _context; // <--- Inte AppDbContext, utan IAppDbContext

        // Dependency Injection skickar in den riktiga databasen, men vi ser den bara som ett interface
        public GetAllUsersHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Users.ToListAsync(cancellationToken);
        }
    }
}