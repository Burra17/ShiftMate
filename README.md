🛡️ ShiftMate Backend
ShiftMate är ett modernt schemaläggningssystem byggt med .NET 8 och Clean Architecture. Systemet tillåter användare att hantera sina arbetspass, begära byten med kollegor och hantera sin profil på ett säkert sätt.

🏗️ Arkitektur & Teknikstack
Projektet följer Clean Architecture för att separera affärslogik från infrastruktur:

Domain: Innehåller entiteter som User, Shift och SwapRequest.

Application: Hanterar affärslogik via MediatR (Commands/Queries) och använder DTOs för att leverera optimerad data.

Infrastructure: Hanterar databaskommunikation via Entity Framework Core och SQL Server.

API: RESTful controllers med JWT-autentisering och Swagger för dokumentation.

✨ Nyckelfunktioner (Hittills)
Säker Inloggning: JWT-baserad autentisering där användarens identitet skyddas.

Passhantering: Möjlighet att se personliga pass och tillgängliga byten med automatisk beräkning av tidsåtgång (durationHours).

Profilhantering: Användare kan uppdatera sina personuppgifter direkt mot databasen.

Automatisk Seeding: Systemet fyller automatiskt databasen med testdata (t.ex. "André" och "Boss Bossman") vid uppstart om den är tom.

Migrations: Fullständig versionshantering av databasschemat med EF Core Migrations.

🚀 Kom igång
Förutsättningar
.NET 8 SDK

SQL Server (LocalDB eller Express)

Visual Studio 2022

Installation
Klona repot.

Uppdatera ConnectionStrings i appsettings.json i ShiftMate.Api så den pekar på din lokala SQL-server.

Öppna Package Manager Console, välj ShiftMate.Infrastructure som default project och kör:

PowerShell
Update-Database
Starta projektet.

Testning
Använd Swagger-gränssnittet som dyker upp vid start.

Logga in med andre@shiftmate.com / dummy_hash_123 för att få din token.

Använd hänglåset (Authorize) i Swagger för att låsa upp skyddade endpoints med din token.
