# AUDIT – ShiftMate

Granskning av projektet ur perspektivet "ny kollega som precis fått repot i knät".

**Poängskala:** 0 = saknas, 1 = finns lite, 2 = sitter bra

---

## Steg 1: Starta från noll

Repot klonades till en tom mapp och startades enbart med hjälp av README. Följande saker krävde att man "bara visste":

- **API:et startar inte på https med README-kommandot.** `dotnet run --project ShiftMate.Api` väljer första profilen i `launchSettings.json` (`http`, port 5092). Resultatet blir varningen `Failed to determine the https port for redirect`, och frontend (som pekar på `https://localhost:7215`) når inte backend. Det krävs `--launch-profile https`.
- **Dev-certifikat nämns inte.** `dotnet dev-certs https --trust` saknas i instruktionerna.
- **Konfigurationen är otydlig.** README säger att connection string ska skrivas i `appsettings.json` (risk för att hemligheter committas) i stället för user-secrets.
- **JWT-inställningar saknas helt.** `Jwt:Key`, `Jwt:Issuer` och `Jwt:Audience` nämns varken i README eller som mall i appsettings. Appen kraschar vid start utan dem (`Program.cs` läser `Jwt:Key` med `!`). Det gick bara att starta för att gamla user secrets fanns kvar lokalt.
- **`Resend:ApiKey` är tom utan förklaring.** Det framgår inte om appen fungerar utan den.
- **Fel variabelnamn i README.** Där står `VITE_API_BASE_URL`, men koden använder `VITE_API_URL`.
- **Klonsteget och mappstrukturen saknas.** `ShiftMate.sln` ligger i undermappen `ShiftMate/`, inte i repots rot.
- **`dotnet-ef` förutsätts vara installerat.** `dotnet tool install --global dotnet-ef` saknas.
- **Inga testkonton beskrivs.** `DbInitializer` seedar användare vid start, men README säger inte hur man loggar in. Registrering kräver en inbjudningskod.
- **`package-lock.json` ändrades vid `npm install`.** Orsaken är en annan npm-version än den som skapade låsfilen (`"peer": true`-rader försvann). Varken Node- eller npm-version är låst (ingen `.nvmrc` eller `engines`), och README använder `npm install` i stället för `npm ci`.
- **Inaktuell filstruktur i README.** `MEMORY.md` nämns men finns inte, och det står inte hur man kör testerna.

---

## Steg 2: Checklista

### Hur koden är byggd

| Punkt | Poäng | Kommentar |
|---|:---:|---|
| Är controllers tunna, och ligger logiken i services? | 2 | Controllers läser bara claims, delegerar via `_mediator.Send()` och returnerar svaret. Logiken ligger i CQRS-handlers i stället för services, vilket ger en ännu finare uppdelning (en handler per use case). De två `try/catch` i `UsersController` är avsiktliga, för att inte avslöja vilka e-postadresser som finns. Liten anmärkning: kontrollen `if (userId == null) return Unauthorized();` upprepas i nästan varje action. |
| Används DTOs, eller skickas databasobjekten rakt ut till frontend? | 2 | Alla handlers mappar entiteter till DTOs (`ShiftDto`, `UserDto` m.fl.). Inga entiteter eller känsliga fält som `PasswordHash` lämnar API:et. |
| Är frontend uppdelad på ett sätt som går att hitta i? | 1 | `components/` har en början på feature-mappar (`manager/`, `schedule/`, `ui/`), men alla sidor ligger direkt i `src/` blandat med `App.jsx`, `api.js` och `main.jsx`. Flera sidor är stora (`Dashboard.jsx` 503 rader, `Profile.jsx` 392), och `api.js` (360 rader) samlar alla features i en fil. En `pages/`-mapp och utbrutna komponenter vore ett första steg, med feature-baserad struktur som möjligt nästa steg. |

### Hur koden mår

| Punkt | Poäng | Kommentar |
|---|:---:|---|
| Finns det tester, och går de att köra med ett enda kommando? | 1 | Backend har 198 enhetstester (xUnit, FluentAssertions, Moq) som alla går gröna med `dotnet test`. Frontend saknar tester helt (bara ESLint, som ger flera fel och varningar). Det finns inga integrationstester för controllers eller middleware, och README säger inte hur testerna körs. |
| Kollar servern själv att datan är rimlig, eller litar den blint på formuläret? | 2 | 11 FluentValidation-validators täcker alla commands med fri användarinmatning (skapa/uppdatera pass, registrering, lösenord m.m.). De 13 commands utan validator tar i princip bara emot id:n, och deras handlers kontrollerar ägarskap, organisation och status och kastar `NotFound`/`Forbidden`/`Conflict`. |
| Får användaren ett vettigt besked när något går fel? | 2 | `ExceptionHandlingMiddleware` mappar alla undantag till rätt statuskod med ett enhetligt JSON-format och svenska meddelanden, och döljer detaljer vid 500-fel i produktion. Frontend visar felen via toasts (`useToast`). |
| Visar React att något laddar, och vad som händer när listan är tom? | 2 | Återanvändbara `LoadingSpinner` och `EmptyState` används på sidorna, och `actionLoading` visar laddning per knapp. En `ErrorBoundary` förhindrar vit skärm vid renderingsfel. |

