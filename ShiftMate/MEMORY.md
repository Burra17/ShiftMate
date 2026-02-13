# SHIFTMATE - SESSION MEMORY

This file tracks what has been worked on across sessions.
Update this file at the end of each significant work session.

---

## CURRENT STATUS

- **Active Branch:** `feature/email-design-improvements`
- **Last Updated:** 2026-02-13
- **Project State:** Stabil — Centraliserad email-templateservice med professionell design

---

## SESSION LOG

### 2026-02-13 - Email Design Improvements (feature/email-design-improvements)

- **What was done:**
  - **Centraliserad EmailTemplateService (ny fil):**
    - `ShiftMate.Application/Services/EmailTemplateService.cs` — Statisk klass som genererar all email-HTML
    - 5 publika metoder: `SwapProposal`, `DirectSwapAccepted`, `MarketplaceShiftTaken`, `SwapDeclined`, `ShiftAssigned`
    - 4 privata hjälpmetoder: `Layout` (bas-layout), `InfoBox` (färgad inforuta), `Button` (CTA-knapp), `ShiftRow` (pass-info)
    - Professionell, ren design: vitt kort på ljusgrå bakgrund, subtila ramar, inga emojis i layout
    - Email-safe HTML: tabellbaserad layout, inline styles, kompatibelt med Outlook/Gmail/etc.
  - **Dynamisk FrontendUrl (dev vs produktion):**
    - `EmailTemplateService.FrontendUrl` — Statisk property som sätts en gång i `Program.cs`
    - `appsettings.json` → `https://shiftmate-ruby.vercel.app` (produktion)
    - `appsettings.Development.json` → `http://localhost:5173` (utveckling)
    - Knappar i emails pekar på rätt sida: `/mine`, `/schedule`, `/marketplace`
  - **Alla 5 handlers uppdaterade:**
    - Inline HTML ersatt med `EmailTemplateService`-anrop
    - Inga konstruktor-ändringar — noll test-modifieringar behövdes
    - Nettoreduktion: -80 rader kod (69 tillagda, 149 borttagna)
  - **Nya filer (1):**
    - `ShiftMate.Application/Services/EmailTemplateService.cs`
  - **Modifierade filer (8):**
    - `ShiftMate.Api/Program.cs` — Sätter `FrontendUrl` från config
    - `ShiftMate.Api/appsettings.json` — Avkommenterad `FrontendUrl` (produktion)
    - `ShiftMate.Api/appsettings.Development.json` — `FrontendUrl` override (localhost)
    - `ShiftMate.Application/SwapRequests/Commands/ProposeDirectSwapCommand.cs` — Använder `EmailTemplateService.SwapProposal()`
    - `ShiftMate.Application/SwapRequests/Commands/AcceptSwapCommand.cs` — Använder `DirectSwapAccepted()` + `MarketplaceShiftTaken()`
    - `ShiftMate.Application/SwapRequests/Commands/DeclineSwapRequestCommand.cs` — Använder `SwapDeclined()`
    - `ShiftMate.Application/Shifts/Commands/TakeShiftCommandHandler.cs` — Använder `MarketplaceShiftTaken()`
    - `ShiftMate.Application/Shifts/Commands/CreateShiftCommand.cs` — Använder `ShiftAssigned()`
  - **Build OK** — dotnet build + dotnet test (13/13 gröna), noll test-ändringar

- **Nästa steg (planerade):**
  - In-app notification system (badge counts, notification dropdown)
  - Överväg egen domän för professionella emails (t.ex. noreply@shiftmate.se)
  - Status magic strings ("Pending", "Accepted") → enum + migration

### 2026-02-12 - Email Notification System (feature/email-notifications → merged to main)

