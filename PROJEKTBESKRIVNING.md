# ShiftMate - Projektbeskrivning

## Om projektet

ShiftMate är ett fullständigt skifthanteringssystem som jag byggt själv från grunden - backend, frontend och deployment. Projektet löser ett verkligt problem: att hantera arbetspass, skiftbyten och schemaläggning för arbetsplatser med skiftarbetande personal.
Jag har designat och utvecklat hela systemet som **solo fullstack-utvecklare**, från uml klassdiagram och databasdesign till API-endpoints, React-komponenter och produktionsdeployment. Projektet är byggt med **Clean Architecture** och följer **CQRS-mönstret** för att separera kommandon och queries.
Jag har lärt mig mycket under utvecklingen och försökt använda moderna verktyg och bästa praxis, inklusive: JWT-autentisering, FluentValidation, MediatR, Docker, CI/CD, och AI-assistenter som Gemini CLI och Claude Code.
Sen har jag också löst flera komplexa tekniska utmaningar, som att migrera från Gmail SMTP till Resend HTTP API, hantera passkrockar vid direktbyten och fixa UTC-buggar i Npgsql 8.
Jag har använt Swagger för första gången för att dokumentera API:et under utveckling, vilket gjorde det mycket enklare att testa endpoints och förstå dataflödet.


---

## Vad systemet gör

### Kärnfunktionalitet

- **Skifthantering** - Skapa, tilldela och hantera arbetspass med krock-kontroll som förhindrar dubbelbokning
- **Marknadsplats för pass** - Anställda kan lägga ut pass till byte och andra kan plocka upp lediga pass
- **Direktbyten** - Föreslå passbyten direkt till kollegor med godkännande-/avvisningsflöde
- **Schemaöversikt** - Tre vyer (dag/vecka/månad) för att se hela arbetsplatsens schema
- **Adminpanel** - Administratörer kan skapa och tilldela pass med snabbvalsförval
- **Profilhantering** - Personlig statistik, lösenordsbyte och profilredigering
- **E-postnotifikationer** - Automatiska mail vid passbyte, tilldelning och förfrågningar via Resend HTTP API

### Användarroller

Systemet stödjer tre roller med rollbaserad åtkomstkontroll:
- **Admin** - Full access: skapa/tilldela pass, se all data, adminpanel
- **Manager** - Utökad åtkomst för teamhantering
- **Employee** - Standardanvändare: egna pass, byten, marknadsplats

---

## Teknikstack

### Backend - .NET 8

| Teknologi | Användning |
|-----------|------------|
| ASP.NET Core Web API | HTTP-lager och REST-endpoints |
| Entity Framework Core 8 | ORM mot PostgreSQL |
| MediatR 12.4 | CQRS-mönstret - separerar kommandon och queries |
| FluentValidation 12.1 | Validering i Application-lagret |
| BCrypt.Net-Next 4.0 | Lösenordshashing med automatisk salt-generering |
| JWT Bearer Authentication | Token-baserad autentisering med claims |
| Swashbuckle (Swagger) | API-dokumentation i utvecklingsmiljö |
| Resend HTTP API | Transaktionella e-postnotifikationer |

### Frontend - React 19

| Teknologi | Användning |
|-----------|------------|
| React 19 | UI-bibliotek, enbart funktionella komponenter |
| Vite 7.2 | Byggverktyg med HMR |
| React Router v7 | SPA-routing med skyddade routes |
| Axios | HTTP-klient med JWT-interceptor och centraliserade API-anrop |
| Tailwind CSS 4.1 | Utility-first styling med neon dark-tema |

### Databas & Hosting

| Tjänst | Användning |
|--------|------------|
| PostgreSQL (Supabase) | Relationsdatabas - en instans för utveckling, en för produktion |
| Render | Backend-hosting med Docker och health check |
| Vercel | Frontend-hosting med automatisk deploy |
| Docker | Multi-stage build för containeriserad backend |

---

## Arkitektur - Clean Architecture

Projektet följer **Clean Architecture** med fyra separerade lager, där beroenden alltid pekar inåt:

```
┌─────────────────────────────────────────────┐
│              ShiftMate.Api                  │  ← HTTP-lager (Controllers, Program.cs)
│         Tunna controllers, JWT, CORS        │
├─────────────────────────────────────────────┤
│          ShiftMate.Infrastructure           │  ← Data & externa tjänster
│      AppDbContext, Migrations, Email        │
├─────────────────────────────────────────────┤
│          ShiftMate.Application              │  ← Affärslogik (CQRS)
│   Commands, Queries, DTOs, Validering       │
├─────────────────────────────────────────────┤
│            ShiftMate.Domain                 │  ← Entiteter (User, Shift, SwapRequest)
│         Inga beroenden alls                 │
└─────────────────────────────────────────────┘
```

