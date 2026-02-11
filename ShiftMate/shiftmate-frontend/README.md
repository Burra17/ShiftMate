# ShiftMate Frontend

React-klienten för ShiftMate - en applikation för skiftplanering och hantering av skiftbyten.

## Teknikstack

- **React 19** - Funktionella komponenter med Hooks
- **Vite 7** - Byggverktyg med HMR (Hot Module Replacement)
- **Tailwind CSS 4** - Utility-first CSS med Neon Dark-tema (`bg-slate-950`, `text-blue-400`)
- **Axios** - HTTP-klient med JWT-interceptor för automatisk autentisering
- **React Router v7** - Klientbaserad routing

## Komma igång

```bash
# Installera beroenden
npm install

# Starta utvecklingsserver (http://localhost:5173)
npm run dev

# Bygg för produktion
npm run build

# Förhandsgranska produktionsbygge
npm run preview
```

## Miljövariabler

- `.env.development` - API-URL för lokal utveckling (`https://localhost:7215/api`)
- `.env.production` - API-URL för produktion (`https://shiftmate-vow0.onrender.com/api`)

## Sidor

| Sida | Fil | Beskrivning |
|------|-----|-------------|
| Inloggning | `Login.jsx` | Inloggning med e-post och lösenord |
| Registrering | `Register.jsx` | Skapa nytt konto |
| Mina Pass | `ShiftList.jsx` | Användarens skift och inkommande bytesförfrågningar |
| Lediga Pass | `MarketPlace.jsx` | Otilldelade och erbjudna skift att ta |
| Schema | `Schedule.jsx` | Komplett schemaöversikt grupperad per datum |
| Profil | `Profile.jsx` | Användarinformation och statistik |
| Admin Panel | `components/AdminPanel.jsx` | Skapa skift och tilldela användare (admin-only) |

## Filstruktur

```
shiftmate-frontend/
├── .env.development          # API-URL för utveckling
├── .env.production           # API-URL för produktion
├── eslint.config.js          # ESLint-regler
├── index.html                # HTML-ingång
├── package.json              # Beroenden och skript
├── postcss.config.js         # PostCSS-konfiguration
├── tailwind.config.js        # Tailwind CSS-anpassning
├── vercel.json               # Vercel-driftsättning
├── vite.config.js            # Vite-konfiguration
├── public/
│   └── vite.svg
└── src/
    ├── main.jsx              # React-ingångspunkt (ReactDOM.createRoot)
    ├── App.jsx               # Huvudrouting, autentiseringskontroll, sidebar
    ├── api.js                # Axios-instans med JWT-interceptor och hjälpfunktioner
    ├── index.css             # Globala Tailwind-stilar
    ├── App.css               # Komponentspecifika stilar
    ├── Login.jsx             # Inloggningssida
    ├── Register.jsx          # Registreringssida
    ├── ShiftList.jsx         # Mina pass + bytesförfrågningar
    ├── MarketPlace.jsx       # Lediga pass (marknadsplats)
    ├── Schedule.jsx          # Schemaöversikt
    ├── Profile.jsx           # Profil och statistik
    ├── assets/
    │   └── react.svg
    └── components/
        ├── AuthLayout.jsx    # Delad layout för inloggning/registrering
        └── AdminPanel.jsx    # Admin-panel för skifthantering
```

## Autentisering

- JWT-token lagras i `localStorage`
- Axios-interceptor bifogar automatiskt `Authorization: Bearer {token}` till alla anrop
- Vid 401-svar loggas användaren ut automatiskt
- Rollkontroll (Admin/Employee) sker via avkodning av JWT-token direkt i klienten

## Tema

Applikationen använder ett Neon Dark-tema med färgerna:
- Bakgrund: `bg-slate-950`
- Primär text: `text-blue-400`
- Kanter: `border-blue-500/30`
- Accent: `text-cyan-400`, `text-green-400`