- **What was done:**
  - **Migrering från Gmail SMTP till Resend HTTP API:**
    - Problem identifierat: Gmail SMTP fungerar lokalt men blockeras på Render (cloud IPs)
    - `ResendEmailService.cs` — Ny service med HttpClient för Resend API (ersätter SmtpEmailService)
    - Använder `https://api.resend.com/emails` med Bearer token authentication
    - Fire-and-forget pattern med `Task.Run()` för att inte blocka requests
  - **Konfiguration:**
    - `appsettings.json` — Bytt från EmailSettings (SMTP) till Resend (API key, FromEmail, FromName)
    - `Program.cs` — `AddHttpClient<IEmailService, ResendEmailService>()` istället för SmtpEmailService
    - Default `FromEmail`: "onboarding@resend.dev" (Resends officiella test-email, fungerar i production)
  - **Email-notiser på 4 strategiska platser:**
    - `AcceptSwapCommand.cs` — Notifierar requestor när swap godkänns (både direktbyte och marketplace)
    - `DeclineSwapRequestCommand.cs` — Notifierar requestor när swap nekas
    - `TakeShiftCommandHandler.cs` — Notifierar originalägare när pass tas från marketplace
    - `CreateShiftCommand.cs` — Notifierar användare när admin tilldelar nytt pass
  - **Email-innehåll:**
    - HTML-formaterade emails med svensk formatering (datum/tid via CultureInfo "sv-SE")
    - Färgkodade headers (grönt för godkänt, rött för nekat, blått för nytt pass)
    - Tydlig information om vad som hände (vilka pass, vilka tider, vem som gjorde vad)
  - **Testsvit uppdaterad:**
    - Mockar för `IEmailService` och `ILogger<T>` tillagda i alla handlers
    - `AcceptSwapHandlerTests.cs` — 7 tester, alla gröna
    - `CreateShiftHandlerTests.cs` — 4 tester, alla gröna
    - Alla 13 tester klarar utan fel
  - **Deployment:**
    - Render environment variable: `RESEND__APIKEY` (dubbel underscore för nested config)
    - Testat lokalt — email skickas vid swap requests och pass-tilldelningar ✅
  - **Nya filer (1):**
    - `ShiftMate.Infrastructure/Services/ResendEmailService.cs`
  - **Modifierade filer (7):**
    - `ShiftMate.Api/Program.cs` — ResendEmailService registrering
    - `ShiftMate.Api/appsettings.json` — Resend config
    - `ShiftMate.Application/SwapRequests/Commands/AcceptSwapCommand.cs` — Email-notis + logging
    - `ShiftMate.Application/SwapRequests/Commands/DeclineSwapRequestCommand.cs` — Email-notis + logging
    - `ShiftMate.Application/Shifts/Commands/TakeShiftCommandHandler.cs` — Email-notis + logging
    - `ShiftMate.Application/Shifts/Commands/CreateShiftCommand.cs` — Email-notis + logging
    - `ShiftMate.Tests/*` — Mockar tillagda
  - **Build OK** — dotnet build + dotnet test (13/13 gröna)

- **Nästa steg (planerade):**
  - ~~Snygga till email-designen (logo, bättre styling, responsiv design)~~ ✅ (löst i feature/email-design-improvements)
  - In-app notification system (badge counts, notification dropdown)
  - Överväg egen domän för professionella emails (t.ex. noreply@shiftmate.se)

### 2026-02-12 - Profile Page Improvements (feature/profile-page-improvements → merged to main)

- **What was done:**
  - **Backend — Byt lösenord (ny funktionalitet):**
    - `ChangePasswordCommand.cs` — CQRS command + handler: verifierar nuvarande lösenord med BCrypt, hashar nya, sparar
    - `ChangePasswordCommandValidator.cs` — FluentValidation: CurrentPassword NotEmpty, NewPassword NotEmpty + MinimumLength(8)
    - `UsersController.cs` — Ny endpoint `PUT /api/Users/change-password` (samma mönster som UpdateProfile)
  - **Frontend — Månadsstatistik:**
    - `fetchStats` beräknar nu pass/timmar denna månad + totaler (4 kort i 2x2 grid)
    - Progress-bar under "Timmar denna månad" som visar nuvarande månad vs genomsnittliga timmar/månad
  - **Frontend — Lösenordsbyte:**
    - Ny `changePassword()` funktion i `api.js`
    - Formulär med tre fält (nuvarande, nytt, bekräfta) + frontend-validering + toast-feedback
  - **UI-polish (konsistent med resten av appen):**
    - Glödande vänster-accentbarer (`w-1` + `shadow-[0_0_15px]`) på alla stats-kort (rosa/lila/blå/indigo)
    - Svag färgtonad bakgrund per kort (`bg-pink-500/5`, `bg-blue-500/5`, etc.)
    - "Statistik"-sektionsrubrik (`text-xl font-black uppercase`)
    - Emoji-ikoner på alla knappar (✏️ Redigera, 🔒 Byt lösenord, 🚪 Logga ut)
    - Rollbadge under användarnamnet (Admin=röd, Chef=amber, Anställd=blå)
    - Glödande accentbar på lösenordsformuläret
  - **Nya filer (2):**
    - `ShiftMate.Application/Users/Commands/ChangePasswordCommand.cs`
    - `ShiftMate.Application/Users/Commands/ChangePasswordCommandValidator.cs`
  - **Modifierade filer (3):**
    - `ShiftMate.Api/Controllers/UsersController.cs` — ny endpoint
    - `shiftmate-frontend/src/api.js` — ny `changePassword()` funktion
    - `shiftmate-frontend/src/Profile.jsx` — komplett omskrivning med alla förbättringar
  - **Build OK** — dotnet build + vite build utan fel