### CQRS-mönstret med MediatR

All affärslogik hanteras genom **Command/Query Responsibility Segregation**:

- **Commands** (skrivoperationer) - `CreateShiftCommand`, `AcceptSwapCommand`, `RegisterUserCommand` osv.
- **Queries** (läsoperationer) - `GetAllShiftsQuery`, `GetMyShiftsQuery`, `GetClaimableShiftsQuery` osv.
- **Handlers** - Varje command/query har en dedikerad handler med all logik
- **Validators** - FluentValidation-klasser validerar input innan handlern körs

Ett typiskt flöde ser ut så här:

```
Controller → _mediator.Send(command) → Validator → Handler → DbContext → Svar
```

Controllers är medvetet **tunna** - de extraherar bara användar-ID från JWT-claims och delegerar till MediatR. All affärslogik, krock-kontroller och databasoperationer lever i handlers.

### Varför denna arkitektur?

- **Testbart** - Handlers kan enhetstestas utan HTTP-kontext
- **Separation of Concerns** - Varje lager har ett tydligt ansvar
- **Skalbart** - Nya features läggs till som nya commands/queries utan att röra befintlig kod
- **Inga cirkulära beroenden** - Domain-lagret har noll beroenden, Application beror bara på Domain

---

## Konventioner

### Språkkonvention

- **Kod** (variabler, klasser, metoder) - Engelska
- **Kommentarer och förklaringar** - Svenska

### Backend-regler

- Entiteter returneras **aldrig** direkt från API:et - alltid via DTOs
- Manuell mapping (Select till DTO) istället för AutoMapper - mer kontroll, explicit
- `AsNoTracking()` på alla read-only queries för prestanda
- UTC-normalisering tidigt i handlers (Npgsql 8 kräver `DateTimeKind.Utc` för `timestamptz`)
- Fire-and-forget-pattern för e-post med `Task.Run()` - e-postfel ska inte krascha requests

### Frontend-regler

- Enbart **funktionella komponenter** med hooks
- Alla API-anrop centraliserade i `api.js` med 30+ hjälpfunktioner - ingen direkt Axios i komponenter
- Axios-interceptor hanterar 401 automatiskt (auto-utloggning vid utgången token)
- Tidsformatering med svensk locale (`sv-SE`)
- Konsekvent neon dark-tema: `bg-slate-950`, `text-blue-400`, `border-blue-500/30`

### Namngivning

| Typ | Konvention | Exempel |
|-----|------------|---------|
| Commands | PascalCase + "Command" | `CreateShiftCommand` |
| Handlers | PascalCase + "Handler" | `CreateShiftHandler` |
| DTOs | PascalCase + "Dto" | `ShiftDto`, `UserDto` |
| Queries | PascalCase + "Query" | `GetAllShiftsQuery` |
| Privata fält | _camelCase | `_context`, `_emailService` |
| Frontend-filer | PascalCase (komponenter) | `ShiftList.jsx`, `MarketPlace.jsx` |

---

## Testning

### Ramverk och verktyg

| Paket | Version | Användning |
|-------|---------|------------|
| xUnit | 2.5.3 | Testramverk |
| Moq | 4.20.72 | Mocking av beroenden (IEmailService, ILogger, IValidator) |
| FluentAssertions | 8.8.0 | Läsbara assertions (`Should().NotBeEmpty()`, `Should().ThrowAsync()`) |
| EF Core InMemory | 8.0.23 | In-memory databas för isolerade tester |
| Coverlet | 6.0.0 | Kodtäckning |

### Teststruktur

Testerna ligger i `ShiftMate.Tests/` och följer **Arrange-Act-Assert**-mönstret med svenska kommentarer:

```csharp
[Fact]
public async Task Handle_Should_Throw_When_Shift_Overlaps()
{
    // ARRANGE — Skapa en användare som redan har ett pass
    var context = TestDbContextFactory.Create();
    // ... seed data ...

    // ACT & ASSERT — Ska kasta undantag vid passkrock
    await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
        .Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Denna användare har redan ett pass som krockar med den valda tiden.");
}
```

### Vad som testas

