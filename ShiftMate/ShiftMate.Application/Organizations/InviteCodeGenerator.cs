using System.Security.Cryptography;

namespace ShiftMate.Application.Organizations;

// En statisk klass som genererar unika invite codes för organisationer. Koden består av stora bokstäver och siffror, och är 8 tecken lång.
public static class InviteCodeGenerator
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CodeLength = 8;

    public static string GenerateInviteCode()
    {
        return string.Create(CodeLength, Chars, (span, chars) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
        });
    }
}
