using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Organizations;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.Demo.Commands.StartDemo;

// Skapar en egen sandlåda (organisation med data) för en besökare och loggar in den som vald roll.
// Städar först bort utgångna sandlådor – då behövs inget bakgrundsjobb.
public class StartDemoCommandHandler : IRequestHandler<StartDemoCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IValidator<StartDemoCommand> _validator;

    public StartDemoCommandHandler(
        IAppDbContext context,
        IJwtTokenGenerator tokenGenerator,
        IValidator<StartDemoCommand> validator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _validator = validator;
    }

    public async Task<string> Handle(StartDemoCommand request, CancellationToken cancellationToken)
    {
        // 1. Validering
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var now = DateTime.UtcNow;

        // 2. Städa utgångna sandlådor – bara IsDemo, aldrig riktiga organisationer
        var expiredBefore = now - DemoConstants.SandboxLifetime;
        var expiredIds = await _context.Organizations
            .Where(o => o.IsDemo && o.CreatedAt < expiredBefore)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);
        if (expiredIds.Count > 0)
        {
            await OrganizationRemover.RemoveWithAllDataAsync(_context, expiredIds, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Tak för antal samtidiga sandlådor
        var activeCount = await _context.Organizations.CountAsync(o => o.IsDemo, cancellationToken);
        if (activeCount >= DemoConstants.MaxActiveSandboxes)
        {
            throw new TooManyRequestsException("Demon är full just nu – försök igen om en stund.");
        }

        // 4. Bygg och spara sandlådan. Lösenordet är slumpat och okänt – demo-användare loggar bara in via demon.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
        var visitorRole = request.Role == "Manager" ? Role.Manager : Role.Employee;
        var sandbox = DemoSandboxBuilder.Build(visitorRole, passwordHash, now);

        _context.Organizations.Add(sandbox.Organization);
        _context.Users.AddRange(sandbox.Users);
        _context.Shifts.AddRange(sandbox.Shifts);
        _context.SwapRequests.AddRange(sandbox.SwapRequests);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Token för besökaren (Organization är satt, så IsDemo-claimet kommer med)
        return _tokenGenerator.GenerateToken(sandbox.Visitor);
    }
}