- **CreateShiftHandlerTests** (4 tester)
  - Sparar pass korrekt i databasen
  - Skapar öppet pass utan tilldelad användare
  - Kastar undantag när användaren inte finns
  - Kastar undantag vid passkrock (overlap-kontroll)

- **AcceptSwapHandlerTests** (6 tester)
  - Godkänner öppet byte utan krockar
  - Kastar undantag vid passkrock
  - Kastar undantag när bytesförfrågan inte finns
  - Kastar undantag när byte redan godkänts
  - Godkänner direktbyte samma dag
  - Avvisar direktbyte när tredje pass orsakar krock

- **CreateShiftCommandValidatorTests** - Validerar FluentValidation-regler

- **TestDbContextFactory** - Hjälpklass som skapar och river ner InMemory-databaser

### Köra tester

```bash
dotnet test
```

Alla 13 tester passerar med grönt resultat.

---

## Databashantering - Supabase (PostgreSQL)

### Två separata miljöer

Projektet använder **två separata Supabase-instanser** för att hålla utvecklings- och produktionsdata isolerade:

| Miljö | Syfte |
|-------|-------|
| **Development** | Lokal utveckling - seedad med testdata, säkert att experimentera med |
| **Production** | Produktionsmiljön på Render - riktig data |

Varje instans har sin egen connection string, JWT-nyckel och API-nycklar.

### Känslig konfiguration med User Secrets

Alla känsliga värden lagras med **.NET User Secrets** och exponeras **aldrig** i källkoden:

```bash
# Initiera (redan gjort - UserSecretsId finns i .csproj)
dotnet user-secrets init

# Exempel på hemligheter som lagras:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=..."
dotnet user-secrets set "Jwt:Key" "din-hemliga-jwt-nyckel"
dotnet user-secrets set "Jwt:Issuer" "ShiftMate"
dotnet user-secrets set "Jwt:Audience" "ShiftMateClient"
dotnet user-secrets set "Resend:ApiKey" "re_..."
```

User Secrets lagras lokalt på utvecklarens maskin (`%APPDATA%\Microsoft\UserSecrets\{id}\secrets.json`) och läses automatiskt av ASP.NET Core i Development-miljö. De committeras aldrig till Git.

I produktion sätts samma värden som **miljövariabler** på Render med dubbel underscore-notation:

```
ConnectionStrings__DefaultConnection = ...
Jwt__Key = ...
Resend__ApiKey = ...
```

### Databasschema

```
User (Id, FirstName, LastName, Email, PasswordHash, Role)
  │
  ├── Shift (Id, StartTime, EndTime, UserId?, IsUpForSwap)
  │
  └── SwapRequest (Id, ShiftId, RequestingUserId, TargetUserId?, TargetShiftId?, Status, CreatedAt)
```

Migrationer hanteras med **EF Core Migrations** (4 migrationer applicerade) och körs automatiskt vid uppstart via `DbInitializer`.

---

## Deployment

### Backend - Render (Docker)

Backenden deployas via en **multi-stage Dockerfile** som minimerar image-storleken:

```dockerfile
# Steg 1: Bygg med SDK (stor image, alla verktyg)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "ShiftMate.Api.csproj" -c Release -o /app/publish

# Steg 2: Kör med runtime (liten image, bara det som behövs)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShiftMate.Api.dll"]
```

**Render-konfiguration:**
- Automatisk deploy vid push till `main`
- Health check endpoint: `GET /health` (håller instansen vaken)
- Miljövariabler för alla hemligheter (connection strings, JWT, Resend API-nyckel)
- URL: `https://shiftmate-vow0.onrender.com`

### Frontend - Vercel

Frontend deployas automatiskt via Vercel:
- Bygger med `vite build`
- Miljövariabel `VITE_API_URL` pekar på Render-backenden
- URL: `https://shiftmate-ruby.vercel.app`

### Miljöhantering (Development vs Production)

| Konfiguration | Development | Production |
|---------------|-------------|------------|
| Backend-URL | `https://localhost:7215` | `https://shiftmate-vow0.onrender.com` |
| Frontend-URL | `http://localhost:5173` | `https://shiftmate-ruby.vercel.app` |
| Databas | Supabase dev-instans | Supabase prod-instans |
| Hemligheter | .NET User Secrets | Render miljövariabler |
| Swagger | Aktiverat | Avaktiverat |
| E-post avsändare | `onboarding@resend.dev` | `onboarding@resend.dev` |
| CORS | localhost:5173 | shiftmate-ruby.vercel.app |

---

## Utvecklingsflöde

### Starta lokalt