- **Idéer diskuterade men ej implementerade:**
  - Profilbild-uppladdning (kräver fillagring, ny User-kolumn + migration)

### 2026-02-11 - Schedule Redesign (feature/schedule-ui-improvements)

- **What was done:**
  - **Komplett omskrivning av Schema-sidan** med tre vylägen:
    - **Dag (DayView):** Detaljerad lista med fullstora ShiftCards (avatar, namn, tid, varaktighet, bytesstatus)
    - **Vecka (WeekView):** 7-kolumnsrutnät mån–sön på desktop, staplat på mobil. Standardvy.
    - **Månad (MonthView):** 42-cells kalenderrutnät med färgade passprickar. Klick på dag → öppnar dagsvy.
  - **Navigation:** Föregående/Nästa-pilar, "Idag"-knapp, periodlabel som anpassas per vy
  - **Egna pass markerade:** Blå accentkant + tonad bakgrund i alla vyer
  - **Idag-indikator:** Ring-highlight i månadsvy, blå prick + text i veckovy
  - **Nya filer (6):**
    - `src/components/schedule/ShiftCard.jsx` — Återanvändbart passkort (compact/full + ägar-markering)
    - `src/components/schedule/ViewToggle.jsx` — Dag/Vecka/Månad segmenterad kontroll
    - `src/components/schedule/NavigationBar.jsx` — Navigering + periodlabel
    - `src/components/schedule/DayView.jsx` — Detaljerad dagslista
    - `src/components/schedule/WeekView.jsx` — Responsivt veckorutnät
    - `src/components/schedule/MonthView.jsx` — Kalenderrutnät med passprickar + förklaring
  - **Modifierade filer (3):**
    - `src/utils/dateUtils.js` — 11 nya kalenderhjälpfunktioner (getMonday, getWeekNumber, getCalendarDays, isSameDay, isToday, formatMonthYear, formatWeekLabel, formatDayLabel, getShortWeekday, addDays, addMonths)
    - `src/api.js` — Ny `getCurrentUserId()` hjälpfunktion (JWT nameidentifier claim)
    - `src/Schedule.jsx` — Omskriven till orkestrerare med viewMode/currentDate state
  - **Inga nya npm-beroenden** — enbart native Date + egna utility-funktioner
  - **Build OK** — vite build kompilerar utan fel

### 2026-02-11 - Frontend UX (feature/frontend-ux-improvements → merged to main)

- **What was done:**
  - **AdminPanel redesign:**
    - Ersatte två `datetime-local`-inputs med separat datumväljare + tidsinputs
    - Snabbvalsknappar: Öppning (05.45-13), Örjan (06.13-15), Dagpass (11-20), Kvällspass (14-22.15)
    - Beräknad passlängd visas som förhandsgranskning
    - Formuläret återställs automatiskt efter skapning
    - Meddelanden auto-försvinner efter 4 sekunder
    - Hanterar nattpass som passerar midnatt korrekt
  - **Mobil navigation:**
    - Fast bottenmeny med ikoner + etiketter, synlig bara på mobil (`md:hidden`)
    - Samma menyalternativ som sidomenyn inkl. Admin för admin-användare
    - Aktiv flik-indikator med glow-effekt
  - **Svenskifiering:**
    - `<html lang="en">` → `<html lang="sv">` (förhindrar Chrome auto-translate från att förstöra svensk text)
    - Sidtitel → "ShiftMate"
    - "Admin Panel" → "Admin" i navigationen

### 2026-02-11 - Bugfixes & Tests (fix/post-cleanup-bugfixes → merged to main)

