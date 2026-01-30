using Microsoft.EntityFrameworkCore;
using ShiftMate.Infrastructure;          // För AppDbContext och DbInitializer
using ShiftMate.Application;             // För AddApplication (MediatR)
using ShiftMate.Application.Interfaces;  // <--- VIKTIGT: För IAppDbContext
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. KONFIGURERA TJÄNSTER (DEPENDENCY INJECTION)
// ---------------------------------------------------------

// Lägg till Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Om vi stöter på en loop, ignorera den istället för att krascha
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Lägg till Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Registrera den "riktiga" databasen (AppDbContext)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registrera interfacet (IAppDbContext)
// Detta säger: "Om någon ber om IAppDbContext, ge dem den AppDbContext vi skapade ovan."
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// 3. Registrera Application-lagret (MediatR)
builder.Services.AddApplication();

// ---------------------------------------------------------
// 2. BYGG APPLIKATIONEN & KÖR SEEDER
// ---------------------------------------------------------
var app = builder.Build();

// Skapa en tillfällig "Scope" för att hämta databasen och köra seedern
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Hämta databasen
        var context = services.GetRequiredService<AppDbContext>();

        // Kör seedern (fyller på med data om det är tomt)
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

app.UseAuthorization();

app.MapControllers();

app.Run();