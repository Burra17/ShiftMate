using MediatR;
using ShiftMate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain;
using Microsoft.Extensions.Logging;

namespace ShiftMate.Application.Shifts.Commands
{
    public class TakeShiftCommandHandler : IRequestHandler<TakeShiftCommand, bool>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Logging.ILogger<TakeShiftCommandHandler> _logger;

        public TakeShiftCommandHandler(
            IAppDbContext context,
            IEmailService emailService,
            Microsoft.Extensions.Logging.ILogger<TakeShiftCommandHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> Handle(TakeShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. Hämta passet (inkl. nuvarande ägare för email-notis)
            var shift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            if (shift == null)
            {
                throw new Exception("Arbetspasset kunde inte hittas.");
            }

            // Validera att passet tillhör samma organisation
            if (shift.OrganizationId != request.OrganizationId)
            {
                throw new Exception("Passet tillhör inte din organisation.");
            }

            // 2. KONTROLLERA TILLGÄNGLIGHET
            // Vi kastar bara fel om passet INTE är för byte OCH det redan har en ägare.
            // Om UserId är null (öppet pass) så är det fritt fram att ta!
            if (!shift.IsUpForSwap && shift.UserId != null)
            {
                throw new Exception("Detta pass är inte tillgängligt för att tas.");
            }

            // 3. Hämta användaren
            var user = await _context.Users
                .Include(u => u.Shifts)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new Exception("Användaren kunde inte hittas.");
            }

            // 4. KROCK-KONTROLL
            // Kontrollera om användaren redan har ett pass på samma dag.
            // (Vi kollar dock inte mot passet vi försöker ta, ifall det av misstag redan står på oss)
            var newShiftDate = shift.StartTime.Date;

            bool hasShiftOnSameDay = user.Shifts.Any(s =>
                s.Id != shift.Id && // Ignorera passet vi försöker ta (om det mot förmodan redan var vårt)
                s.StartTime.Date == newShiftDate
            );

            if (hasShiftOnSameDay)
            {
                throw new Exception("Du kan inte ta ett pass på en dag där du redan har ett annat pass.");
            }

            // 5. UTFÖR UPPDATERINGEN
            // Spara originalägaren för email-notis (om passet var på marketplace)
            var originalOwner = shift.User;
            var wasOnMarketplace = shift.IsUpForSwap && shift.UserId.HasValue;

            shift.UserId = request.UserId;
            shift.IsUpForSwap = false; // Nollställ bytes-flaggan
            shift.User = user;         // Uppdatera navigation property

            // Spara
            await _context.SaveChangesAsync(cancellationToken);

            // 6. SKICKA EMAIL till originalägaren (om passet var på marketplace)
            if (wasOnMarketplace && originalOwner != null)
            {
                try
                {
                    var culture = new System.Globalization.CultureInfo("sv-SE");
                    var subject = $"✅ {user.FirstName} tog ditt pass!";
                    var emailBody = Services.EmailTemplateService.MarketplaceShiftTaken(
                        originalOwner.FirstName,
                        $"{user.FirstName} {user.LastName}",
                        shift.StartTime.ToString("dddd d MMMM", culture),
                        $"{shift.StartTime:HH:mm} - {shift.EndTime:HH:mm}"
                    );

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(originalOwner.Email, subject, emailBody);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Kunde inte skicka marketplace-email till {Email}", originalOwner.Email);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Email-notifiering misslyckades för taget pass {Id}", shift.Id);
                }
            }

            return true;
        }
    }
}