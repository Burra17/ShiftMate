using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.SwapRequests.Commands.AcceptSwap;

// Handlern för att acceptera en bytesförfrågan. Den hanterar både direktbyten och öppna byten, inklusive alla nödvändiga krock-kontroller och notifieringar.
public class AcceptSwapCommandHandler : IRequestHandler<AcceptSwapCommand>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly Microsoft.Extensions.Logging.ILogger<AcceptSwapCommandHandler> _logger;

    public AcceptSwapCommandHandler(
        IAppDbContext context,
        IEmailService emailService,
        Microsoft.Extensions.Logging.ILogger<AcceptSwapCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(AcceptSwapCommand request, CancellationToken cancellationToken)
    {
        // Hämta bytet, inklusive ALLA relevanta pass (både det som ges och det som tas)
        var swapRequest = await _context.SwapRequests
           .Include(sr => sr.Shift)
           .Include(sr => sr.TargetShift) // <-- VIKTIGT: Ladda in målpasset
            .Include(sr => sr.RequestingUser) // <-- Ladda in användaren som frågade
            .Include(sr => sr.TargetUser) // <-- Ladda in mål-användaren (om direktbyte)
            .FirstOrDefaultAsync(sr => sr.Id == request.SwapRequestId, cancellationToken);

        if (swapRequest == null) throw new NotFoundException("Bytet hittades inte.");
        if (swapRequest.Status != SwapRequestStatus.Pending) throw new InvalidOperationException("Det här bytet är inte längre tillgängligt.");

        // Kontrollera om det är ett DIREKTBYTE eller ett ÖPPET BYTE
        bool isDirectSwap = swapRequest.TargetShiftId.HasValue && swapRequest.TargetShift != null;

        if (isDirectSwap)
        {
            // Säkerhetskoll: Endast den avsedda mottagaren (TargetUser) får acceptera ett direktbyte.
            if (swapRequest.TargetUserId != request.CurrentUserId)
            {
                throw new ForbiddenException("Du har inte behörighet att acceptera detta specifika byte.");
            }

            var originalShift = swapRequest.Shift;
            var targetShift = swapRequest.TargetShift!; // Vi vet att den inte är null här
            var requestingUserId = swapRequest.RequestingUserId;
            var targetUserId = request.CurrentUserId;

            // Krock-kontroll för BÅDA parter
            // Exkludera passet som varje person lämnar ifrån sig — det tillhör dem inte efter bytet
            if (swapRequest.RequestingUser == null) throw new InvalidOperationException("Fel: Avsändarens användardata saknas.");
            var requestorOverlap = await _context.Shifts.AnyAsync(s =>
                s.UserId == requestingUserId &&
                s.Id != targetShift.Id &&
                s.Id != originalShift.Id &&
                s.StartTime < targetShift.EndTime &&
                s.EndTime > targetShift.StartTime, cancellationToken);
            if (requestorOverlap) throw new InvalidOperationException($"Bytet kan inte genomföras eftersom {swapRequest.RequestingUser.FirstName} skulle få en passkrock.");

            // Kollar om den som accepterar krockar med passet de får
            if (swapRequest.TargetUser == null) throw new InvalidOperationException("Fel: Mottagarens användardata saknas.");
            var acceptorOverlap = await _context.Shifts.AnyAsync(s =>
                s.UserId == targetUserId &&
                s.Id != originalShift.Id &&
                s.Id != targetShift.Id &&
                s.StartTime < originalShift.EndTime &&
                s.EndTime > originalShift.StartTime, cancellationToken);
            if (acceptorOverlap) throw new InvalidOperationException("Bytet kan inte genomföras eftersom du skulle få en passkrock.");

            // Genomför bytet: Byt ägare på båda passen
            originalShift.UserId = targetUserId;
            targetShift.UserId = requestingUserId;
            originalShift.IsUpForSwap = false;
            targetShift.IsUpForSwap = false;
        }
        else
        {
            // --- LOGIK FÖR ÖPPET BYTE (som förut) ---
            var newShift = swapRequest.Shift;
            // Robusthetskoll: Om Shift ändå skulle vara null här (datainkonsekvens)
            if (newShift == null) throw new NotFoundException("Fel: Passet för bytesförfrågan kunde inte hittas.");

            // Krock-kontroll för den som tar passet
            var hasOverlap = await _context.Shifts.AnyAsync(s =>
               s.UserId == request.CurrentUserId &&
               s.StartTime < newShift.EndTime &&
               s.EndTime > newShift.StartTime,
               cancellationToken);

            if (hasOverlap)
            {
                throw new InvalidOperationException("Du har redan ett pass som krockar med detta!");
            }

            // Genomför bytet
            newShift.UserId = request.CurrentUserId;
            newShift.IsUpForSwap = false;
        }

        // Avsluta förfrågan
        swapRequest.Status = SwapRequestStatus.Accepted;

        // Spara alla ändringar
        await _context.SaveChangesAsync(cancellationToken);

        // Skicka email till den som föreslog bytet (fire-and-forget)
        try
        {
            if (swapRequest.RequestingUser != null)
            {
                var culture = new System.Globalization.CultureInfo("sv-SE");
                string emailBody;
                string subject;

                if (isDirectSwap && swapRequest.TargetUser != null)
                {
                    subject = $"🎉 {swapRequest.TargetUser.FirstName} godkände bytet!";
                    emailBody = Services.EmailTemplateService.DirectSwapAccepted(
                        swapRequest.RequestingUser.FirstName,
                        $"{swapRequest.TargetUser.FirstName} {swapRequest.TargetUser.LastName}",
                        swapRequest.Shift.StartTime.ToString("dddd d MMMM", culture),
                        $"{swapRequest.Shift.StartTime:HH:mm} - {swapRequest.Shift.EndTime:HH:mm}",
                        swapRequest.TargetShift!.StartTime.ToString("dddd d MMMM", culture),
                        $"{swapRequest.TargetShift.StartTime:HH:mm} - {swapRequest.TargetShift.EndTime:HH:mm}"
                    );
                }
                else
                {
                    var takerName = swapRequest.TargetUser?.FirstName ?? "Någon";
                    var takerFull = swapRequest.TargetUser != null
                        ? $"{swapRequest.TargetUser.FirstName} {swapRequest.TargetUser.LastName}"
                        : "Någon";

                    subject = $"✅ {takerName} tog ditt pass!";
                    emailBody = Services.EmailTemplateService.MarketplaceShiftTaken(
                        swapRequest.RequestingUser.FirstName,
                        takerFull,
                        swapRequest.Shift.StartTime.ToString("dddd d MMMM", culture),
                        $"{swapRequest.Shift.StartTime:HH:mm} - {swapRequest.Shift.EndTime:HH:mm}"
                    );
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(swapRequest.RequestingUser.Email, subject, emailBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kunde inte skicka godkännande-email till {Email}", swapRequest.RequestingUser.Email);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email-notifiering misslyckades för godkänt byte {Id}", swapRequest.Id);
        }
    }
}
