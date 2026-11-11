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
- **Email:** Resend HTTP API via IEmailService (`ConsoleEmailService` logs emails instead in Development when `Resend:ApiKey` is missing)
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → Api)

### Frontend (React 19 + Vite)

- **Core:** React 19, JavaScript (ES6+), functional components only
- **Build:** Vite 7.2.4
- **Styling:** Tailwind CSS 4.1.18 (Theme: Neon Dark — `bg-slate-950`, `text-blue-400`)
- **Typography:** Plus Jakarta Sans (headings/bold), DM Sans (body) via Google Fonts
- **State/Network:** Axios with JWT interceptor, React Hooks (`useState`, `useEffect`)
- **Routing:** React Router v7
- **Notifications:** Toast system via `ToastContext` (success, error, info, warning)
- **Testing:** Vitest + Testing Library + jsdom (`npm test` = `vitest run`)
- **Lint/Format:** ESLint + Prettier (`.prettierrc`: `singleQuote: true`, `endOfLine: "auto"`)

### CI & Code Quality

- **CI:** GitHub Actions — single workflow `.github/workflows/ci.yml` on PRs and pushes to `main`
  - `changes` (dorny/paths-filter, pinned to commit SHA) → `backend` / `frontend` (skipped via `if:` when their part is unchanged) → `ci-ok` ("CI OK", `if: always()`, fails if any job failed or was cancelled)
  - Backend job: `dotnet restore` → `dotnet format --verify-no-changes` → build → test
  - Frontend job: `npm ci` → lint → `format:check` → test → build (Node version from `.nvmrc`)
- **Branch protection:** ruleset on `main` — PR required, **`CI OK`** is the only required check, force push and deletion blocked. Never require the backend/frontend jobs directly: a skipped path would leave the check missing and block the PR forever.
- **Formatting:** `.editorconfig` in repo root (read by `dotnet format` and Prettier). `end_of_line` is deliberately omitted (Windows CRLF checkout would fail locally).
- **PR template:** `.github/pull_request_template.md` — fill it in, use `Fixes #N` (English keyword).
- **Dependabot:** `.github/dependabot.yml` — weekly (Monday) PRs for npm, NuGet and GitHub Actions; minor+patch grouped per ecosystem, majors separate. Major updates ignored for packages tied to the .NET version (`Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore*`, `Microsoft.Extensions.*`, `Npgsql.EntityFrameworkCore.PostgreSQL`) and `MediatR` (13+ is commercially licensed). Commit prefix `chore(deps)`, label `dependencies`.
- **Contributor guide:** `CONTRIBUTING.md` — workflow, CI and how to fix red checks locally.

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

1. **Controllers** must be thin — receive HTTP request, extract user from JWT claims, delegate to MediatR (`_mediator.Send()`), return HTTP response. **No try/catch** — `ExceptionHandlingMiddleware` maps thrown exceptions to status codes globally. No business logic.
2. **Entities** must NEVER be returned from the API. Always map to **DTOs**.
3. **Business logic** lives exclusively in `Application/[Feature]/Commands/[Name]/` or `Application/[Feature]/Queries/[Name]/` handlers.
4. **New commands/queries** must follow the existing CQRS pattern: each command/query in its own folder with command, handler, and (if needed) validator as separate files (`{Name}Command.cs`, `{Name}CommandHandler.cs`, `{Name}CommandValidator.cs`).
5. **Validation** uses FluentValidation in the Application layer. Every command that accepts user input SHOULD have a validator.
6. **Reference examples:** Look at `Shifts/Commands/CreateShift/` (command + handler + validator) or `ShiftsController.cs` for style and patterns.

### Error Handling Pattern

Handlers throw exceptions; `ExceptionHandlingMiddleware` (in `Api/Middleware/`) maps them to HTTP status codes globally. **Controllers must not have try/catch.**

```csharp
// In handlers — throw the most specific exception type
throw new ValidationException(validationResult.Errors);    // FluentValidation → 400
throw new NotFoundException("Passet hittades inte.");      // → 404
throw new ForbiddenException("Du har inte behörighet.");   // → 403
throw new ConflictException("E-posten finns redan.");      // → 409
throw new EmailNotVerifiedException("...");                // → 400 + Code: "EMAIL_NOT_VERIFIED"
throw new InvalidOperationException("Business rule msg."); // → 400 (generic business rule)

// In controllers — no try/catch. Just delegate.
var result = await _mediator.Send(command);
return Ok(new { Message = "Framgångsmeddelande", result });
```