- **What was done:**
  - **Fix 1 - Testsvit (4→13 tester, 0 failing):**
    - `CreateShiftHandlerTests`: Seedade User-entitet i InMemory DB (handlern kontrollerar user-existens sedan krock-kontrollen lades till)
    - `AcceptSwapHandlerTests`: Seedade User-entiteter för alla FK-relationer (Npgsql Include() kräver att refererade entiteter finns)
    - Nya tester: öppet pass utan UserId, user-not-found, passkrock vid skapning, lyckat öppet byte, byte-ej-hittat, redan-accepterat byte, direktbyte samma dag, direktbyte med överlappande tider, avvisat byte vid tredje-pass-krock
  - **Fix 2 - Direktbyte overlap-logik (`AcceptSwapCommand.cs`):**
    - Bugg: Vid direktbyte (t.ex. onsdag-mot-onsdag) blockerades bytet felaktigt med "passkrock" eftersom overlap-checken räknade in passet som personen ger bort
    - Fix: Lade till `s.Id != originalShift.Id` i requestor-checken och `s.Id != targetShift.Id` i acceptor-checken så att båda bytespassen exkluderas
  - **Fix 3 - Admin kan inte skapa pass med tilldelad användare (`CreateShiftCommand.cs`):**
    - Bugg: Npgsql 8 kräver `DateTimeKind.Utc` för queries mot `timestamptz`-kolumner. Frontend skickar `DateTimeKind.Unspecified` från `datetime-local`. Öppna pass fungerade (skippar overlap-query), tilldelade pass kraschade
    - Fix: Normaliserar DateTime till UTC via `SpecifyKind` i början av handlern, före alla DB-queries

- **Kända problem:**
  - Swap accept/decline-regression från cleanup behöver fortfarande felsökas i browser (frontend-sidan)

### 2026-02-11 - Code Cleanup (refactor/code-cleanup → merged to main)

- **What was done:**
  - **Group 1 - Dead Code Removal:**
    - Tömde `App.css` (oanvänd Vite-template CSS)
    - Tog bort oanvänd `fetchShifts`-import i `MarketPlace.jsx`
    - Tog bort redundanta `using`-satser i `TakeShiftCommandHandler.cs` (täcks av .NET 8 implicit usings)
  - **Group 2 - Kommentarer & Strängar:**
    - Fixade encoding-korruption (`√∂` → `ö`) i `IEmailService.cs` och `CreateShiftCommandValidatorTests.cs`
    - Översatte valideringsmeddelanden i `RegisterUserCommandValidator.cs` till svenska
    - Översatte loggmeddelande i `SmtpEmailService.cs` och `api.js` till svenska
  - **Group 3 - DRY Frontend (dateUtils):**
    - Skapade `src/utils/dateUtils.js` med `formatDate()`, `formatTime()`, `formatTimeRange()`
    - Uppdaterade `ShiftList.jsx`, `MarketPlace.jsx`, `Schedule.jsx` att använda delade utils
  - **Group 4 - DRY Backend (JWT Extension):**
    - Skapade `ShiftMate.Api/Extensions/ClaimsPrincipalExtensions.cs` med `GetUserId()`
    - Uppdaterade alla tre controllers att använda extension-metoden
  - **Group 5 - Frontend API-centralisering:**
    - Lade till 8 centraliserade funktioner i `api.js` (fetchMyShifts, fetchClaimableShifts, takeShift, cancelShiftSwap, initiateSwap, fetchReceivedSwapRequests, acceptSwapRequest, declineSwapRequest)
    - Lade till delad `decodeToken()` hjälpfunktion, refaktorerade `getUserRole()` att använda den
    - Uppdaterade `ShiftList.jsx`, `MarketPlace.jsx`, `Schedule.jsx`, `Profile.jsx`
  - **Group 6 - Performance:**
    - Lade till `.AsNoTracking()` på alla 6 read-only query handlers

- **Kända problem (att felsöka nästa session):**
  - Swap-logiken (godkänn/neka bytesförfrågan) slutade fungera efter cleanup
  - Troligen relaterat till Group 5 (API-centralisering i ShiftList.jsx) eller Group 4 (controller-refaktorering)
  - Koden ser korrekt ut vid granskning — behöver köras med browser devtools för att se exakt felmeddelande
  - 2 pre-existing testfel finns (CreateShiftHandlerTests, AcceptSwapHandlerTests) — ej relaterade till cleanup

### 2026-02-11 - Migration to Claude Code

- **What was done:**
  - Migrated from Gemini CLI to Claude Code
  - Created `CLAUDE.md` as the new Source of Truth (replacing `GEMINI.md`)
  - Created `MEMORY.md` for cross-session context tracking
  - Full project review and onboarding completed

- **Current state of the project:**
  - Backend: .NET 8 API with Clean Architecture + CQRS fully operational
  - Frontend: React 19 + Vite + Tailwind (neon dark theme) fully operational
  - Database: PostgreSQL on Supabase with 4 migrations applied
  - Deployment: Backend on Render, Frontend on Vercel
  - Auth: JWT-based with role support (Admin/Employee/Manager)

