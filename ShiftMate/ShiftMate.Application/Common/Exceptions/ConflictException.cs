namespace ShiftMate.Application.Common.Exceptions;

// Används när en resurs inte kan skapas eller uppdateras på grund av en konflikt, t.ex. när en användare försöker skapa en resurs som redan finns.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
