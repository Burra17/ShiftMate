namespace ShiftMate.Application.Common.Exceptions;

// Används när en gräns för belastning har nåtts, t.ex. när taket för antal demo-sandlådor är nått → 429.
public class TooManyRequestsException : Exception
{
    public TooManyRequestsException(string message) : base(message) { }
}