Custom exceptions live in `Application/Common/Exceptions/`. Add new ones there if you need a status code that isn't covered yet.

**Anti-enumeration exception:** `ForgotPassword` and `ResendVerification` controllers keep an inline `try { } catch { }` that swallows errors silently — this is intentional so the response is identical regardless of whether the email exists.

### API Response Format

All responses use consistent JSON structure:

```
Success:           { "message": "...", "id": "..." }  or  [array of DTOs]
Paginated:         { "items": [...], "totalCount": N, "page": N, "pageSize": N, "totalPages": N }
Error (generic):   { "error": true, "message": "..." }
Error (validation):{ "error": true, "message": "Valideringsfel: ...", "details": ["...", "..."] }
Error (special):   { "error": true, "message": "...", "code": "EMAIL_NOT_VERIFIED" }
Login:             { "token": "jwt.string" }
```

- ASP.NET Core serializes JSON with **camelCase** by default (no custom naming policy configured).
- Frontend must use **camelCase** when reading response properties (e.g., `data.message`, not `data.Message`).
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

1. Use **functional components** only. No class components. **One exception:** `components/ErrorBoundary.jsx` must be a class component — React has no hook equivalent for `componentDidCatch`/`getDerivedStateFromError`. It wraps `<App />` in `main.jsx`.
2. All API calls go through `src/api.js` (centralized Axios instance with helper functions). Never call Axios directly in components.
3. Handle 401 (Unauthorized) automatically via the Axios interceptor (auto-logout).
4. Follow the existing neon dark theme (`bg-slate-950`, `text-blue-400`, `border-blue-500/30`).
5. Time formatting uses Swedish locale (`sv-SE`).
6. Use `useToast()` and `useConfirm()` from `ToastContext` for user feedback — never `alert()` or `window.confirm()`.
7. Use `Promise.all()` when loading multiple independent data sources in `useEffect`.
8. Loading states: global `loading` for initial data fetch, `actionLoading` (with ID) for per-item button actions.
9. Role checks via `getUserRole()` from `api.js` — conditionally render Manager-only UI.
10. **ESLint errors fail CI, warnings do not.** Six `react-hooks/exhaustive-deps` warnings remain deliberately — adding effect dependencies can change behavior and needs manual testing per component. Don't "fix" them as a side effect of unrelated work.
11. `contexts/ToastContext.jsx` has `eslint-disable-next-line react-refresh/only-export-components` on `useToast`/`useConfirm` — hooks live next to the provider on purpose (standard context pattern); the rule only affects dev hot reload.
12. Don't call `setState` synchronously inside `useEffect` to derive state from props (`react-hooks/set-state-in-effect`). Set initial state directly, or compare with the previous prop during render (see `DatePicker.jsx`, `TimePicker.jsx`).

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
| Folder per command | `[Feature]/Commands/[VerbEntity]/` | `Shifts/Commands/CreateShift/` |
| Folder per query | `[Feature]/Queries/[GetWhat]/` | `Shifts/Queries/GetAllShifts/` |
| Command | `[Verb][Entity]Command.cs` | `CreateShiftCommand.cs` |
| Command handler | Separate file: `[Verb][Entity]CommandHandler.cs` | `CreateShiftCommandHandler.cs` |
| Command validator | Separate file: `[Verb][Entity]CommandValidator.cs` | `CreateShiftCommandValidator.cs` |
| Query | `[Get][What]Query.cs` | `GetAllShiftsQuery.cs` |
| Query handler | Separate file: `[Get][What]QueryHandler.cs` | `GetAllShiftsQueryHandler.cs` |
| Custom exception | `[Name]Exception.cs` in `Application/Common/Exceptions/` | `NotFoundException.cs` |
| DTO | `[Entity]Dto.cs` in `Application/DTOs/` | `ShiftDto.cs` |
| Controller | `[Entity plural]Controller.cs` | `ShiftsController.cs` |
| Interface | `I[Name].cs` | `IAppDbContext.cs` |
| Middleware | `[Name]Middleware.cs` in `Api/Middleware/` | `ExceptionHandlingMiddleware.cs` |

### Frontend

| What | Convention | Example |
|------|-----------|---------|
| Page component | `PascalCase.jsx` | `Dashboard.jsx` |
| Shared component | `components/PascalCase.jsx` | `components/ManagerPanel.jsx` |
| Feature component | `components/[feature]/PascalCase.jsx` | `components/manager/ShiftForm.jsx` |
| UI component | `components/ui/PascalCase.jsx` | `components/ui/ConfirmModal.jsx` |
| Context | `contexts/[Name]Context.jsx` | `contexts/ToastContext.jsx` |
| API helper | `camelCase` function in `api.js` | `fetchMyShifts()` |

