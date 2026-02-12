using ShiftMate.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ShiftMate.Infrastructure.Services
{
    // Konfigurationsklass för Resend-inställningar
    public class ResendSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    // DTO för Resend API-anrop
    internal class ResendEmailRequest
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public string[] To { get; set; } = Array.Empty<string>();

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("html")]
        public string Html { get; set; } = string.Empty;
    }

    // Implementering av e-posttjänsten med Resend HTTP API
    public class ResendEmailService : IEmailService
    {
        private readonly ResendSettings _settings;
        private readonly ILogger<ResendEmailService> _logger;
        private readonly HttpClient _httpClient;
        private const string ResendApiUrl = "https://api.resend.com/emails";

        // Konstruktor som tar in HttpClient, konfiguration och logger
        public ResendEmailService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ResendEmailService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Hämta Resend-inställningar från konfiguration
            _settings = configuration.GetSection("Resend").Get<ResendSettings>()
                ?? throw new ArgumentNullException(nameof(ResendSettings), "Resend-sektionen saknas i konfigurationen.");

            // Validera kritiska inställningar
            if (string.IsNullOrEmpty(_settings.ApiKey))
                throw new ArgumentNullException(nameof(_settings.ApiKey), "Resend ApiKey saknas i konfigurationen.");

            if (string.IsNullOrEmpty(_settings.FromEmail))
                throw new ArgumentNullException(nameof(_settings.FromEmail), "Resend FromEmail saknas i konfigurationen.");

            // Konfigurera HttpClient med API-nyckel (görs en gång)
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }

        // Metod för att skicka e-post asynkront via Resend API
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                // Bygg Resend-request
                var emailRequest = new ResendEmailRequest
                {
                    From = string.IsNullOrEmpty(_settings.FromName)
                        ? _settings.FromEmail
                        : $"{_settings.FromName} <{_settings.FromEmail}>",
                    To = new[] { toEmail },
                    Subject = subject,
                    Html = message
                };

                // Skicka POST till Resend API
                var response = await _httpClient.PostAsJsonAsync(ResendApiUrl, emailRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("E-post skickat till {ToEmail} med ämne: {Subject}", toEmail, subject);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Resend API-fel vid sändning till {ToEmail}. Status: {StatusCode}, Svar: {Response}",
                        toEmail,
                        response.StatusCode,
                        errorContent
                    );
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-fel vid sändning av e-post till {ToEmail}.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Okänt fel vid sändning av e-post till {ToEmail}.", toEmail);
            }
        }
    }
}
