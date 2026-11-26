namespace ShiftMate.Application.Demo;

// Regler för demo-sandlådorna som besökare från portfolion får.
public static class DemoConstants
{
    // Reserverad toppdomän (.invalid) – kan aldrig finnas på riktigt. DemoSafeEmailService hoppar över alla .invalid-adresser.
    public const string EmailDomain = "demo.shiftmate.invalid";

    // Token gäller 1 h, så efter 2 h kan ingen längre använda sandlådan och den städas bort.
    public static readonly TimeSpan SandboxLifetime = TimeSpan.FromHours(2);

    // Tak för antal samtidiga sandlådor, så att ingen kan fylla databasen.
    public const int MaxActiveSandboxes = 100;
}