### Test Files

| What | Convention | Example |
|------|-----------|---------|
| Test file | `[HandlerName]Tests.cs` | `CreateShiftHandlerTests.cs` |
| Integration test file | `Integration/[Controller]EndpointsTests.cs` | `ShiftsEndpointsTests.cs` (HTTP via `ShiftMateApiFactory`) |
| Test method | `[Method]_Should_[Behavior]_When_[Condition]` | `Handle_Should_Throw_When_User_Not_Found` |
| Structure | Arrange → Act → Assert (AAA) | Always follow this order |
| Tools | xUnit `[Fact]`, FluentAssertions, Moq | `result.Should().NotBeEmpty()` |
| Frontend test file | `[Component].test.jsx` next to the component (Vitest only finds `.test.`/`.spec.`) | `components/EmptyState.test.jsx` |
| Frontend tools | Vitest (`describe`/`it`/`expect`), Testing Library (`render`, `screen`), jsdom | `screen.getByText('...')` |

---

## GIT WORKFLOW

### Branching Strategy

- **Main branch:** `main` (production-ready code)
- **Every change MUST be developed in a new branch** before merging to main.
- **Branch naming:** `feature/<name>`, `fix/<name>`, `refactor/<name>` (CI changes: `ci/<name>`)
- **Examples:** `feature/password-reset`, `fix/swap-validation`, `refactor/shift-components`
- Always create a PR to merge back into `main` — direct pushes are blocked by the ruleset.
- Keep commits focused and descriptive, in **English**, using Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `style:`, `ci:`, `chore:`). Subject line under 72 characters (GitHub truncates longer PR titles).
- Commit formatting-only changes separately (`style: ...`) so reviewers can skip them.

---

## PROJECT STRUCTURE

### Repo root

```
.editorconfig                                    # Shared formatting rules (dotnet format + Prettier)
.github/workflows/ci.yml                         # CI: changes → backend / frontend → ci-ok ("CI OK")
.github/pull_request_template.md                 # PR template
.github/dependabot.yml                           # Weekly dependency update PRs
AUDIT.md                                         # Code review baseline + follow-up
CONTRIBUTING.md                                  # Workflow, CI, fixing red checks
```

### Backend

```
ShiftMate.Domain/                                # Entities + enums. No dependencies.
  Entities/                                      # User.cs, Shift.cs, SwapRequest.cs, Organization.cs
  Enums/                                         # Role.cs, SwapRequestStatus.cs

ShiftMate.Application/                           # Business logic layer (CQRS)
  Common/
    PagedResult.cs                               # Generic pagination wrapper
    Exceptions/                                  # NotFoundException, ForbiddenException,
                                                 # ConflictException, EmailNotVerifiedException
  DTOs/                                          # ShiftDto, UserDto, SwapRequestDto, OrganizationDto
  Interfaces/                                    # IAppDbContext, IEmailService
  Services/                                      # EmailTemplateService (static helper)
  [Feature]/Commands/[VerbEntity]/               # One folder per command — contains:
                                                 #   - {Name}Command.cs
                                                 #   - {Name}CommandHandler.cs
                                                 #   - {Name}CommandValidator.cs (if needed)
  [Feature]/Queries/[GetWhat]/                   # One folder per query — contains:
                                                 #   - {Name}Query.cs
                                                 #   - {Name}QueryHandler.cs
  Organizations/InviteCodeGenerator.cs           # Feature-local static helper
  DependencyInjection.cs                         # MediatR + FluentValidation registration

ShiftMate.Infrastructure/                        # Data access & external services
  AppDbContext.cs                                # EF Core DbContext with relationships
  DbInitializer.cs                               # Dev-only seed data (2 orgs, test users & shifts) — never deletes data
  Services/                                      # ResendEmailService, ConsoleEmailService (dev without Resend key)
  Migrations/                                    # EF Core migrations

ShiftMate.Api/                                   # HTTP layer
  Program.cs                                     # DI configuration & middleware pipeline
  Controllers/                                   # Thin controllers — no try/catch
  Middleware/                                    # ExceptionHandlingMiddleware (global error mapping)
  Extensions/                                    # ClaimsPrincipalExtensions (User.GetUserId(), etc.)

ShiftMate.Tests/                                 # Unit + integration tests (xUnit + FluentAssertions + Moq)
  Support/TestDbContextFactory.cs                # InMemory DbContext for handler tests
  Integration/ShiftMateApiFactory.cs             # WebApplicationFactory: InMemory DB, fake email, test JWTs
  Integration/[Controller]EndpointsTests.cs      # HTTP tests: auth, roles, error mapping, multi-tenancy
```