### Hur repot fungerar för ett team

| Punkt | Poäng | Kommentar |
|---|:---:|---|
| Räcker README för att någon annan ska kunna starta projektet? | 1 | README beskriver projektet bra (funktioner, stack, arkitektur), men startguiden har fel och luckor: fel launch-profil (API:et startar bara på http), JWT-inställningar och user-secrets nämns inte, fel env-variabelnamn, inga testkonton och inga låsta Node- eller npm-versioner. Se Steg 1. |
| Är bin, obj och node_modules utanför git? | 2 | Täcks av `.gitignore` (Visual Studio-mallen i roten och Vite-mallen i frontend). `git ls-files` visar 0 spårade filer i `bin/`, `obj/` eller `node_modules/`. |
| Finns det lösenord eller nycklar i koden eller i historiken? | 1 | Nuvarande appsettings innehåller bara platshållare, och hemligheterna ligger i user-secrets. Men **historiken innehåller en JWT-signeringsnyckel** (committad i `6ad2681`, borttagen i `96196bc`) samt Supabase-host och användarnamn (lösenordet var en platshållare). `DbInitializer` innehåller seed-lösenord i klartext och en privat e-postadress. Nyckeln har bytts i produktion och lokalt efter granskningen. |
| Ser commit-historiken ut som en berättelse? | 2 | Ungefär 215 commits. De senaste följer Conventional Commits med scope (`feat(frontend):`, `refactor(exceptions):`) och mergas via PR från feature-grenar. Inga "fix2"/"nu funkar det", men några tidiga commits är vaga (`cleanup`, `uptime robot`) och stilen varierar (`Feat:` eller `feat:`, en commit på svenska). |
| Är koden formaterad på samma sätt överallt? | 0 | `dotnet format --verify-no-changes` ger många fel. Prettier finns inte i projektet (inget beroende, ingen `.prettierrc`), och det saknas `.editorconfig`. ESLint ger fel och varningar. Det finns ett CI-flöde (`.github/workflows/ci.yml`) som bygger och kör testerna, men det kontrollerar varken formatering eller lint. |

---

## Steg 3: Fem viktigaste förbättringar

Prioriterade efter vad som stoppar en ny kollega eller skapar risk, i den ordning de ska åtgärdas. Alla finns som GitHub Issues med labeln `quality`.

1. **Säkerhet: rensa privat data och klartextlösenord ur seed-data** ([#99](https://github.com/Burra17/ShiftMate/issues/99))
   En JWT-nyckel låg i git-historiken. Den är nu bytt i produktion och lokalt, så den läckta nyckeln är ogiltig (historiken skrivs inte om). Kvar att göra: ta bort den privata e-postadressen och klartextlösenorden ur `DbInitializer`.
2. **Onboarding: README-startguide som fungerar från tom klon + låst Node-version** ([#100](https://github.com/Burra17/ShiftMate/issues/100))
   Rätta alla luckor från Steg 1 (launch-profil, dev-cert, user-secrets inklusive JWT, variabelnamn, testkonton). Lås Node 24 med `.nvmrc` och `engines` så att CI och lokal miljö använder samma version.
3. **Formatering: lägg till .editorconfig och Prettier + engångsformatering** ([#101](https://github.com/Burra17/ShiftMate/issues/101))
   Gemensamma regler för backend och frontend. Den första formateringen görs i en egen PR som bara innehåller formatering.
4. **CI: åtgärda ESLint-fel och kontrollera lint/format i CI** ([#102](https://github.com/Burra17/ShiftMate/issues/102))
   Det befintliga CI-flödet utökas så att formatering och lint inte kan avvika igen.
5. **Tester: integrationstester för API-lagret med WebApplicationFactory** ([#103](https://github.com/Burra17/ShiftMate/issues/103))
   Testa att `[Authorize]`, rollkontroller, `ExceptionHandlingMiddleware` och organisationsfiltrering fungerar ihop genom HTTP.

### Övriga förbättringar (inte prioriterade nu)

- **Frontend-struktur:** flytta sidor till `pages/`, bryt ut delar ur `Dashboard.jsx` (503 rader) och dela upp `api.js` per feature.
- **Frontendtester:** Vitest + React Testing Library för de viktigaste komponenterna.
- **Död kod:** `SmtpEmailService` och `EmailSettings` används inte längre (ersatta av Resend).
- **Upprepade kontroller:** `if (userId == null) return Unauthorized();` i nästan varje controller-action.
