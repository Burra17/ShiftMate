# ShiftMate

ShiftMate är en fullstack-applikation for skiftplanering och hantering av skiftbyten mellan anställda. Applikationen gör det enkelt för medarbetare att byta pass med varandra, ta lediga pass och för administratörer att skapa och tilldela skift.

## Funktioner

- **Användarhantering** - Registrering med inbjudningskod, inloggning med JWT-autentisering och profilhantering
- **Mina Pass** - Visa dina tilldelade skift och hantera inkommande bytesförfrågningar
- **Lediga Pass (Marknadsplats)** - Bläddra bland och ta otilldelade eller erbjudna skift
- **Schema** - Komplett schemaöversikt grupperad per datum med alla anställdas skift
- **Skiftbyten** - Erbjud skift på öppna marknaden eller föreslå direktbyten med kollegor
- **Bytesförfrågningar** - Acceptera, neka eller avbryt förfrågningar med fullständig livscykel
- **Manager-panel** - Skapa, redigera och ta bort skift, hantera användare (inaktivera med soft delete), visa/förnya inbjudningskod (rollbaserad åtkomst)
- **Admin-panel** - SuperAdmin kan hantera alla organisationer (skapa, redigera, ta bort)
- **Inbjudningskod** - Managers delar inbjudningskod, anställda anger den vid registrering
- **Rollbaserad navigation** - Manager/SuperAdmin-funktioner visas bara för rätt roll

## Teknikstack

### Backend (.NET 8)

