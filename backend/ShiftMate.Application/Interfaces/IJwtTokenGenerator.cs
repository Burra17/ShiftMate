using ShiftMate.Domain.Entities;

namespace ShiftMate.Application.Interfaces;

// Skapar JWT-token för en inloggad användare.
// Användaren måste vara laddad med Organization (Include), så att OrganizationName kommer med i token.
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
