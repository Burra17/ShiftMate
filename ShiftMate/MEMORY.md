# SHIFTMATE - SESSION MEMORY

This file tracks what has been worked on across sessions.
Update this file at the end of each significant work session.

---

## CURRENT STATUS

- **Active Branch:** `feature/schedule-ui-improvements`
- **Last Updated:** 2026-02-11
- **Project State:** Stabil — schedule-redesign klar (ej mergad till main ännu)

---

## SESSION LOG

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
  - Ersätta `alert()`/`window.confirm()` med stilade toast-meddelanden
  - Profilredigering (backend-endpoint finns: PUT /api/users/profile)

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

---

## NOTES

- Test accounts are seeded via `DbInitializer.cs`
- Comments and explanations should be in Swedish
- Every new feature must be on a new branch (see CLAUDE.md git workflow)
