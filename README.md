# ShiftMate

ShiftMate är en fullstack-applikation for skiftplanering och hantering av skiftbyten mellan anställda. Applikationen gör det enkelt för medarbetare att byta pass med varandra, ta lediga pass och för administratörer att skapa och tilldela skift.

## Funktioner

- **Användarhantering** - Registrering, inloggning med JWT-autentisering och profilhantering
- **Mina Pass** - Visa dina tilldelade skift och hantera inkommande bytesförfrågningar
- **Lediga Pass (Marknadsplats)** - Bläddra bland och ta otilldelade eller erbjudna skift
- **Schema** - Komplett schemaöversikt grupperad per datum med alla anställdas skift
- **Skiftbyten** - Erbjud skift på öppna marknaden eller föreslå direktbyten med kollegor
- **Bytesförfrågningar** - Acceptera, neka eller avbryt förfrågningar med fullständig livscykel
- **Admin-panel** - Skapa skift och tilldela till specifika användare (rollbaserad åtkomst)
- **Rollbaserad navigation** - Admin-funktioner visas bara för administratörer

## Teknikstack

### Backend (.NET 8)

- **Framework:** ASP.NET Core Web API (C#)
- **Databas:** PostgreSQL (hostat på Supabase)
- **ORM:** Entity Framework Core 8
- **Arkitektur:** Clean Architecture med CQRS via MediatR
- **Validering:** FluentValidation
- **Autentisering:** JWT (JSON Web Tokens) med rollbaserade claims
- **Lösenord:** BCrypt-hashning
- **E-post:** SMTP via Gmail

### Frontend (React 19 + Vite)

- **Core:** React 19, JavaScript (ES6+)
- **Byggverktyg:** Vite 7
- **Styling:** Tailwind CSS (Neon Dark-tema)
- **HTTP-klient:** Axios med JWT-interceptor
- **Routing:** React Router v7

### Driftsättning

- **Backend:** Render
- **Frontend:** Vercel
- **Container:** Docker (multi-stage build)

## Komma igång (Utveckling)

### Förkrav

- [.NET SDK 8.0 eller högre](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js och npm](https://nodejs.org/en/download/)
- En PostgreSQL-databas (t.ex. via [Supabase](https://supabase.com/) eller lokal installation)

### 1. Backend-Setup

1.  Navigera till projektets rotkatalog (där `ShiftMate.sln` ligger).
2.  Återställ NuGet-paket:
    ```bash
    dotnet restore
    ```
3.  Konfigurera din databasanslutningssträng i `ShiftMate.Api/appsettings.json`.
4.  Uppdatera databasen med senaste migreringarna:
    ```bash
    dotnet ef database update --project ShiftMate.Infrastructure --startup-project ShiftMate.Api
    ```
5.  Kör backend-API:et:
    ```bash
    dotnet run --project ShiftMate.Api
    ```
    API:et startar på `https://localhost:7215`.

### 2. Frontend-Setup

1.  Navigera till frontend-katalogen:
    ```bash
    cd shiftmate-frontend
    ```
2.  Installera npm-beroenden:
    ```bash
    npm install
    ```
3.  Konfigurera API-bas-URL i `.env.development` om det behövs (standard: `VITE_API_BASE_URL=https://localhost:7215/api`).
4.  Starta frontend-utvecklingsservern:
    ```bash
    npm run dev
    ```
    Frontend startar på `http://localhost:5173`.

## Projektets filstruktur

```
ShiftMate/
├── ShiftMate.sln
├── Dockerfile
├── CLAUDE.md                              # Arkitekturdokumentation (Source of Truth)
├── MEMORY.md                              # Sessionslogg för utveckling
│
├── ShiftMate.Domain/                      # Domänlager - Entiteter (inga beroenden)
│   ├── User.cs
│   ├── Shift.cs
│   └── SwapRequest.cs
│
├── ShiftMate.Application/                 # Applikationslager - Affärslogik (CQRS)
│   ├── DependencyInjection.cs
│   ├── DTOs/
│   │   ├── UserDto.cs
│   │   ├── ShiftDto.cs
│   │   └── SwapRequestDto.cs
│   ├── Interfaces/
│   │   ├── IAppDbContext.cs
│   │   └── IEmailService.cs
│   ├── Shifts/
│   │   ├── Commands/
│   │   │   ├── CreateShiftCommand.cs
│   │   │   ├── CreateShiftCommandValidator.cs
│   │   │   ├── TakeShiftCommand.cs
│   │   │   ├── TakeShiftCommandHandler.cs
│   │   │   ├── CancelShiftSwapCommand.cs
│   │   │   └── CancelShiftSwapCommandHandler.cs
│   │   └── Queries/
│   │       ├── GetAllShiftsQuery.cs
│   │       ├── GetAllShiftsHandler.cs
│   │       ├── GetMyShiftsQuery.cs
│   │       └── GetClaimableShiftsQuery.cs
│   ├── SwapRequests/
│   │   ├── Commands/
│   │   │   ├── InitiateSwapCommand.cs
│   │   │   ├── ProposeDirectSwapCommand.cs
│   │   │   ├── AcceptSwapCommand.cs
│   │   │   ├── DeclineSwapRequestCommand.cs
│   │   │   └── CancelSwapRequestCommand.cs
│   │   └── Queries/
│   │       ├── GetAvailableSwapsQuery.cs
│   │       └── GetReceivedSwapRequestsQuery.cs
│   └── Users/
│       ├── Commands/
│       │   ├── RegisterUserCommand.cs
│       │   ├── RegisterUserCommandHandler.cs
│       │   ├── RegisterUserCommandValidator.cs
│       │   ├── LoginCommand.cs
│       │   └── UpdateProfileCommand.cs
│       └── Queries/
│           └── GetAllUsersQuery.cs
│
├── ShiftMate.Infrastructure/              # Infrastrukturlager - Dataåtkomst
│   ├── AppDbContext.cs
│   ├── DbInitializer.cs
│   ├── Services/
│   │   └── SmtpEmailService.cs
│   └── Migrations/
│       ├── 20260202220054_InitialCreate.cs
│       ├── 20260203170207_FixRolesAndAddSwapRequests.cs
│       ├── 20260204131820_AddTargetShiftIdToSwapRequest.cs
│       └── 20260206165707_AllowUnassignedShifts.cs
│
├── ShiftMate.Api/                         # API-lager - Controllers
│   ├── Program.cs
│   ├── appsettings.json
│   └── Controllers/
│       ├── UsersController.cs
│       ├── ShiftsController.cs
│       └── SwapRequestsController.cs
│
├── ShiftMate.Tests/                       # Enhetstester
│   ├── AcceptSwapHandlerTests.cs
│   ├── CreateShiftCommandValidatorTests.cs
│   ├── CreateShiftHandlerTests.cs
│   └── Support/
│       └── TestDbContextFactory.cs
│
└── shiftmate-frontend/                    # React-applikation
    ├── package.json
    ├── vite.config.js
    ├── tailwind.config.js
    ├── index.html
    ├── .env.development
    ├── .env.production
    ├── vercel.json
    └── src/
        ├── main.jsx
        ├── App.jsx
        ├── api.js
        ├── index.css
        ├── Login.jsx
        ├── Register.jsx
        ├── ShiftList.jsx
        ├── MarketPlace.jsx
        ├── Schedule.jsx
        ├── Profile.jsx
        └── components/
            ├── AuthLayout.jsx
            └── AdminPanel.jsx
```

## Mer information

- **Arkitekturdokumentation:** Se [CLAUDE.md](ShiftMate/CLAUDE.md) för detaljerade kodregler, mönster och konventioner.
- **Frontend-specifik information:** Se [shiftmate-frontend/README.md](ShiftMate/shiftmate-frontend/README.md).
