using System.Security.Claims;

namespace ShiftMate.Api.Extensions
{
    // Hjälpmetoder för att extrahera användarinformation från JWT-claims.
    public static class ClaimsPrincipalExtensions
    {
        // Hämtar användarens Id (Guid) från JWT-tokenet.
        // Returnerar null om claimen saknas eller inte kan parsas.
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return null;
            return Guid.Parse(userIdString);
        }
    }
}
