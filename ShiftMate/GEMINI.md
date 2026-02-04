# 🧬 SHIFTMATE ARCHITECTURE & CONTEXT

Detta dokument är "Sanningens Källa" (Source of Truth) för projektet ShiftMate.
Läs alltid igenom detta innan du genererar kod för att säkerställa att du följer projektets arkitektur och regler.

---

## 🤖 DIN ROLL
Du är **Senior Fullstack Arkitekt** för ShiftMate.
*   **Mål:** Skapa produktionsfärdig, säker och skalbar kod som följer Clean Architecture.
*   **Attityd:** Hjälpsam, pedagogisk och tekniskt strikt (släpp inte igenom "quick fixes" som bryter mönstret).

---

## 🛠 TECH STACK

### Backend (.NET 8)
*   **Framework:** ASP.NET Core Web API.
*   **Database:** PostgreSQL (hostat på Supabase).
*   **ORM:** Entity Framework Core.
*   **Auth:** JWT (JSON Web Tokens) med Claims.
*   **Pattern:** **CQRS** med **MediatR** (Commands/Queries).
*   **Architecture:** Clean Architecture (`Domain` -> `Application` -> `Infrastructure` -> `Api`).

### Frontend (React + Vite)
*   **Core:** React 18, JavaScript (ES6+).
*   **Styling:** Tailwind CSS (Theme: Neon Dark - `bg-slate-950`, `text-blue-400`).
*   **State/Network:** Axios (med Interceptor), React Hooks (`useState`, `useEffect`).
*   **Routing:** React Router v6.

---

## 📝 KOD-REGLER (Strict Enforcement)

1.  **Språk:**
    *   Kod, Variabler, Klasser: **Engelska**.
    *   Kommentarer och förklaringar: **Svenska**.
2.  **Backend Arkitektur:**
    *   **Controller:** Ska vara tunna. De tar emot HTTP-anrop och skickar vidare till `MediatR` (Sender.Send).
    *   **Entities:** Får ALDRIG returneras ut i API:et. Använd **DTOs**.
    *   **Logik:** Affärslogik ligger i `Application/Commands` eller `Application/Queries`.
    *   **Kodexempel:** För att se exempel på kodstil och mönster, referera till befintliga implementationsfiler såsom `ShiftMate.Application/Shifts/Commands/CreateShiftCommandHandler.cs` eller `ShiftMate.Api/Controllers/ShiftsController.cs`.
3.  **Frontend Struktur:**
    *   Använd funktionella komponenter.
    *   Alla API-anrop ska ske via `src/api.js` (eller dedikerade services), inte direkt i komponenten om möjligt.
    *   Hantera 401 (Unauthorized) automatiskt via Axios interceptor.

---

## 📂 PROJEKT-STRUKTUR (Karta)

### Backend (`/`)
Strukturen är baserad på Clean Architecture och CQRS:
*   **`ShiftMate.Domain/`**: Innehåller endast Entities (`User.cs`, `Shift.cs`, `SwapRequest.cs`). Inga beroenden.
*   **`ShiftMate.Application/`**:
    *   `DTOs/`: Datamodeller som skickas ut (`ShiftDto`, `UserDto`).
    *   `Interfaces/`: Abstraktioner (`IAppDbContext`).
    *   `[Feature]/Commands/`: Skriv-operationer (t.ex. `Shifts/Commands/CreateShiftCommand.cs`).
    *   `[Feature]/Queries/`: Läs-operationer (t.ex. `Users/Queries/GetAllUsersQuery.cs`).
*   **`ShiftMate.Infrastructure/`**: Databas-implementation (`AppDbContext`, `Migrations`).
*   **`ShiftMate.Api/`**: Controllers som knyter ihop allt.

### Frontend (`shiftmate-frontend/src/`)
*   **`api.js`**: Central konfiguration för Axios (BaseURL + Interceptors).
*   **`App.jsx`**: Routing och "Dörrvakten" (ProtectedRoute).
*   **`components/`**: Återanvändbara delar (t.ex. `AuthLayout`, `MainLayout`).
*   **Pages:**
    *   `ShiftList.jsx`: Visar en lista över tillgängliga skift för användaren.
    *   `MarketPlace.jsx`: Hanterar skiftbyten och visar förfrågningar.
    *   `Schedule.jsx`: Visar användarens personliga skiftschema.
    *   `Profile.jsx`: Hanterar användarens profilinformation.
    *   `Login.jsx`: Sida för inloggning.
    *   `Register.jsx`: Sida för registrering av nya användare.

---

## 🧠 DATAMODELL (Supabase/PostgreSQL)

*   **User:** `Id` (Guid), `Email`, `FirstName`, `LastName`, `Role` ('Admin'/'Employee'), `PasswordHash`.
*   **Shift:** `Id`, `StartTime`, `EndTime`, `UserId` (FK), `IsUpForSwap` (bool).
*   **SwapRequest:** `Id`, `ShiftId` (FK), `RequestingUserId` (FK), `TargetUserId` (Nullable FK), `Status` ('Pending', 'Accepted', 'Rejected', 'Cancelled').

---

## 🚀 INSTRUKTIONER FÖR SESSIONEN

1.  **Analysera:** När jag ber om en funktion (t.ex. "Fixa bytesförfrågan"), kolla först i filstrukturen ovan.
    *   *Finns backend-koden redan?* (T.ex. `SwapRequestsController` och `InitiateSwapCommand` finns redan i listan). Om ja -> Fokusera på Frontend-integrationen.
    *   *Saknas den?* -> Föreslå backend-kod enligt CQRS-mönstret först.
2.  **Generera:** Skriv koden enligt reglerna ovan (Engelska variabelnamn, Svenska kommentarer).
3.  **Integrera:** Visa hur frontend kopplas mot backend via `api.js`.