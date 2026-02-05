using Microsoft.AspNetCore.Authentication.JwtBearer; // <--- NY: För JWT
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;                // <--- NY: För Token-validering
using Microsoft.OpenApi.Models;
using ShiftMate.Application;
using ShiftMate.Application.Interfaces;
using ShiftMate.Infrastructure;
using ShiftMate.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Text;                                   // <--- NY: För att läsa nyckeln (Encoding)
using System.Text.Json.Serialization;

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

    // Kräv säkerhet globalt
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Koppla in databasen
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrera interfacet IAppDbContext
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Koppla in Application-lagret (MediatR)
builder.Services.AddApplication();

// Registrera e-posttjänsten
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

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

// ---------------------------------------------------------
// 2. BYGG APPLIKATIONEN & KÖR SEEDER
// ---------------------------------------------------------
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
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

app.UseHttpsRedirection();

app.UseCors("AllowReactApp"); // <--- Denna måste ligga här!

// --- VIKTIGT: Authentication måste ligga FÖRE Authorization ---
app.UseAuthentication(); // <--- Kollar VEM du är (Har du biljett?)
app.UseAuthorization();  // <--- Kollar VAD du får göra (Får du komma in?)

app.MapControllers();

// --- HEALTH CHECK (För UptimeRobot) ---
// En enkel endpoint som bara svarar 200 OK.
// Används för att hålla Render-servern vaken.
app.MapGet("/health", () => Results.Ok("ShiftMate is alive! 🤖"));

app.Run();