### Frontend (`shiftmate-frontend/src/`)

```
main.jsx                        # React entrypoint
App.jsx                         # Main routing, auth check, sidebar navigation
api.js                          # Axios config + JWT interceptor + API helper functions (lowercase endpoint paths)
index.css                       # Global Tailwind styles + animations
utils/
  dateUtils.js                  # Swedish-locale date/time formatting helpers (formatDate, formatTime)
Dashboard.jsx                   # Landing page with overview & quick stats
Login.jsx                       # Login page
Register.jsx                    # Registration page
ForgotPassword.jsx              # Forgot password — request reset email
ResetPassword.jsx               # Reset password with token from email
VerifyEmail.jsx                 # Email verification (token from URL)
AdminPanel.jsx                  # SuperAdmin: organization management
ShiftList.jsx                   # "Mina Pass" — user's shifts + incoming swap requests
MarketPlace.jsx                 # "Lediga Pass" — claimable shifts
Schedule.jsx                    # "Schema" — full schedule grouped by date
Profile.jsx                     # User profile & stats
contexts/
  ToastContext.jsx              # Toast notifications + confirm dialog (useToast, useConfirm)
components/
  AuthLayout.jsx                # Shared auth page layout (login/register)
  ManagerPanel.jsx              # Manager tab shell (delegates to manager/ sub-components)
  NotificationDropdown.jsx      # Bell icon with swap request notifications
  EmptyState.jsx                # Reusable empty state placeholder
  LoadingSpinner.jsx            # Reusable loading spinner
  ErrorBoundary.jsx             # Catches render errors, shows fallback (only class component — see Frontend Rules)
  manager/
    ShiftForm.jsx               # Create new shift (date, time, quick-select, assign user)
    ShiftListTab.jsx            # All shifts with edit, delete, pagination
    UserManagement.jsx          # User list with role change and deactivation
    InviteCode.jsx              # View/copy/regenerate invite code
  ui/
    ToastContainer.jsx          # Toast notification list (portal-rendered)
    ConfirmModal.jsx            # Confirmation dialog (portal-rendered)
    DatePicker.jsx              # Custom calendar date picker (Swedish locale, Monday-first)
    TimePicker.jsx              # Custom two-step time picker (hour → minute grid)
    UserSelect.jsx              # Custom user dropdown with search and avatars
  schedule/
    NavigationBar.jsx           # Date navigation for schedule views
    ViewToggle.jsx              # Day/Week/Month view toggle
    DayView.jsx                 # Daily schedule view
    WeekView.jsx                # Weekly schedule view
    MonthView.jsx               # Monthly schedule view
    ShiftCard.jsx               # Individual shift card in schedule
```

---

## DATA MODEL (PostgreSQL)

- **Organization:** `Id` (Guid), `Name` (string, unique), `InviteCode` (string, max 8, unique), `InviteCodeGeneratedAt` (DateTime), `CreatedAt` (DateTime)
- **User:** `Id` (Guid), `Email` (unique, case-insensitive), `FirstName`, `LastName`, `Role` (Employee/Manager/SuperAdmin), `PasswordHash`, `IsEmailVerified` (bool), `EmailVerificationTokenHash`, `EmailVerificationTokenExpiresAt`, `ResetTokenHash`, `ResetTokenExpiresAt`, `IsActive` (bool, default true), `DeactivatedAt` (DateTime?), `OrganizationId` (FK → Organization)
- **Shift:** `Id`, `StartTime`, `EndTime`, `UserId` (nullable FK → User), `IsUpForSwap` (bool), `OrganizationId` (FK → Organization)
- **SwapRequest:** `Id`, `ShiftId` (FK), `RequestingUserId` (FK), `TargetUserId` (nullable FK), `TargetShiftId` (nullable FK), `Status` (SwapRequestStatus enum: Pending/Accepted/Declined/Cancelled, stored as string), `CreatedAt`

### Multi-Tenancy

All data is scoped by Organization. Query handlers filter with `.Where(x => x.OrganizationId == orgId)`. Command handlers validate org ownership. The OrganizationId is extracted from JWT claims via `User.GetOrganizationId()` in controllers.