```bash
# Backend (från ShiftMate.Api/)
dotnet run

# Frontend (från shiftmate-frontend/)
npm install
npm run dev
```

Backend startar på `https://localhost:7215` med Swagger UI tillgängligt.
Frontend startar på `http://localhost:5173` och pratar med backend via `.env.development`.

### Git-workflow

Alla features utvecklas på **egna branches** och mergas via **Pull Requests**:

```
main (produktion)
  ├── feature/notification-system
  ├── feature/email-design-improvements
  ├── fix/swap-validation
  └── refactor/code-cleanup
```

- Branch-namngivning: `feature/<namn>`, `fix/<namn>`, `refactor/<namn>`
- Commits på svenska/engelska med beskrivande meddelanden
- PR-flöde: branch → develop → review → merge till main

### Testdata (Seed)

Vid uppstart seedar `DbInitializer.cs` testdata automatiskt:
- 4 testanvändare (André, Erik, Sara, Mahmoud) med BCrypt-hashade lösenord
- 13+ arbetspass spridda över 5 dagar med varierande bytesstatusar

---

## Tekniska utmaningar jag löst

### 1. Gmail SMTP blockeras i molnet

**Problem:** Gmail SMTP fungerade perfekt lokalt men blockerades på Render eftersom Gmail inte tillåter SMTP-anslutningar från cloud-IP:er.

**Lösning:** Migrerade hela e-postsystemet till **Resend HTTP API**. Implementerade `ResendEmailService` med `HttpClient` och Bearer token-autentisering. Bytte registreringen i DI-containern från `SmtpEmailService` till `ResendEmailService` med `AddHttpClient<>()`.

### 2. Passkrockar vid direktbyten

**Problem:** Vid direktbyten mellan kollegor (t.ex. onsdag mot onsdag) blockerades bytet felaktigt av overlap-kontrollen. Systemet räknade in passet som personen ger bort som en krock.

**Lösning:** La till exkludering av bytespasset i overlap-queryn (`s.Id != originalShift.Id` för requestor, `s.Id != targetShift.Id` för acceptor). Skrev sedan 6 nya tester som verifierar att logiken fungerar korrekt i alla edge cases.

### 3. UTC-krav i Npgsql 8

**Problem:** Npgsql 8 kräver `DateTimeKind.Utc` för alla queries mot `timestamptz`-kolumner. Frontend skickar `DateTimeKind.Unspecified` från `datetime-local`-inputs. Öppna pass (utan overlap-query) fungerade, men tilldelade pass kraschade.

**Lösning:** Normaliserar DateTime till UTC med `DateTime.SpecifyKind()` i början av varje handler, innan några databasoperationer körs.

### 4. Centraliserad e-postdesign

**Problem:** E-post-HTML var duplicerad i 5 olika handlers med inkonsekvent design.

**Lösning:** Skapade en statisk `EmailTemplateService` i Application-lagret med 5 publika metoder och 4 privata hjälpmetoder. Tabellbaserad HTML-layout kompatibel med Outlook/Gmail. Dynamisk `FrontendUrl` som sätts i `Program.cs` (localhost i dev, Vercel i prod). Resulterade i en nettoreduktion av 80 rader kod utan att behöva ändra konstruktorer eller tester.

---

## AI-verktyg i utvecklingen

Under projektets gång har jag använt AI som utvecklingsverktyg. Projektet startades med **Gemini CLI** och migrerades sedan till **Claude Code** för bättre utvecklarupplevelse. AI:n har använts som ett stöd för att:

- Resonera kring arkitektoniska beslut
- Felsöka komplexa problem (overlap-logik, UTC-buggar)
- Följa konsekventa mönster (CQRS, Clean Architecture)
- Refaktorera och städa kod

All kod har granskats, förståtts och godkänts av mig. AI:n är ett verktyg i verktygslådan - inte en ersättning för förståelse.

---

## Sammanfattning

ShiftMate visar min förmåga att som ensam utvecklare:

- **Designa och implementera** en fullständig applikation med Clean Architecture och CQRS
- **Hantera hela stacken** - från PostgreSQL-schema till React-komponenter
- **Deploya till produktion** med Docker, Render och Vercel
- **Skriva tester** med xUnit, Moq och FluentAssertions
- **Lösa riktiga tekniska problem** - inte bara följa tutorials
- **Hantera känslig konfiguration** med User Secrets och miljövariabler
- **Använda moderna verktyg** som AI-assistenter, Git-workflow med branches/PRs och CI/CD
- **Följa etablerade mönster** konsekvent genom hela kodbasen
