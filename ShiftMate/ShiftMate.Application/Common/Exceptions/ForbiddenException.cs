namespace ShiftMate.Application.Common.Exceptions;

// Används när en användare försöker utföra en åtgärd som de inte har behörighet att utföra, t.ex. när de inte är inloggade eller inte har rätt roll.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
