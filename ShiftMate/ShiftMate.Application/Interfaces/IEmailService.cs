using System.Threading.Tasks;

namespace ShiftMate.Application.Interfaces
{
    public interface IEmailService // Skapa interface för e-posttjänst
    {
        Task SendEmailAsync(string toEmail, string subject, string message); // Metod för att skicka e-post
    }
}
