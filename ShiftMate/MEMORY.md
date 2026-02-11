# SHIFTMATE - SESSION MEMORY

This file tracks what has been worked on across sessions.
Update this file at the end of each significant work session.

---

## CURRENT STATUS

- **Active Branch:** `migration/claude`
- **Last Updated:** 2026-02-11
- **Project State:** Stable, all core features implemented

---

## SESSION LOG

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
  - (To be filled as work continues)

---

## DECISIONS LOG

Track important architectural or design decisions here.

| Date | Decision | Reason |
|------|----------|--------|
| 2026-02-11 | Switched from Gemini CLI to Claude Code | Better developer experience |
| 2026-02-11 | Created CLAUDE.md + MEMORY.md | Consistent context across sessions |

---

## NOTES

- Test accounts are seeded via `DbInitializer.cs`
- Comments and explanations should be in Swedish
- Every new feature must be on a new branch (see CLAUDE.md git workflow)
