using Microsoft.EntityFrameworkCore; // Behövs för att använda SQL Server-inställningar
using ShiftMate.Infrastructure;      // Behövs för att hitta din AppDbContext

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. KONFIGURERA TJÄNSTER (DEPENDENCY INJECTION)
// ---------------------------------------------------------

// Lägg till Controllers (hanterar API-anropen)
builder.Services.AddControllers();

// Lägg till Swagger (dokumentation för API:et)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// VIKTIGT: Koppla in databasen här!
// Den hämtar "DefaultConnection" från din appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------
// 2. BYGG APPLIKATIONEN
// ---------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------
// 3. KONFIGURERA HTTP-PIPELINE (HUR ANROP HANTERAS)
// ---------------------------------------------------------

// Om vi kör lokalt (Development), visa Swagger-sidan
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Starta appen
app.Run();