---

## API ENDPOINTS

### Public (No Auth)
- `POST /api/users/login` — Login → returns JWT token
- `POST /api/users/register` — Register (requires InviteCode) → returns UserDto
- `POST /api/users/forgot-password` — Request password reset email
- `POST /api/users/reset-password` — Reset password with token
- `POST /api/users/verify-email` — Verify email with token
- `POST /api/users/resend-verification` — Resend verification email
- `GET /health` — Health check

### Authenticated (Any Role)
- `GET /api/users` — Get all users (?page=1&pageSize=20 for pagination)
- `PUT /api/users/profile` — Update own profile
- `PUT /api/users/change-password` — Change own password
- `GET /api/shifts` — All shifts (?onlyWithUsers=true&page=1&pageSize=20 for pagination)
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
- `GET /api/organizations/my-invite-code` — View own org's invite code
- `POST /api/organizations/{id}/regenerate-invite-code` — Regenerate invite code
- `POST /api/shifts/admin` — Create shift and assign to user
- `PUT /api/shifts/{id}` — Update any shift
- `DELETE /api/shifts/{id}` — Delete any shift
- `DELETE /api/users/{id}` — Soft delete (deactivate) a user
- `PUT /api/users/{id}/role` — Change a user's role

### SuperAdmin Only
- `GET /api/organizations/admin` — All organizations with details
- `POST /api/organizations` — Create new organization
- `PUT /api/organizations/{id}` — Update organization
- `DELETE /api/organizations/{id}` — Delete organization (cascades to users + shifts)

---

## ENVIRONMENT

### Development
- Backend: `https://localhost:7215` (`dotnet run` uses the https profile; Development runs `Database.Migrate()` + seed on startup)
- Frontend: `http://localhost:5173` (`strictPort` — CORS only allows this port; Node version in `.nvmrc`)
- Frontend env: `.env.development` → `VITE_API_URL=https://localhost:7215/api`

### Production
- Backend: `https://shiftmate-vow0.onrender.com`
- Frontend: Vercel
- Frontend env: `.env.production` → `VITE_API_URL=https://shiftmate-vow0.onrender.com/api`

---

## SECURITY CHECKLIST

- Never expose `PasswordHash` or internal IDs unnecessarily in API responses.
- Always validate user ownership before mutations (e.g., user can only cancel their own swap).
- Use `[Authorize(Roles = "Manager")]` for manager-only endpoints — never check roles in handlers.
- Hash passwords with BCrypt — never store plaintext.
- JWT tokens include `UserId`, `Email`, `Role`, `OrganizationId`, and `OrganizationName` as claims.
- Sanitize all user input via FluentValidation before processing.
- Frontend: never trust client-side role checks alone — backend enforces authorization.

---

## IMPLEMENTATION WORKFLOW

Follow these steps **in order** for every implementation task:

1. **Read CLAUDE.md** — This file is the source of truth for patterns and rules.
2. **Create a new branch** — `feature/`, `fix/`, or `refactor/` before making any changes.
3. **Analyze relevant files** — Read existing code to understand patterns. Check that the feature doesn't already exist. Don't duplicate logic (DRY).
4. **Implement** — Write code following the rules above. Match existing patterns exactly. Keep it simple (KISS). Don't add what's not needed (YAGNI).
5. **Build & test** — Run the same checks as CI. All must pass.
   - Backend (in `ShiftMate/`): `dotnet format --verify-no-changes`, `dotnet build`, `dotnet test`
   - Frontend (in `ShiftMate/shiftmate-frontend/`): `npm run lint`, `npm run format:check`, `npm test`, `npm run build`
   - If formatting fails, run `dotnet format` / `npm run format` and commit that separately.
6. **Let the user test** — Wait for the user to verify in the browser. Fix issues if needed.
7. **Update ALL documentation** — Before committing, always review and update: CLAUDE.md (new endpoints, structure, patterns), README.md (test counts, features, file structure), CONTRIBUTING.md (workflow/CI changes), and frontend README.md if frontend changes were made. Every `.md` file that references changed areas must be updated.
8. **Commit & push** — Descriptive commit message in English. Push branch to GitHub.
9. **Create PR** — Use the commit message as title. Fill in the PR template (what/why, `Fixes #N`, trade-offs, how it was tested, checklist). Wait for **CI OK** to be green.
10. **Merge to main** — Only after user approval.
11. **Clean up** — Checkout main, pull latest, delete branch locally and on GitHub.
