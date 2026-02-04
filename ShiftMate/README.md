# ShiftMate

ShiftMate är en applikation för skiftplanering och hantering av skiftbyten, designad för att förenkla administrationen för både anställda och administratörer.

## Tekniker som Används

*   **Backend:** .NET 8 Web API (C#), Entity Framework Core, PostgreSQL (via Supabase), CQRS med MediatR, JWT för autentisering.
*   **Frontend:** React 18 (JavaScript/Vite), Tailwind CSS, Axios för HTTP-anrop, React Router v6.

## Komma Igång (Utveckling)

Följ dessa steg för att få igång ShiftMate lokalt.

### Förkrav

*   [.NET SDK 8.0 eller högre](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js och npm (eller Yarn)](https://nodejs.org/en/download/)
*   En PostgreSQL-databas (t.ex. via [Supabase](https://supabase.com/) eller lokal installation)

### 1. Backend-Setup

1.  Navigera till projektets rotkatalog:
    ```bash
    cd ShiftMate.Api
    ```
2.  Återställ NuGet-paket:
    ```bash
    dotnet restore
    ```
3.  Konfigurera din databasanslutningssträng i `ShiftMate.Api/appsettings.Development.json` (eller motsvarande för produktionsmiljö).
4.  Uppdatera databasen med senaste migreringarna:
    ```bash
    dotnet ef database update --project ShiftMate.Infrastructure
    ```
5.  Kör backend-API:et:
    ```bash
    dotnet run --project ShiftMate.Api
    ```
    API:et kommer normalt att starta på `https://localhost:7001` (kontrollera `launchSettings.json`).

### 2. Frontend-Setup

1.  Navigera till frontend-katalogen:
    ```bash
    cd shiftmate-frontend
    ```
2.  Installera npm-beroenden:
    ```bash
    npm install
    ```
3.  Konfigurera API-bas-URL i `shiftmate-frontend/.env.development` om det behövs (t.ex. `VITE_API_BASE_URL=https://localhost:7001`).
4.  Starta frontend-utvecklingsservern:
    ```bash
    npm run dev
    ```
    Frontend kommer normalt att starta på `http://localhost:5173`.

### Huvudfunktioner

*   Användarhantering (registrering, inloggning, profil).
*   Skiftplanering och visning av skiftscheman.
*   Funktion för att lägga upp skift för byte.
*   Hantering av bytesförfrågningar mellan anställda.
*   Admin-funktioner för skift- och användarhantering.

### Mer Detaljerad Frontend-Information

För frontend-specifik information, se [shiftmate-frontend/README.md](shiftmate-frontend/README.md).

---

## Projektets Filstruktur (Backend och Core)

```
.
├── Dockerfile
├── GEMINI.md
├── package-lock.json
├── ShiftMate.sln
├── ShiftMate.Api/
│   ├── Controllers/
│   │   ├── ShiftsController.cs
│   │   ├── SwapRequestsController.cs
│   │   └── UsersController.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs
│   ├── ShiftMate.Api.csproj
│   ├── ShiftMate.Api.csproj.user
│   ├── ShiftMate.Api.http
│   ├── appsettings.Development.json
│   └── appsettings.json
├── ShiftMate.Application/
│   ├── DTOs/
│   │   ├── ShiftDto.cs
│   │   ├── SwapRequestDto.cs
│   │   └── UserDto.cs
│   ├── Interfaces/
│   │   └── IAppDbContext.cs
│   ├── Shifts/
│   │   ├── Commands/
│   │   │   ├── CancelShiftSwapCommand.cs
│   │   │   ├── CancelShiftSwapCommandHandler.cs
│   │   │   ├── CreateShiftCommand.cs
│   │   │   ├── CreateShiftCommandValidator.cs
│   │   │   ├── TakeShiftCommand.cs
│   │   │   └── TakeShiftCommandHandler.cs
│   │   └── Queries/
│   │       ├── GetAllShiftsHandler.cs
│   │       ├── GetAllShiftsQuery.cs
│   │       └── GetMyShiftsQuery.cs
│   ├── SwapRequests/
│   │   ├── Commands/
│   │   │   ├── AcceptSwapCommand.cs
│   │   │   ├── CancelSwapRequestCommand.cs
│   │   │   └── InitiateSwapCommand.cs
│   │   └── Queries/
│   │       └── GetAvailableSwapsQuery.cs
│   ├── Users/
│   │   ├── Commands/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── RegisterUserCommand.cs
│   │   │   ├── RegisterUserCommandHandler.cs
│   │   │   ├── RegisterUserCommandValidator.cs
│   │   │   └── UpdateProfileCommand.cs
│   │   └── Queries/
│   │       └── GetAllUsersQuery.cs
│   ├── DependencyInjection.cs
│   └── ShiftMate.Application.csproj
├── ShiftMate.Domain/
│   ├── Shift.cs
│   ├── ShiftMate.Domain.csproj
│   ├── SwapRequest.cs
│   └── User.cs
├── ShiftMate.Infrastructure/
│   ├── Migrations/
│   │   ├── 20260202220054_InitialCreate.Designer.cs
│   │   ├── 20260202220054_InitialCreate.cs
│   │   ├── 20260203170207_FixRolesAndAddSwapRequests.Designer.cs
│   │   ├── 20260203170207_FixRolesAndAddSwapRequests.cs
│   │   └── AppDbContextModelSnapshot.cs
│   ├── AppDbContext.cs
│   ├── DbInitializer.cs
│   └── ShiftMate.Infrastructure.csproj
└── ShiftMate.Tests/
    ├── Support/
    │   └── TestDbContextFactory.cs
    ├── AcceptSwapHandlerTests.cs
    ├── CreateShiftCommandValidatorTests.cs
    ├── CreateShiftHandlerTests.cs
    └── ShiftMate.Tests.csproj
```