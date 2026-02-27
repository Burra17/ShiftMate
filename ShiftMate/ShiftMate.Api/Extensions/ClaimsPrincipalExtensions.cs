using System.Security.Claims;

namespace ShiftMate.Api.Extensions
{
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
}
