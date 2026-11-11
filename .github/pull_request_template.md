## Vad gör den här PR:en?

<!-- Beskriv kort VAD som ändrats och VARFÖR. Punktlista går bra. -->

Fixes #

<!-- Skriv issue-numret efter "Fixes #" – den stängs automatiskt vid merge. Måste vara på engelska (Fixes/Closes/Resolves).
     Löser PR:en bara en del av en issue: skriv "Del av #N" istället. Ingen issue: ta bort raden. -->

## Val och avvägningar

<!-- Vilka beslut tog du, och vad valde du bort? Det är oftast det granskaren frågar om.
     Inget särskilt att nämna: ta bort rubriken. -->

## Hur testade jag?

<!-- Vad körde du, och vad såg du? Länka gärna till CI-körningar. -->

- [ ] Manuellt i webbläsaren / Swagger: <!-- vad testade du? -->

## Checklista

<!-- Samma kontroller som CI kör – bocka av innan du pushar. Stryk de rader som inte berörs (~~text~~). -->

**Backend** (i `ShiftMate/`)

- [ ] `dotnet format --verify-no-changes`
- [ ] `dotnet test`

**Frontend** (i `ShiftMate/shiftmate-frontend/`)

- [ ] `npm run lint`
- [ ] `npm run format:check`
- [ ] `npm test`
- [ ] `npm run build`

**Allmänt**

- [ ] Inga lösenord, nycklar eller connection strings i koden
- [ ] Dokumentation uppdaterad (CLAUDE.md, README.md, frontend-README – se steg 7 i CLAUDE.md)
- [ ] Ändringar i `.github/` (workflows, mallar) är extra noga granskade

## Skärmdump

<!-- Valfritt – för ändringar i gränssnittet. Ta bort rubriken annars. -->
