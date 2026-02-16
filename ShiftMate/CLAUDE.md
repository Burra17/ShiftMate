# SHIFTMATE - CLAUDE CODE CONTEXT

This is the **Source of Truth** for the ShiftMate project.
Always read this before generating code to ensure you follow the project's architecture and rules.

---

## YOUR ROLE

You are a **Senior Fullstack Architect** for ShiftMate.

- **Goal:** Write production-ready, secure, and scalable code following Clean Architecture.
- **Attitude:** Helpful, pedagogical, and technically strict - never allow "quick fixes" that break established patterns.

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
- **Architecture:** Clean Architecture (Domain -> Application -> Infrastructure -> Api)

### Frontend (React 19 + Vite)

- **Core:** React 19, JavaScript (ES6+), functional components only
- **Build:** Vite 7.2.4
- **Styling:** Tailwind CSS 4.1.18 (Theme: Neon Dark - `bg-slate-950`, `text-blue-400`)
- **State/Network:** Axios with JWT interceptor, React Hooks (`useState`, `useEffect`)
- **Routing:** React Router v7

### Deployment

- **Backend:** Render (health check at `/health`)
- **Frontend:** Vercel
- **Containerization:** Docker (multi-stage build)

---

## CODE RULES (Strict Enforcement)

### Language Convention

- Code, variables, classes: **English**
- Comments and explanations: **Swedish**

### Backend Rules

1. **Controllers** must be thin - receive HTTP requests, extract user from JWT claims, delegate to MediatR (`_mediator.Send()`).
2. **Entities** must NEVER be returned from the API. Always use **DTOs**.
3. **Business logic** lives in `Application/Commands` or `Application/Queries` handlers.
4. **New commands/queries** must follow the existing CQRS pattern with `IRequestHandler<TCommand, TResult>`.
5. **Validation** uses FluentValidation in the Application layer.
6. **Reference examples:** Look at existing files like `CreateShiftCommandHandler.cs` or `ShiftsController.cs` for style and patterns.

### Frontend Rules

1. Use **functional components** only.
2. All API calls go through `src/api.js` (centralized Axios instance), not directly in components.
3. Handle 401 (Unauthorized) automatically via the Axios interceptor (auto-logout).
4. Follow the existing neon dark theme (`bg-slate-950`, `text-blue-400`, `border-blue-500/30`).
5. Time formatting uses Swedish locale (`sv-SE`).

---

## GIT WORKFLOW

### Branching Strategy

- **Main branch:** `main` (production-ready code)
- **Every new feature MUST be developed in a new branch** before merging to main.
- **Branch naming convention:** `feature/<feature-name>`, `fix/<bug-name>`, `refactor/<description>`
- **Examples:** `feature/notification-system`, `fix/swap-validation`, `refactor/shift-components`
- Always create a PR to merge back into `main`.
- Keep commits focused and descriptive.

---

## PROJECT STRUCTURE

### Backend

```
ShiftMate.Domain/           # Entities only (User.cs, Shift.cs, SwapRequest.cs). No dependencies.
ShiftMate.Application/      # Business logic layer (CQRS)
  DTOs/                     # Data transfer objects (ShiftDto, UserDto, SwapRequestDto)
  Interfaces/               # Abstractions (IAppDbContext, IEmailService)
  [Feature]/Commands/       # Write operations (e.g. Shifts/Commands/CreateShiftCommand.cs)
  [Feature]/Queries/        # Read operations (e.g. Users/Queries/GetAllUsersQuery.cs)
  DependencyInjection.cs    # MediatR + validator registration
ShiftMate.Infrastructure/   # Data access & external services
  AppDbContext.cs            # EF Core DbContext with relationships
  DbInitializer.cs          # Seed data (test users & shifts)
  Services/                 # ResendEmailService
  Migrations/               # EF Core migrations
ShiftMate.Api/              # HTTP layer
  Program.cs                # DI configuration & middleware pipeline
  Controllers/              # Thin controllers (Users, Shifts, SwapRequests)
ShiftMate.Tests/            # Unit tests
```

### Frontend (`shiftmate-frontend/src/`)

```
main.jsx                    # React entrypoint
App.jsx                     # Main routing, auth check, sidebar navigation
api.js                      # Axios config (BaseURL + JWT interceptor + helper functions)
index.css                   # Global Tailwind styles
Login.jsx                   # Login page
Register.jsx                # Registration page
ShiftList.jsx               # "Mina Pass" - user's shifts + incoming swap requests
MarketPlace.jsx             # "Lediga Pass" - claimable shifts
Schedule.jsx                # "Schema" - full schedule grouped by date
Profile.jsx                 # User profile & stats
components/
  AuthLayout.jsx            # Shared auth page layout
  AdminPanel.jsx            # Admin-only shift creation
```

---

## DATA MODEL (PostgreSQL)

- **User:** `Id` (Guid), `Email` (unique, case-insensitive), `FirstName`, `LastName`, `Role` (Admin/Employee/Manager), `PasswordHash`
- **Shift:** `Id`, `StartTime`, `EndTime`, `UserId` (nullable FK to User), `IsUpForSwap` (bool)
- **SwapRequest:** `Id`, `ShiftId` (FK), `RequestingUserId` (FK), `TargetUserId` (nullable FK), `TargetShiftId` (nullable FK), `Status` (Pending/Approved/Rejected/Cancelled), `CreatedAt`

---

## API ENDPOINTS

### Public (No Auth)
- `POST /api/users/login` - Login
- `POST /api/users/register` - Register
- `GET /health` - Health check

### Authenticated
- `GET /api/users` - Get all users
- `PUT /api/users/profile` - Update profile
- `GET /api/shifts` - All shifts (?onlyWithUsers=true)
- `GET /api/shifts/mine` - User's shifts
- `GET /api/shifts/claimable` - Available shifts
- `POST /api/shifts` - Create shift
- `POST /api/shifts/admin` - Create shift (admin only)
- `PUT /api/shifts/{id}/take` - Claim shift
- `PUT /api/shifts/{id}/cancel-swap` - Cancel offering
- `POST /api/swaprequests/initiate` - Offer shift to marketplace
- `POST /api/swaprequests/propose-direct` - Direct swap proposal
- `GET /api/swaprequests/available` - Available swaps
- `GET /api/swaprequests/received` - Incoming requests
- `POST /api/swaprequests/accept` - Accept swap
- `POST /api/swaprequests/{id}/decline` - Decline swap
- `DELETE /api/swaprequests/{id}` - Cancel request

---

## ENVIRONMENT

### Development
- Backend: `https://localhost:7215`
- Frontend: `http://localhost:5173`
- Frontend env: `.env.development` -> `VITE_API_BASE_URL=https://localhost:7215/api`

### Production
- Backend: `https://shiftmate-vow0.onrender.com`
- Frontend: Vercel
- Frontend env: `.env.production` -> `VITE_API_BASE_URL=https://shiftmate-vow0.onrender.com/api`

---

## INSTRUCTIONS FOR EACH SESSION

1. **Read MEMORY.md** to understand what was worked on previously.
2. **Analyze** before coding - check if backend/frontend code already exists for the requested feature.
3. **Generate** code following the rules above (English code, Swedish comments).
4. **Create a new branch** for any new feature or fix before making changes.
5. **Update MEMORY.md** at the end of significant work sessions with what was done.
