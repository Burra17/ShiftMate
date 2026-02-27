namespace ShiftMate.Application.Services
{
    // Centraliserad email-template service.
    // FrontendUrl sätts en gång i Program.cs — inga DI-ändringar behövs i handlers.
    public static class EmailTemplateService
    {
        // Sätts vid uppstart — styr om knappar pekar på localhost eller produktion
        public static string FrontendUrl { get; set; } = "http://localhost:5173";

        // Färger
        private const string Green = "#16a34a";
        private const string Red = "#dc2626";
        private const string Blue = "#2563eb";
        private const string TextDark = "#111827";
        private const string TextMuted = "#6b7280";
        private const string BorderLight = "#e5e7eb";

        // Bas-layout som alla emails delar
        private static string Layout(string title, string body)
        {
            return $@"<!DOCTYPE html>
<html lang=""sv"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1.0"">
<title>{title}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f3f4f6;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding:32px 16px;"">
<tr><td align=""center"">
<table width=""520"" cellpadding=""0"" cellspacing=""0"" style=""max-width:520px;width:100%;background-color:#ffffff;border:1px solid {BorderLight};border-radius:8px;overflow:hidden;"">

  <tr>
    <td style=""padding:32px 32px 24px;border-bottom:1px solid {BorderLight};"">
      <h1 style=""margin:0;font-size:18px;font-weight:600;color:{TextDark};"">{title}</h1>
    </td>
  </tr>

  <tr>
    <td style=""padding:24px 32px;color:{TextDark};font-size:14px;line-height:1.6;"">
{body}
    </td>
  </tr>

  <tr>
    <td style=""padding:16px 32px;border-top:1px solid {BorderLight};text-align:center;"">
      <p style=""margin:0;color:{TextMuted};font-size:12px;"">ShiftMate — Schemahantering</p>
    </td>
  </tr>

</table>
</td></tr>
</table>
</body>
</html>";
        }

        // Informationsruta med färgad vänsterkant
        private static string InfoBox(string html, string borderColor)
        {
            return $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:16px 0;"">
<tr>
  <td width=""3"" style=""background-color:{borderColor};""></td>
  <td style=""background-color:#f9fafb;padding:12px 16px;font-size:14px;line-height:1.6;color:{TextDark};"">
{html}
  </td>
</tr>
</table>";
        }

        // CTA-knapp (tabell-baserad för kompatibilitet)
        private static string Button(string text, string path, string color)
        {
            var url = $"{FrontendUrl}{path}";
            return $@"<table cellpadding=""0"" cellspacing=""0"" style=""margin:20px 0;"">
<tr>
  <td style=""background-color:{color};border-radius:6px;"">
    <a href=""{url}"" target=""_blank"" style=""display:inline-block;padding:10px 24px;color:#ffffff;text-decoration:none;font-size:13px;font-weight:600;"">
      {text}
    </a>
  </td>
</tr>
</table>";
        }

        // Rad med pass-info (label, datum, tid)
        private static string ShiftRow(string label, string date, string time, string color)
        {
            return $@"<div style=""margin-bottom:8px;"">
<span style=""color:{color};font-size:12px;font-weight:600;text-transform:uppercase;"">{label}</span><br>
<span style=""font-weight:600;color:{TextDark};"">{date}</span><br>
<span style=""color:{TextMuted};font-size:13px;"">{time}</span>
</div>";
        }

        // --- Email-typer ---

        // Ny bytesförfrågan (någon vill byta med dig)
        public static string SwapProposal(
            string recipientName,
            string proposerFullName,
            string yourDate,
            string yourTime,
            string theirDate,
            string theirTime)
        {
            var body = $@"
      <p style=""margin:0 0 16px;"">Hej {recipientName},</p>
      <p style=""margin:0 0 16px;"">{proposerFullName} vill byta pass med dig:</p>
{InfoBox(
    ShiftRow("Du lämnar", yourDate, yourTime, Red) +
    ShiftRow("Du får", theirDate, theirTime, Green),
    Blue)}
{Button("Visa förfrågan", "/mine", Blue)}";

            return Layout("Ny bytesförfrågan", body);
        }

        // Direktbyte godkänt
        public static string DirectSwapAccepted(
            string recipientName,
            string acceptorFullName,
            string givenDate,
            string givenTime,
            string receivedDate,
            string receivedTime)
        {
            var body = $@"
      <p style=""margin:0 0 16px;"">Hej {recipientName},</p>
      <p style=""margin:0 0 16px;"">{acceptorFullName} har godkänt ditt bytesförslag.</p>
{InfoBox(
    ShiftRow("Du lämnade", givenDate, givenTime, Red) +
    ShiftRow("Du fick", receivedDate, receivedTime, Green),
    Green)}
{Button("Se ditt schema", "/schedule", Green)}";

            return Layout("Byte godkänt", body);
        }

        // Pass taget från lediga pass
        public static string MarketplaceShiftTaken(
            string recipientName,
            string takerFullName,
            string shiftDate,
            string shiftTime)
        {
            var body = $@"
      <p style=""margin:0 0 16px;"">Hej {recipientName},</p>
      <p style=""margin:0 0 16px;"">{takerFullName} har tagit ditt pass från lediga pass.</p>
{InfoBox(ShiftRow("Pass som togs", shiftDate, shiftTime, Green), Green)}
{Button("Se ditt schema", "/schedule", Green)}";

            return Layout("Ditt pass blev taget", body);
        }

        // Byte nekat
        public static string SwapDeclined(
            string recipientName,
            string declinerFullName,
            string shiftDate,
            string shiftTime)
        {
            var body = $@"
      <p style=""margin:0 0 16px;"">Hej {recipientName},</p>
      <p style=""margin:0 0 16px;"">{declinerFullName} har nekat ditt bytesförslag.</p>
{InfoBox(ShiftRow("Nekat pass", shiftDate, shiftTime, Red), Red)}
      <p style=""margin:16px 0;color:{TextMuted};font-size:13px;"">
        Du kan föreslå bytet till någon annan eller lägga upp passet under lediga pass.
      </p>
{Button("Gå till lediga pass", "/marketplace", Blue)}";

            return Layout("Byte nekat", body);
        }

        // Nytt pass tilldelat av admin
        public static string ShiftAssigned(
            string recipientName,
            string shiftDate,
            string shiftTime,
            double hours)
        {
            var body = $@"
      <p style=""margin:0 0 16px;"">Hej {recipientName},</p>
      <p style=""margin:0 0 16px;"">Du har tilldelats ett nytt arbetspass.</p>
{InfoBox(
    ShiftRow("Ditt nya pass", shiftDate, shiftTime, Blue) +
    $@"<span style=""color:{TextMuted};font-size:13px;"">{hours:F1} timmar</span>",
    Blue)}
{Button("Se ditt schema", "/schedule", Blue)}";

            return Layout("Nytt pass tilldelat", body);
        }
        // Lösenordsåterställning
        public static string PasswordReset(string recipientName, string resetPath)
        {
            var body = $@"
      <p style=""margin:0 0 16px;"">Hej {recipientName},</p>
      <p style=""margin:0 0 16px;"">Vi har fått en förfrågan om att återställa ditt lösenord. Klicka på knappen nedan för att välja ett nytt lösenord.</p>
{Button("Återställ lösenord", resetPath, Blue)}
      <p style=""margin:16px 0;color:{TextMuted};font-size:13px;"">
        Länken är giltig i 1 timme. Om du inte begärde detta kan du ignorera detta meddelande.
      </p>";

            return Layout("Återställ lösenord", body);
        }
    }
}
