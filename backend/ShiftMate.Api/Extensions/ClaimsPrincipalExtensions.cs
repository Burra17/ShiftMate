using System.Security.Claims;

namespace ShiftMate.Api.Extensions;

// Extension-metoder för att enkelt hämta UserId och OrganizationId från ClaimsPrincipal i controllers
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return null;
        return Guid.Parse(userIdString);
    }

    public static Guid? GetOrganizationId(this ClaimsPrincipal user)
    {
        var orgIdString = user.FindFirstValue("OrganizationId");
        if (string.IsNullOrEmpty(orgIdString)) return null;
        if (Guid.TryParse(orgIdString, out var orgId)) return orgId;
        return null;
    }
}