- **Framework:** ASP.NET Core Web API (C#)
- **Databas:** PostgreSQL (hostat på Supabase)
- **ORM:** Entity Framework Core 8
- **Arkitektur:** Clean Architecture med CQRS via MediatR
- **Validering:** FluentValidation
- **Autentisering:** JWT (JSON Web Tokens) med rollbaserade claims
- **Lösenord:** BCrypt-hashning
- **E-post:** Resend HTTP API

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

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22 eller senare](https://nodejs.org/en/download/) (versionen står i `ShiftMate/shiftmate-frontend/.nvmrc` – använder du nvm räcker `nvm use`)
- Ett gratis [Supabase](https://supabase.com/)-projekt som din lokala PostgreSQL-databas
- Git

### 1. Klona repot

```bash
git clone https://github.com/Burra17/ShiftMate.git
cd ShiftMate/ShiftMate
```

Solution-filen `ShiftMate.sln` ligger i undermappen `ShiftMate/` – alla kommandon nedan körs därifrån.

### 2. Lita på utvecklingscertifikatet (en gång per dator)

API:et körs på HTTPS lokalt och behöver .NET:s utvecklingscertifikat:

```bash
dotnet dev-certs https --trust
```

### 3. Skapa databasen

1. Skapa ett nytt projekt på [supabase.com](https://supabase.com/) och spara databaslösenordet.
2. Gå till **Connect** → **Session pooler** och kopiera värdena för host och användarnamn.
3. Anslutningssträngen ska ha formatet:
   ```
   Host=<host>;Port=5432;Database=postgres;Username=<användarnamn>;Password=<lösenord>;Ssl Mode=Require;Trust Server Certificate=true
   ```

Tabellerna skapas automatiskt med EF Core-migreringar när API:et startar i Development.

### 4. Konfigurera hemligheter med user-secrets

Hemligheter läggs **aldrig** i `appsettings.json` eller i git. De sparas lokalt med [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (utanför repot, per dator):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<din anslutningssträng från steg 3>" --project ShiftMate.Api
dotnet user-secrets set "Jwt:Key" "<slumpad nyckel, minst 32 tecken>" --project ShiftMate.Api
dotnet user-secrets set "Jwt:Issuer" "ShiftMate" --project ShiftMate.Api
dotnet user-secrets set "Jwt:Audience" "ShiftMate" --project ShiftMate.Api
```

Generera en slumpad JWT-nyckel:

```bash
# Git Bash / macOS / Linux
openssl rand -base64 48
```

```powershell
# PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

Kontrollera med `dotnet user-secrets list --project ShiftMate.Api`.

**E-post (valfritt):** Utan Resend-nyckel skickas inga riktiga mejl i Development – istället loggas mottagare, ämne och länkar (t.ex. verifierings- och återställningslänk) i API-terminalen, så kopiera länken därifrån. Vill du skicka riktiga mejl, skapa ett gratis konto på [resend.com](https://resend.com/) och sätt:

```bash
dotnet user-secrets set "Resend:ApiKey" "<din nyckel>" --project ShiftMate.Api
```

### 5. Starta backend

```bash
dotnet run --project ShiftMate.Api
```

API:et startar på `https://localhost:7215` (Swagger på `/swagger`). Vid första starten körs migreringarna och testdata seedas.

### 6. Starta frontend

I en ny terminal:

```bash
cd shiftmate-frontend
npm ci
npm run dev
```

Frontend startar på `http://localhost:5173`. Använd `npm ci` (inte `npm install`) så att `package-lock.json` inte skrivs om. Porten är låst – är 5173 upptagen avbryts starten, eftersom API:ets CORS bara tillåter den porten. Stäng i så fall den andra Vite-servern.

API-adressen läses från `VITE_API_URL` i `shiftmate-frontend/.env.development` (standard: `https://localhost:7215/api`).

### 7. Logga in med testkonton

I Development seedas två organisationer och följande konton:

| Roll | E-post | Lösenord | Organisation |
|------|--------|----------|--------------|
| SuperAdmin | `superadmin@shiftmate.com` | `SuperAdmin123` | – |
| Manager | `manager@shiftmate.com` | `Manager123` | ShiftMate Demo |
| Employee | `anna@shiftmate.com` | `Andersson123` | ShiftMate Demo |
| Employee | `sara@shiftmate.com` | `Svensson123` | ShiftMate Demo |
| Employee | `mahmoud@shiftmate.com` | `Mahmoud123` | ShiftMate Demo |
| Manager | `test@testforetaget.se` | `TestPass123` | Testföretaget AB |

Vill du testa registrering behövs en inbjudningskod. I en ny databas är de `DEMO1234` (ShiftMate Demo) och `TEST5678` (Testföretaget AB). Koden kan förnyas av en Manager, och seeden skriver inte över den – fungerar koden inte, logga in som Manager och se den aktuella koden i **Manager-panelen → Inbjudningskod**.

### 8. Kör testerna

```bash
dotnet test
```

### Skapa en ny migrering

Behövs bara när du ändrar datamodellen. Kräver verktyget `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Namn> --project ShiftMate.Infrastructure --startup-project ShiftMate.Api
```

## Projektets filstruktur

```
├── AUDIT.md                               # Kodgranskning med prioriterade förbättringar
├── CLAUDE.md                              # Arkitekturdokumentation (Source of Truth)
├── PROJEKTBESKRIVNING.md                  # Projektbeskrivning
├── README.md                              # Denna fil
│
└── ShiftMate/                             # Solution-mapp
    ├── ShiftMate.sln
    ├── Dockerfile
    │
    ├── ShiftMate.Domain/                  # Domänlager - Entiteter & enums (inga beroenden)
    │   ├── Entities/                      # User, Shift, SwapRequest, Organization
    │   └── Enums/                         # Role, SwapRequestStatus
    │
    ├── ShiftMate.Application/             # Applikationslager - Affärslogik (CQRS)
    │   ├── DependencyInjection.cs
    │   ├── Common/                        # PagedResult + Exceptions/ (NotFound, Forbidden, etc.)
    │   ├── DTOs/
    │   ├── Interfaces/
    │   ├── Services/                      # EmailTemplateService
    │   ├── Shifts/Commands/[X]/ & Queries/[X]/        # Mapp per command/query
    │   ├── SwapRequests/Commands/[X]/ & Queries/[X]/
    │   ├── Users/Commands/[X]/ & Queries/[X]/
    │   └── Organizations/Commands/[X]/ & Queries/[X]/
    │
    ├── ShiftMate.Infrastructure/          # Infrastrukturlager - Dataåtkomst
    │   ├── AppDbContext.cs
    │   ├── DbInitializer.cs
    │   ├── Services/                      # ResendEmailService, ConsoleEmailService (dev)
    │   └── Migrations/
    │
    ├── ShiftMate.Api/                     # API-lager - HTTP
    │   ├── Program.cs
    │   ├── Controllers/                   # Tunna controllers (ingen try/catch)
    │   ├── Middleware/                    # ExceptionHandlingMiddleware (global felmappning)
    │   └── Extensions/                    # ClaimsPrincipalExtensions
    │
    ├── ShiftMate.Tests/                   # Enhetstester (201 tester)
    │
    └── shiftmate-frontend/                # React-applikation
        ├── src/
        │   ├── App.jsx
        │   ├── api.js
        │   ├── Dashboard.jsx
        │   ├── ShiftList.jsx
        │   ├── MarketPlace.jsx
        │   ├── Schedule.jsx
        │   ├── Profile.jsx
        │   ├── AdminPanel.jsx
        │   └── components/
        │       ├── AuthLayout.jsx
        │       └── ManagerPanel.jsx
        └── README.md
```

## Mer information

- **Arkitekturdokumentation:** Se [CLAUDE.md](CLAUDE.md) för detaljerade kodregler, mönster och konventioner.
- **Frontend-specifik information:** Se [shiftmate-frontend/README.md](ShiftMate/shiftmate-frontend/README.md).
