# Bidra till ShiftMate

Hur vi arbetar i repot: från issue till mergad kod. Se [README.md](README.md) för hur du startar projektet och [CLAUDE.md](CLAUDE.md) för kodregler och arkitektur.

## Arbetsflöde

1. **Issue först** – varje ändring börjar som en GitHub-issue. Kvalitetsförbättringar får labeln `quality`.
2. **Ny gren från `main`** – `feature/<namn>`, `fix/<namn>` eller `refactor/<namn>` (ändringar i CI: `ci/<namn>`).
3. **Commits på engelska** enligt [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `style:`, `ci:`, `chore:`. Håll rubriken under 72 tecken.
4. **Kör kontrollerna lokalt** innan du pushar (se [Innan du pushar](#innan-du-pushar)).
5. **Öppna en Pull Request** – mallen fylls i automatiskt. Skriv `Fixes #N` (på engelska) så stängs issuen vid merge, eller `Del av #N` om PR:en bara löser en del.
6. **Vänta på grön `CI OK`** – utan den går det inte att merga.
7. **Merga och ta bort grenen.**

## `main` är skyddad

En regel (ruleset) på `main` gör att:

- all kod måste gå via en Pull Request – direkt push till `main` nekas
- kontrollen **`CI OK`** måste vara grön innan merge
- force push och radering av `main` är blockerade

## CI – vad körs och när?

All CI ligger i [`.github/workflows/ci.yml`](.github/workflows/ci.yml) och körs på varje PR mot `main` och varje push till `main`.

| Jobb | Körs när | Gör |
|---|---|---|
| **Detect changes** | alltid | avgör om backend och/eller frontend ändrats |
| **Backend (build and test)** | backend eller `ci.yml` ändrats | `dotnet restore` → `dotnet format --verify-no-changes` → `dotnet build` → `dotnet test` |
| **Frontend (lint, format, test, build)** | frontend eller `ci.yml` ändrats | `npm ci` → `npm run lint` → `npm run format:check` → `npm test` → `npm run build` |
| **CI OK** | alltid | blir rött om något jobb ovan misslyckades eller avbröts |

Jobb som inte behövs blir **Skipped** (räknas som godkänt). Därför kräver `main` bara **`CI OK`** – om regeln krävde backend- och frontend-jobben direkt skulle en PR som bara ändrar ena delen vänta för evigt på en kontroll som aldrig startar. Bakgrund i [#119](https://github.com/Burra17/ShiftMate/pull/119).

En ny push till samma PR avbryter en körning som redan pågår.

## Min PR är röd – vad gör jag?

Öppna det röda jobbet i fliken **Checks** och se vilket steg som failade. Varje steg avslutas med en felkod (exit code) – allt annat än `0` gör steget rött.

| Rött steg | Fixa lokalt | I mappen |
|---|---|---|
| `dotnet format --verify-no-changes` | `dotnet format` | `ShiftMate/` |
| `dotnet build` / `dotnet test` | `dotnet build` / `dotnet test` och åtgärda felet | `ShiftMate/` |
| `npm run lint` | `npm run lint` och åtgärda felen (varningar stoppar inte CI) | `ShiftMate/shiftmate-frontend/` |
| `npm run format:check` | `npm run format` | `ShiftMate/shiftmate-frontend/` |
| `npm test` / `npm run build` | `npm test` / `npm run build` och åtgärda felet | `ShiftMate/shiftmate-frontend/` |

Committa formateringsändringar för sig (t.ex. `style: apply formatting`) så att de inte blandas med logikändringar.

## Innan du pushar

Samma kontroller som CI kör – kör de som berör din ändring:

```bash
# Backend (i ShiftMate/)
dotnet format --verify-no-changes
dotnet test
```

```bash
# Frontend (i ShiftMate/shiftmate-frontend/)
npm run lint
npm run format:check
npm test
npm run build
```

## Kodstil och formatering

- **`.editorconfig`** (repots rot) – gemensamma regler för indentering och whitespace. `dotnet format` och Prettier läser den. `end_of_line` är medvetet utelämnad: på Windows med `core.autocrlf=true` checkas filerna ut med CRLF, och regeln hade gjort formateringskontrollen röd lokalt.
- **Prettier** (frontend) – `.prettierrc` med `singleQuote: true` (som koden redan var skriven) och `endOfLine: "auto"` (samma Windows-skäl). `package-lock.json` och `dist` ignoreras.
- **ESLint** (frontend) – fel stoppar CI, varningar gör det inte. Sex `react-hooks/exhaustive-deps`-varningar finns kvar medvetet: att lägga till beroenden i `useEffect` kan ändra beteendet och kräver manuell test per komponent.

## Tester

- **Backend** – enhetstester per handler (`[HandlerName]Tests.cs`) och integrationstester i `ShiftMate.Tests/Integration/` som startar hela API:et i minnet med `WebApplicationFactory`. Ingen databas eller nyckel behövs.
- **Frontend** – Vitest + Testing Library + jsdom. Testfilen ligger bredvid komponenten och heter `X.test.jsx` (Vitest hittar bara filer med `.test.` eller `.spec.` i namnet). `npm test` kör `vitest run` – utan `run` startar watch-läge, som aldrig avslutas i CI.
