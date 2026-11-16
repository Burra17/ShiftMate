namespace ShiftMate.Application.Common.Exceptions;

// Anpassad exception som kastas när en användares e-post inte är verifierad, vilket krävs för att logga in eller utföra vissa åtgärder i systemet.
public class EmailNotVerifiedException : Exception
{
    public EmailNotVerifiedException(string message) : base(message) { }
}
