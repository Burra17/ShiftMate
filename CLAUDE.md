# SHIFTMATE - CLAUDE CODE CONTEXT

This is the **Source of Truth** for the ShiftMate project.
Always read this before generating code to ensure you follow the project's architecture and rules.

---

## YOUR ROLE

You are a **Senior Fullstack Architect** for ShiftMate.

- **Goal:** Write production-ready, secure, and scalable code following Clean Architecture.
- **Attitude:** Helpful, pedagogical, and technically strict — never allow "quick fixes" that break established patterns.

---

## DESIGN PRINCIPLES

These principles guide every decision in this project:

- **KISS** (Keep It Simple, Stupid) — Choose the simplest solution that works. No clever tricks.
- **DRY** (Don't Repeat Yourself) — Extract shared logic only when it's used in 3+ places. Three similar lines > premature abstraction.
- **SRP** (Single Responsibility Principle) — Each class/function does one thing well. Controllers delegate, handlers handle, DTOs carry data.
- **YAGNI** (You Aren't Gonna Need It) — Don't build for hypothetical future requirements. Implement what's needed now.
- **Separation of Concerns** — Domain has no dependencies. Application owns business logic. Infrastructure handles data access. API handles HTTP.
- **Fail Fast** — Validate early, throw on invalid state. Don't silently swallow errors.

---

## TECH STACK

### Backend (.NET 8)

- **Framework:** ASP.NET Core Web API
- **Database:** PostgreSQL (hosted on Supabase)
- **ORM:** Entity Framework Core 8.0.23
- **Auth:** JWT (JSON Web Tokens) with Claims
- **Pattern:** CQRS with MediatR 12.4.1
- **Validation:** FluentValidation 12.1.1
- **Password Hashing:** BCrypt.Net-Next 4.0.3
- **Email:** Resend HTTP API via IEmailService
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → Api)

### Frontend (React 19 + Vite)

- **Core:** React 19, JavaScript (ES6+), functional components only
- **Build:** Vite 7.2.4
- **Styling:** Tailwind CSS 4.1.18 (Theme: Neon Dark — `bg-slate-950`, `text-blue-400`)
- **State/Network:** Axios with JWT interceptor, React Hooks (`useState`, `useEffect`)
- **Routing:** React Router v7
- **Notifications:** Toast system via `ToastContext` (success, error, info, warning)

### Deployment

- **Backend:** Render (health check at `/health`)
- **Frontend:** Vercel
- **Containerization:** Docker (multi-stage build)

---

## CODE RULES (Strict Enforcement)

### Language Convention

- Code, variables, classes, commit messages: **English**
- Comments, validation messages, UI text: **Swedish**

### Backend Rules

1. **Controllers** must be thin — receive HTTP request, extract user from JWT claims, delegate to MediatR (`_mediator.Send()`), catch exceptions, return HTTP response. No business logic.
2. **Entities** must NEVER be returned from the API. Always map to **DTOs**.
3. **Business logic** lives exclusively in `Application/Commands` or `Application/Queries` handlers.
4. **New commands/queries** must follow the existing CQRS pattern with `IRequestHandler<TCommand, TResult>`.
5. **Validation** uses FluentValidation in the Application layer. Every command that accepts user input SHOULD have a validator.
6. **Reference examples:** Look at existing files like `CreateShiftCommand.cs` or `ShiftsController.cs` for style and patterns.

### Error Handling Pattern

Handlers throw exceptions, controllers catch and convert to HTTP responses:

```csharp
// In handlers — throw on invalid state
throw new ValidationException(validationResult.Errors);   // Input validation
throw new InvalidOperationException("Message in Swedish"); // Business rule violation
throw new Exception("Message in Swedish");                 // General errors

// In controllers — consistent try/catch pattern
try {
    var result = await _mediator.Send(command);
    return Ok(new { Message = "Framgångsmeddelande", result });
} catch (ValidationException vex) {
    return BadRequest(new { Error = true, Message = "Valideringsfel: " + vex.Message, Details = vex.Errors });
} catch (Exception ex) {
    return BadRequest(new { Error = true, Message = $"Ett fel uppstod: {ex.Message}" });
}
```

### API Response Format

All responses use consistent JSON structure:

```
Success:  { "Message": "...", "Id": "..." }  or  [array of DTOs]
Error:    { "Error": true, "Message": "..." }
Login:    { "Token": "jwt.string" }
```

- Use **PascalCase** for JSON property names (C# default).
- Success messages and error messages in **Swedish**.

### DTO Mapping Rules

- Map entities to DTOs **inline in handlers** using `.Select()` or manual construction.
- No AutoMapper or separate mapping classes — keep it explicit and simple (KISS).
- **Never expose sensitive fields** (e.g., `PasswordHash`) in DTOs.
- Nest related DTOs when needed (e.g., `ShiftDto` contains `UserDto` for the assigned user).

### Authorization Pattern

```csharp
// Controller level — use [Authorize] attributes
[Authorize]                      // Requires any authenticated user
[Authorize(Roles = "Manager")]   // Requires Manager role

// Extract user ID from JWT claims using extension method
var userId = User.GetUserId();   // ClaimsPrincipalExtensions.cs
if (userId == null) return Unauthorized();
```

### Frontend Rules

1. Use **functional components** only. No class components.
2. All API calls go through `src/api.js` (centralized Axios instance with helper functions). Never call Axios directly in components.
3. Handle 401 (Unauthorized) automatically via the Axios interceptor (auto-logout).
4. Follow the existing neon dark theme (`bg-slate-950`, `text-blue-400`, `border-blue-500/30`).
5. Time formatting uses Swedish locale (`sv-SE`).
6. Use `useToast()` and `useConfirm()` from `ToastContext` for user feedback — never `alert()` or `window.confirm()`.
7. Use `Promise.all()` when loading multiple independent data sources in `useEffect`.
8. Loading states: global `loading` for initial data fetch, `actionLoading` (with ID) for per-item button actions.
9. Role checks via `getUserRole()` from `api.js` — conditionally render Manager-only UI.

### Frontend State Management

- **Local state** (`useState` + `useEffect`) for component data. No Redux/Zustand.
- **Context** only for cross-cutting concerns (currently: `ToastContext` for notifications).
- **Props** for parent-to-child communication. Lift state up when siblings need shared data.
- **localStorage** for JWT token persistence. Access via `api.js` helper functions.

---

## NAMING CONVENTIONS

### Backend

| What | Convention | Example |
|------|-----------|---------|
| Command | `[Verb][Entity]Command.cs` | `CreateShiftCommand.cs` |
| Command handler | Inside same file as command | Class: `CreateShiftCommandHandler` |
| Command validator | Separate file: `[Command]Validator.cs` | `CreateShiftCommandValidator.cs` |
| Query | `[Get][What]Query.cs` | `GetAllShiftsQuery.cs` |
| Query handler | Separate file: `[Get][What]Handler.cs` | `GetAllShiftsHandler.cs` |
| DTO | `[Entity]Dto.cs` | `ShiftDto.cs` |
| Controller | `[Entity plural]Controller.cs` | `ShiftsController.cs` |
| Interface | `I[Name].cs` | `IAppDbContext.cs` |

### Frontend

| What | Convention | Example |
|------|-----------|---------|
| Page component | `PascalCase.jsx` | `Dashboard.jsx` |
| Shared component | `components/PascalCase.jsx` | `components/ManagerPanel.jsx` |
| UI component | `components/ui/PascalCase.jsx` | `components/ui/ConfirmModal.jsx` |
| Context | `contexts/[Name]Context.jsx` | `contexts/ToastContext.jsx` |
| API helper | `camelCase` function in `api.js` | `fetchMyShifts()` |

### Test Files

| What | Convention | Example |
|------|-----------|---------|
| Test file | `[HandlerName]Tests.cs` | `CreateShiftHandlerTests.cs` |
| Test method | `[Method]_Should_[Behavior]_When_[Condition]` | `Handle_Should_Throw_When_User_Not_Found` |
| Structure | Arrange → Act → Assert (AAA) | Always follow this order |
| Tools | xUnit `[Fact]`, FluentAssertions, Moq | `result.Should().NotBeEmpty()` |

---

## GIT WORKFLOW

### Branching Strategy

- **Main branch:** `main` (production-ready code)
- **Every change MUST be developed in a new branch** before merging to main.
- **Branch naming:** `feature/<name>`, `fix/<name>`, `refactor/<name>`
- **Examples:** `feature/password-reset`, `fix/swap-validation`, `refactor/shift-components`
- Always create a PR to merge back into `main`.
- Keep commits focused and descriptive, in **English**.

---

## PROJECT STRUCTURE

### Backend

```
ShiftMate.Domain/               # Entities + enums. No dependencies.
  User.cs, Shift.cs, SwapRequest.cs, SwapRequestStatus.cs

ShiftMate.Application/          # Business logic layer (CQRS)
  DTOs/                         # ShiftDto, UserDto, SwapRequestDto
  Interfaces/                   # IAppDbContext, IEmailService
  [Feature]/Commands/           # Write operations (command + handler in same file)
  [Feature]/Queries/            # Read operations (query + handler in separate files)
  DependencyInjection.cs        # MediatR + FluentValidation registration

ShiftMate.Infrastructure/       # Data access & external services
  AppDbContext.cs               # EF Core DbContext with relationships
  DbInitializer.cs              # Seed data (test users & shifts)
  Services/                     # ResendEmailService
  Migrations/                   # EF Core migrations

ShiftMate.Api/                  # HTTP layer
  Program.cs                    # DI configuration & middleware pipeline
  Controllers/                  # Thin controllers (Users, Shifts, SwapRequests)

ShiftMate.Tests/                # Unit tests (xUnit + FluentAssertions + Moq)
  Support/TestDbContextFactory.cs
```

### Frontend (`shiftmate-frontend/src/`)

```
main.jsx                        # React entrypoint
App.jsx                         # Main routing, auth check, sidebar navigation
api.js                          # Axios config + JWT interceptor + API helper functions
index.css                       # Global Tailwind styles + animations
Dashboard.jsx                   # Landing page with overview & quick stats
Login.jsx                       # Login page
Register.jsx                    # Registration page
ShiftList.jsx                   # "Mina Pass" — user's shifts + incoming swap requests
MarketPlace.jsx                 # "Lediga Pass" — claimable shifts
Schedule.jsx                    # "Schema" — full schedule grouped by date
Profile.jsx                     # User profile & stats
contexts/
  ToastContext.jsx              # Toast notifications + confirm dialog (useToast, useConfirm)
components/
  AuthLayout.jsx                # Shared auth page layout (login/register)
  ManagerPanel.jsx              # Manager: shift CRUD, user management
  ui/
    ToastContainer.jsx          # Toast notification list (portal-rendered)
    ConfirmModal.jsx            # Confirmation dialog (portal-rendered)
```

---

## DATA MODEL (PostgreSQL)

- **User:** `Id` (Guid), `Email` (unique, case-insensitive), `FirstName`, `LastName`, `Role` (Employee/Manager), `PasswordHash`
- **Shift:** `Id`, `StartTime`, `EndTime`, `UserId` (nullable FK → User), `IsUpForSwap` (bool)
- **SwapRequest:** `Id`, `ShiftId` (FK), `RequestingUserId` (FK), `TargetUserId` (nullable FK), `TargetShiftId` (nullable FK), `Status` (SwapRequestStatus enum: Pending/Accepted/Declined/Cancelled, stored as string), `CreatedAt`

---

## API ENDPOINTS

### Public (No Auth)
- `POST /api/users/login` — Login → returns JWT token
- `POST /api/users/register` — Register → returns UserDto
- `POST /api/users/forgot-password` — Request password reset email
- `POST /api/users/reset-password` — Reset password with token
- `GET /health` — Health check

### Authenticated (Any Role)
- `GET /api/users` — Get all users
- `PUT /api/users/profile` — Update own profile
- `GET /api/shifts` — All shifts (?onlyWithUsers=true)
- `GET /api/shifts/mine` — Current user's shifts
- `GET /api/shifts/claimable` — Available unassigned shifts
- `POST /api/shifts` — Create own shift
- `PUT /api/shifts/{id}/take` — Claim a shift
- `PUT /api/shifts/{id}/cancel-swap` — Cancel swap offering
- `POST /api/swaprequests/initiate` — Offer shift to marketplace
- `POST /api/swaprequests/propose-direct` — Direct swap proposal
- `GET /api/swaprequests/available` — Available swaps
- `GET /api/swaprequests/received` — Incoming requests
- `GET /api/swaprequests/sent` — Sent (outgoing) requests
- `POST /api/swaprequests/accept` — Accept swap
- `POST /api/swaprequests/{id}/decline` — Decline swap
- `DELETE /api/swaprequests/{id}` — Cancel request

### Manager Only
- `POST /api/shifts/admin` — Create shift and assign to user
- `PUT /api/shifts/{id}` — Update any shift
- `DELETE /api/shifts/{id}` — Delete any shift

---

## ENVIRONMENT

### Development
- Backend: `https://localhost:7215`
- Frontend: `http://localhost:5173`
- Frontend env: `.env.development` → `VITE_API_BASE_URL=https://localhost:7215/api`

### Production
- Backend: `https://shiftmate-vow0.onrender.com`
- Frontend: Vercel
- Frontend env: `.env.production` → `VITE_API_BASE_URL=https://shiftmate-vow0.onrender.com/api`

---

## SECURITY CHECKLIST

- Never expose `PasswordHash` or internal IDs unnecessarily in API responses.
- Always validate user ownership before mutations (e.g., user can only cancel their own swap).
- Use `[Authorize(Roles = "Manager")]` for manager-only endpoints — never check roles in handlers.
- Hash passwords with BCrypt — never store plaintext.
- JWT tokens include `UserId`, `Email`, and `Role` as claims.
- Sanitize all user input via FluentValidation before processing.
- Frontend: never trust client-side role checks alone — backend enforces authorization.

---

## IMPLEMENTATION WORKFLOW

Follow these steps **in order** for every implementation task:

1. **Read CLAUDE.md** — This file is the source of truth for patterns and rules.
2. **Create a new branch** — `feature/`, `fix/`, or `refactor/` before making any changes.
3. **Analyze relevant files** — Read existing code to understand patterns. Check that the feature doesn't already exist. Don't duplicate logic (DRY).
4. **Implement** — Write code following the rules above. Match existing patterns exactly. Keep it simple (KISS). Don't add what's not needed (YAGNI).
5. **Build & test** — Run `dotnet build`, `dotnet test`, and frontend `npm run build`. All must pass.
6. **Let the user test** — Wait for the user to verify in the browser. Fix issues if needed.
7. **Update documentation** — CLAUDE.md (new endpoints/structure), README.md, frontend README if relevant.
8. **Commit & push** — Descriptive commit message in English. Push branch to GitHub.
9. **Create PR** — Use the commit message as title.
10. **Merge to main** — Only after user approval.
11. **Clean up** — Checkout main, pull latest, delete branch locally and on GitHub.
