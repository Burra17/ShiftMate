using MediatR;
﻿using ShiftMate.Application.Interfaces;
﻿using Microsoft.EntityFrameworkCore;
﻿using System.Text.Json.Serialization; // <--- Behövs för [JsonIgnore]
using Microsoft.Extensions.Logging;
﻿
﻿namespace ShiftMate.Application.SwapRequests.Commands
﻿{
﻿    // Svensk kommentar: Kommandot för att acceptera en bytesförfrågan.
﻿    public record AcceptSwapCommand : IRequest
﻿    {
﻿        public Guid SwapRequestId { get; set; }
﻿
﻿        [JsonIgnore] // Vi hämtar detta från token, så Swagger ska inte visa det
﻿        public Guid CurrentUserId { get; set; }
﻿    }
﻿
﻿    // Svensk kommentar: Handläggaren som utför logiken för att acceptera en bytesförfrågan.
﻿    public class AcceptSwapHandler : IRequestHandler<AcceptSwapCommand>
﻿    {
﻿        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Logging.ILogger<AcceptSwapHandler> _logger;

﻿        public AcceptSwapHandler(
            IAppDbContext context,
            IEmailService emailService,
            Microsoft.Extensions.Logging.ILogger<AcceptSwapHandler> logger)
﻿        {
﻿            _context = context;
            _emailService = emailService;
            _logger = logger;
﻿        }
﻿
﻿        public async Task Handle(AcceptSwapCommand request, CancellationToken cancellationToken)
﻿        {
﻿            // A. Hämta bytet, inklusive ALLA relevanta pass (både det som ges och det som tas)
﻿            var swapRequest = await _context.SwapRequests
﻿                .Include(sr => sr.Shift)
﻿                .Include(sr => sr.TargetShift) // <-- VIKTIGT: Ladda in målpasset
﻿                .Include(sr => sr.RequestingUser) // <-- Ladda in användaren som frågade
﻿                .Include(sr => sr.TargetUser) // <-- Ladda in mål-användaren (om direktbyte)
﻿                .FirstOrDefaultAsync(sr => sr.Id == request.SwapRequestId, cancellationToken);
﻿
﻿            if (swapRequest == null) throw new Exception("Bytet hittades inte.");
﻿            if (swapRequest.Status != "Pending") throw new Exception("Det här bytet är inte längre tillgängligt.");
﻿
﻿            // B. Kontrollera om det är ett DIREKTBYTE eller ett ÖPPET BYTE
﻿            bool isDirectSwap = swapRequest.TargetShiftId.HasValue && swapRequest.TargetShift != null;
﻿
﻿            if (isDirectSwap)
﻿            {
﻿                // --- LOGIK FÖR DIREKTBYTE ---
﻿                // Säkerhetskoll: Endast den avsedda mottagaren (TargetUser) får acceptera ett direktbyte.
﻿                if (swapRequest.TargetUserId != request.CurrentUserId)
﻿                {
﻿                    throw new Exception("Du har inte behörighet att acceptera detta specifika byte.");
﻿                }
﻿
﻿                var originalShift = swapRequest.Shift;
﻿                var targetShift = swapRequest.TargetShift!; // Vi vet att den inte är null här
﻿                var requestingUserId = swapRequest.RequestingUserId;
﻿                var targetUserId = request.CurrentUserId;
﻿
﻿                // Krock-kontroll för BÅDA parter
﻿                // Exkludera passet som varje person lämnar ifrån sig — det tillhör dem inte efter bytet
﻿                if (swapRequest.RequestingUser == null) throw new Exception("Fel: Avsändarens användardata saknas.");
﻿                var requestorOverlap = await _context.Shifts.AnyAsync(s =>
﻿                    s.UserId == requestingUserId &&
﻿                    s.Id != targetShift.Id &&
﻿                    s.Id != originalShift.Id &&
﻿                    s.StartTime < targetShift.EndTime &&
﻿                    s.EndTime > targetShift.StartTime, cancellationToken);
﻿                if (requestorOverlap) throw new Exception($"Bytet kan inte genomföras eftersom {swapRequest.RequestingUser.FirstName} skulle få en passkrock.");

﻿                // Kollar om den som accepterar krockar med passet de får
﻿                if (swapRequest.TargetUser == null) throw new Exception("Fel: Mottagarens användardata saknas.");
﻿                var acceptorOverlap = await _context.Shifts.AnyAsync(s =>
﻿                    s.UserId == targetUserId &&
﻿                    s.Id != originalShift.Id &&
﻿                    s.Id != targetShift.Id &&
﻿                    s.StartTime < originalShift.EndTime &&
﻿                    s.EndTime > originalShift.StartTime, cancellationToken);
﻿                if (acceptorOverlap) throw new Exception("Bytet kan inte genomföras eftersom du skulle få en passkrock.");
﻿
﻿                // Genomför bytet: Byt ägare på båda passen
﻿                originalShift.UserId = targetUserId;
﻿                targetShift.UserId = requestingUserId;
﻿                originalShift.IsUpForSwap = false;
﻿                targetShift.IsUpForSwap = false;
﻿            }
﻿            else
﻿            {
﻿                // --- LOGIK FÖR ÖPPET BYTE (som förut) ---
﻿                var newShift = swapRequest.Shift;
﻿                // Robusthetskoll: Om Shift ändå skulle vara null här (datainkonsekvens)
﻿                if (newShift == null) throw new Exception("Fel: Passet för bytesförfrågan kunde inte hittas.");
﻿
﻿                // Krock-kontroll för den som tar passet
﻿                var hasOverlap = await _context.Shifts.AnyAsync(s =>
﻿                    s.UserId == request.CurrentUserId &&
﻿                    s.StartTime < newShift.EndTime &&
﻿                    s.EndTime > newShift.StartTime,
﻿                    cancellationToken);
﻿
﻿                if (hasOverlap)
﻿                {
﻿                    throw new Exception("Du har redan ett pass som krockar med detta!");
﻿                }
﻿                
﻿                // Genomför bytet
﻿                newShift.UserId = request.CurrentUserId;
﻿                newShift.IsUpForSwap = false;
﻿            }
﻿
﻿            // C. Avsluta förfrågan
﻿            swapRequest.Status = "Accepted";

﻿            // D. Spara alla ändringar
﻿            await _context.SaveChangesAsync(cancellationToken);

            // E. Skicka email till den som föreslog bytet (fire-and-forget)
            try
            {
                if (swapRequest.RequestingUser != null)
                {
                    var culture = new System.Globalization.CultureInfo("sv-SE");
                    string emailBody;
                    string subject;

                    if (isDirectSwap && swapRequest.TargetUser != null)
                    {
                        // Direktbyte: Berätta vad de bytte
                        var myShiftDate = swapRequest.Shift.StartTime.ToString("dddd d MMMM", culture);
                        var myShiftTime = $"{swapRequest.Shift.StartTime:HH:mm} - {swapRequest.Shift.EndTime:HH:mm}";
                        var targetShiftDate = swapRequest.TargetShift!.StartTime.ToString("dddd d MMMM", culture);
                        var targetShiftTime = $"{swapRequest.TargetShift.StartTime:HH:mm} - {swapRequest.TargetShift.EndTime:HH:mm}";

                        subject = $"✅ {swapRequest.TargetUser.FirstName} godkände bytet!";
                        emailBody = $@"
                            <html>
                            <body style=""font-family: Arial, sans-serif; color: #333;"">
                                <div style=""max-width: 500px; border: 1px solid #eee; padding: 20px;"">
                                    <h2 style=""color: #28a745;"">Bytet är godkänt! 🎉</h2>
                                    <p>Hej <strong>{swapRequest.RequestingUser.FirstName}</strong>!</p>
                                    <p><strong>{swapRequest.TargetUser.FirstName} {swapRequest.TargetUser.LastName}</strong> har godkänt ditt bytesförslag.</p>
                                    <hr/>
                                    <p><strong>Du lämnade:</strong> {myShiftDate} ({myShiftTime})</p>
                                    <p><strong>Du fick:</strong> {targetShiftDate} ({targetShiftTime})</p>
                                    <hr/>
                                    <p style=""color: #666; font-size: 12px;"">Logga in på ShiftMate för att se ditt uppdaterade schema.</p>
                                </div>
                            </body>
                            </html>";
                    }
                    else
                    {
                        // Marketplace-byte: Någon tog deras pass
                        var acceptorName = swapRequest.TargetUser?.FirstName ?? "Någon";
                        var shiftDate = swapRequest.Shift.StartTime.ToString("dddd d MMMM", culture);
                        var shiftTime = $"{swapRequest.Shift.StartTime:HH:mm} - {swapRequest.Shift.EndTime:HH:mm}";

                        subject = $"✅ {acceptorName} tog ditt pass!";
                        emailBody = $@"
                            <html>
                            <body style=""font-family: Arial, sans-serif; color: #333;"">
                                <div style=""max-width: 500px; border: 1px solid #eee; padding: 20px;"">
                                    <h2 style=""color: #28a745;"">Ditt pass blev taget! 🎉</h2>
                                    <p>Hej <strong>{swapRequest.RequestingUser.FirstName}</strong>!</p>
                                    <p>Någon har tagit ditt pass från marknadsplatsen.</p>
                                    <hr/>
                                    <p><strong>Pass:</strong> {shiftDate} ({shiftTime})</p>
                                    <hr/>
                                    <p style=""color: #666; font-size: 12px;"">Logga in på ShiftMate för att se ditt uppdaterade schema.</p>
                                </div>
                            </body>
                            </html>";
                    }

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(
                                swapRequest.RequestingUser.Email,
                                subject,
                                emailBody
                            );
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
﻿        }
﻿    }
﻿}
﻿