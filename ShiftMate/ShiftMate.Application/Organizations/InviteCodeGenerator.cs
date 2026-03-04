using System.Security.Cryptography;

namespace ShiftMate.Application.Organizations
{
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
}
