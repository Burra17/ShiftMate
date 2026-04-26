namespace ShiftMate.Application.Common.Exceptions;

// Anpassad undantagsklass för att hantera fall där en resurs inte hittas.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
