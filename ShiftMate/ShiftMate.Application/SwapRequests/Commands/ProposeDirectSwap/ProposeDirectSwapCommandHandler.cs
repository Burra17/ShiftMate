using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;
using System.Globalization;

namespace ShiftMate.Application.SwapRequests.Commands.ProposeDirectSwap;

// Handlern för att föreslå ett direktbyte. Den säkerställer att användaren äger det första passet, att det andra passet är tillgängligt, skapar en bytesförfrågan och skickar en email-notis till kollegan.
public class ProposeDirectSwapCommandHandler : IRequestHandler<ProposeDirectSwapCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ProposeDirectSwapCommandHandler> _logger;

    public ProposeDirectSwapCommandHandler(
        IAppDbContext context,
        IEmailService emailService,
        ILogger<ProposeDirectSwapCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Guid> Handle(ProposeDirectSwapCommand request, CancellationToken cancellationToken)
    {
        // ------------------------------------------------------------------------------
        // 1. HÄMTA DATA
        // Vi hämtar båda passen. För TargetShift kräver vi att UserId inte är null
        // för att förhindra byten mot "lediga/vakanta" pass.
        // ------------------------------------------------------------------------------
        var myShift = await _context.Shifts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.MyShiftId, cancellationToken);

        var targetShift = await _context.Shifts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.TargetShiftId && s.UserId != null, cancellationToken);

        var requestingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.RequestingUserId, cancellationToken);

        // ------------------------------------------------------------------------------
        // 2. VALIDERING
        // ------------------------------------------------------------------------------
        if (myShift == null || targetShift == null || requestingUser == null)
            throw new NotFoundException("Kunde inte hitta passen eller målanvändaren. Passet kan sakna ägare.");

        if (myShift.UserId != request.RequestingUserId)
            throw new ForbiddenException("Du kan bara föreslå byte för pass du själv äger.");

        // Validera att båda passen tillhör samma organisation
        if (myShift.OrganizationId != request.OrganizationId || targetShift.OrganizationId != request.OrganizationId)
            throw new ForbiddenException("Passen tillhör inte din organisation.");

        // ------------------------------------------------------------------------------
        // 3. SPARA BYTESFÖRFRÅGAN (Sker först så att handlingen är säkrad i databasen)
        // ------------------------------------------------------------------------------
        var swapRequest = new SwapRequest
        {
            Id = Guid.NewGuid(),
            RequestingUserId = request.RequestingUserId,
            ShiftId = myShift.Id,
            TargetUserId = targetShift.UserId, // Kollegan som äger det andra passet
            TargetShiftId = targetShift.Id,
            Status = SwapRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SwapRequests.AddAsync(swapRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // ------------------------------------------------------------------------------
        // 4. SKICKA EMAIL-NOTIS (fire-and-forget)
        // ------------------------------------------------------------------------------
        try
        {
            var culture = new CultureInfo("sv-SE");
            var subject = $"🔄 {requestingUser.FirstName} vill byta pass med dig";
            var emailBody = Services.EmailTemplateService.SwapProposal(
                targetShift.User!.FirstName,
                $"{requestingUser.FirstName} {requestingUser.LastName}",
                targetShift.StartTime.ToString("dddd d MMMM", culture),
                $"{targetShift.StartTime:HH:mm} - {targetShift.EndTime:HH:mm}",
                myShift.StartTime.ToString("dddd d MMMM", culture),
                $"{myShift.StartTime:HH:mm} - {myShift.EndTime:HH:mm}"
            );

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(targetShift.User!.Email, subject, emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kunde inte skicka bytesförfrågan-email till {Email}", targetShift.User.Email);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email-notifiering misslyckades för bytesförfrågan {Id}", swapRequest.Id);
        }

        // Returnera ID på den skapade förfrågan
        return swapRequest.Id;
    }
}