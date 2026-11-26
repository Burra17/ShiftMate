using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ShiftMate.Api.Controllers;
using ShiftMate.Api.Middleware;
using ShiftMate.Application;
using ShiftMate.Application.Interfaces;
using ShiftMate.Infrastructure;
using ShiftMate.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. KONFIGURERA TJÄNSTER (DEPENDENCY INJECTION)
// ---------------------------------------------------------

// Lägg till Controllers + Hantera JSON-loopar
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Lägg till Swagger
builder.Services.AddEndpointsApiExplorer();

// --- JWT KONFIGURATION (LÖSNINGEN PÅ FELET) ---
// Här berättar vi för API:et att vi använder JWT som standard
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
// ------------------------------------------------

// Konfigurera Swagger för att hantera JWT-tokens (Hänglåset)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShiftMate API", Version = "v1" });

    // Definiera säkerhetsschemat
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Klistra in din token så här: Bearer {din token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Kräv säkerhet globalt – referensen till "Bearer" skapas mot dokumentet (OpenAPI.NET v2)
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Koppla in databasen
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrera interfacet IAppDbContext
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// JWT-token skapas på ett enda ställe (inloggning och, senare, demo-sandlådan)
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Databaskontroll för /health/db. Kör en riktig liten fråga (räknas som aktivitet, så att
// Supabase inte pausar databasen). Svaret spelar ingen roll – bara att frågan går igenom,
// så en tom tabell ger inte "Unhealthy".
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        customTestQuery: async (db, cancellationToken) =>
        {
            await db.Users.AnyAsync(cancellationToken);
            return true;
        });

// Koppla in Application-lagret (MediatR)
builder.Services.AddApplication();

// Registrera e-posttjänsten. Lokalt utan Resend-nyckel loggas mejlen i terminalen istället.
// I alla andra fall används Resend, som kraschar direkt om nyckeln saknas (fail fast i produktion).
var useConsoleEmail = builder.Environment.IsDevelopment() && string.IsNullOrEmpty(builder.Configuration["Resend:ApiKey"]);
if (useConsoleEmail)
{
    builder.Services.AddScoped<ConsoleEmailService>();
}
else
{
    builder.Services.AddHttpClient<ResendEmailService>();
}

// IEmailService är alltid DemoSafeEmailService runt den riktiga tjänsten – mejl till demo-användare skickas aldrig
builder.Services.AddScoped<IEmailService>(provider => new DemoSafeEmailService(
    useConsoleEmail
        ? provider.GetRequiredService<ConsoleEmailService>()
        : provider.GetRequiredService<ResendEmailService>(),
    provider.GetRequiredService<ILogger<DemoSafeEmailService>>()));

// Sätt FrontendUrl för email-templates (localhost i dev, Vercel i prod)
ShiftMate.Application.Services.EmailTemplateService.FrontendUrl =
    builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";

// --- CORS: Tillåt React-appen (Både lokalt och på Vercel) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins(
            "http://localhost:5173",                 // Din lokala dator
            "https://shiftmate-ruby.vercel.app"      // Din nya Vercel-adress
        )
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// --- RATE LIMIT: demo-sandlådor ---
// Globalt tak (inte per IP – bakom Renders proxy är klientens IP opålitlig och kan förfalskas).
// Antalet kan sänkas i tester via Demo:StartsPerMinute.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { Error = true, Message = "För många demo-starter just nu – försök igen om en minut." },
            cancellationToken);
    };
    options.AddFixedWindowLimiter(DemoController.RateLimitPolicy, limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue("Demo:StartsPerMinute", 20);
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

// ---------------------------------------------------------
// 2. BYGG APPLIKATIONEN & KÖR SEEDER
// ---------------------------------------------------------
var app = builder.Build();

// Lokalt: kör migreringar och seeda testdata – aldrig i produktion
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ett fel inträffade när databasen skulle fyllas med data.");
    }
}

// ---------------------------------------------------------
// 3. KONFIGURERA HTTP-PIPELINE
// ---------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global felhantering — måste ligga tidigt så den fångar allt nedströms
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowReactApp"); // <--- Denna måste ligga här!

// Efter CORS, så att även 429-svar får CORS-headers och kan läsas av frontend
app.UseRateLimiter();

// --- VIKTIGT: Authentication måste ligga FÖRE Authorization ---
app.UseAuthentication(); // <--- Kollar VEM du är (Har du biljett?)
app.UseAuthorization();  // <--- Kollar VAD du får göra (Får du komma in?)

app.MapControllers();

// --- HEALTH CHECKS ---
// /health – liveness: "processen lever". Render använder den för att avgöra om appen ska startas om,
// och UptimeRobot pingar den var 5:e minut så att Render inte somnar. Rör ALDRIG databasen här –
// annars startar Render om API:et varje gång databasen har problem.
app.MapGet("/health", () => Results.Ok("ShiftMate is alive! 🤖"));

// /health/db – readiness: "appen når databasen". 200 Healthy / 503 Unhealthy.
// UptimeRobot pingar den varje timme så att Supabase (gratisnivån) inte pausar databasen.
app.MapHealthChecks("/health/db");

app.Run();

// Gör Program-klassen synlig för integrationstester (WebApplicationFactory<Program>)
public partial class Program { }