- **Implemented features:**
  - User registration and login (JWT)
  - Shift creation (user + admin)
  - Personal shift list ("Mina Pass")
  - Marketplace for available shifts ("Lediga Pass")
  - Full schedule view ("Schema")
  - Open marketplace swaps
  - Direct swap proposals between colleagues
  - Accept/decline/cancel swap requests
  - Admin panel with shift creation and user assignment
  - Role-based navigation (admin panel only for admins)
  - Profile page with stats
  - Seed data with 4 test users

- **Known areas for future work:**
  - Felsöka swap accept/decline i frontend (browser devtools)
  - Status magic strings ("Pending", "Accepted") → enum + migration
  - Error response format-konsistens
  - Profilbild-uppladdning (fillagring + ny User-kolumn + migration)
  - ~~Ersätta `alert()`/`window.confirm()` med stilade toast-meddelanden~~ ✅ (löst i toast-modal-system)
  - ~~Profilredigering~~ ✅ (löst: redigera profil + byt lösenord)

---

## DECISIONS LOG

Track important architectural or design decisions here.

| Date | Decision | Reason |
|------|----------|--------|
| 2026-02-11 | Switched from Gemini CLI to Claude Code | Better developer experience |
| 2026-02-11 | Created CLAUDE.md + MEMORY.md | Consistent context across sessions |
| 2026-02-11 | Skapade `utils/dateUtils.js` | DRY — duplicerad datumformatering i 3 komponenter |
| 2026-02-11 | Skapade `ClaimsPrincipalExtensions.cs` | DRY — JWT-userId-parsning duplicerad i 3 controllers |
| 2026-02-11 | Centraliserade API-anrop i `api.js` | DRY — direkta axios-anrop i komponenter → delade funktioner |
| 2026-02-11 | `.AsNoTracking()` på alla read-only queries | Prestandaoptimering |
| 2026-02-11 | UTC-normalisering tidigt i handlers | Npgsql 8 kräver `DateTimeKind.Utc` för `timestamptz`-queries |
| 2026-02-11 | Exkludera båda bytespass i overlap-check | Direktbyten blockerades felaktigt vid överlapp |
| 2026-02-11 | AdminPanel: datum + tid separat istället för datetime-local | Enklare UX, behöver bara välja datum en gång |
| 2026-02-11 | Mobil bottenmeny | Sidebar var `hidden md:flex` utan mobilalternativ |
| 2026-02-11 | `<html lang="sv">` | Förhindrar Chrome auto-translate från att förstöra svensk text |
| 2026-02-11 | Schema: Dag/Vecka/Månad-vyer med lokal state | Ingen URL-params — konsistent med resten av appen |
| 2026-02-11 | Vecka startar måndag (ISO 8601) | Svensk standard |
| 2026-02-11 | 42-cells månadsrutnät (6 rader) | Konsekvent höjd oavsett månad |
| 2026-02-11 | Nattpass visas på startdagsdatum | Enklast och mest intuitiva tolkningent |
| 2026-02-11 | `getCurrentUserId()` via JWT claim | Samma mönster som `getUserRole()` |
| 2026-02-12 | Månadsstatistik beräknas i frontend | Alla shifts hämtas redan — ingen ny backend-endpoint behövs |
| 2026-02-12 | Progress-bar: nuvarande månad vs genomsnitt | Ger kontext till månadstimmar utan att behöva ett hårdkodat mål |
| 2026-02-12 | Lösenordsbyte via CQRS command | Följer exakt samma mönster som UpdateProfileCommand |
| 2026-02-12 | Resend HTTP API istället för Gmail SMTP | Gmail blockerar cloud IPs (Render), Resend är byggt för transaktional email |
| 2026-02-12 | Fire-and-forget email med Task.Run() | Email-fel ska inte krascha requests — logga fel men fortsätt |
| 2026-02-12 | onboarding@resend.dev som default FromEmail | Resends officiella test-email, fungerar i production, gratis |
| 2026-02-12 | Email-notiser på 4 platser (accept/decline/take/assign) | Maximera användarnytta — meddela vid alla kritiska events |
| 2026-02-12 | HTML-formaterade emails med svensk CultureInfo | Professionellt utseende + svenskt datum/tidsformat |
| 2026-02-13 | Statisk `EmailTemplateService` i Application-lagret | Centralisera email-HTML, undvika DI-ändringar och testbrott |
| 2026-02-13 | `FrontendUrl` via appsettings-lagring | Dev/prod-URL utan hårdkodning, noll kodändringar vid deploy |

---

## NOTES

- Test accounts are seeded via `DbInitializer.cs`
- Comments and explanations should be in Swedish
- Every new feature must be on a new branch (see CLAUDE.md git workflow)
