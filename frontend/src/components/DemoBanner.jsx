import { isDemo } from '../api';

const REPO_URL = 'https://github.com/Burra17/ShiftMate';

// Smal rad överst i appen som bara visas för demo-användare (claim IsDemo i token).
const DemoBanner = () => {
  if (!isDemo()) return null;

  return (
    <div
      role="note"
      className="flex flex-wrap items-center justify-center gap-x-3 gap-y-1 px-4 py-2 bg-blue-500/10 border-b border-blue-500/30 text-xs font-medium text-blue-300 text-center"
    >
      <span>
        Du testar en demo – ändringar raderas automatiskt. Sessionen gäller i en
        timme.
      </span>
      <a
        href={REPO_URL}
        target="_blank"
        rel="noopener noreferrer"
        className="font-bold text-blue-400 hover:text-blue-300 underline underline-offset-2"
      >
        Se koden på GitHub ↗
      </a>
    </div>
  );
};

export default DemoBanner;
