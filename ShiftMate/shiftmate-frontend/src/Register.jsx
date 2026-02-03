// Importerar nödvändiga React-hooks och komponenter.
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from './api'; // Importerar en förkonfigurerad Axios-instans för API-anrop.
import AuthLayout from './components/AuthLayout'; // En layout-komponent för autentiseringssidor.

// Registreringskomponent för nya användare.
const Register = () => {
    // Hook för att programmatiskt navigera användaren, t.ex. efter lyckad registrering.
    const navigate = useNavigate();
    
    // State-variabler för att hålla reda på formulärdata (förnamn, efternamn, etc.).
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    
    // State för att hantera laddnings- och felmeddelanden i UI.
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Funktion som körs när användaren skickar in formuläret.
    const handleSubmit = async (e) => {
        e.preventDefault(); // Förhindrar att webbläsaren laddar om sidan.
        setLoading(true); // Aktiverar laddningsindikator.
        setError(''); // Återställer eventuella tidigare felmeddelanden.

        // Skapar ett dataobjekt (payload) som ska skickas till API:et.
        const payload = { 
            firstName, 
            lastName, 
            email: email.trim().toLowerCase(), // Rensar och normaliserar e-postadressen.
            password 
        };

        try {
            // Gör ett asynkront API-anrop (POST) för att registrera användaren.
            await api.post('/Users/register', payload);
            alert("Konto skapat! Du omdirigeras nu till inloggningssidan.");
            navigate('/login'); // Omdirigerar användaren till inloggningssidan.

        } catch (err) {
            // Fångar upp och hanterar eventuella fel från API-anropet.
            console.error("Registreringsfel:", err.response || err);
            if (err.response?.data) {
                // Visar ett specifikt felmeddelande från servern om det finns.
                setError(err.response.data);
            } else {
                // Generiskt felmeddelande om servern inte kan nås.
                setError("Nätverksfel eller så kunde inte servern nås.");
            }
        } finally {
            // Denna kod körs alltid, oavsett om anropet lyckades eller misslyckades.
            setLoading(false); // Avaktiverar laddningsindikatorn.
        }
    };

    // Renderar JSX (HTML-liknande syntax) för komponenten.
    return (
        <AuthLayout title="ShiftMate" subtitle="Skapa konto för att komma igång">
            <form onSubmit={handleSubmit} className="space-y-6">
                {/* Visar ett felmeddelande om 'error'-state inte är tomt. */}
                {error && <div className="bg-red-500/20 border border-red-500/30 text-red-300 text-xs font-semibold p-3 rounded-lg text-center">{error}</div>}

                {/* Formulärsektion för för- och efternamn. */}
                <div className="flex space-x-4">
                    <div className="space-y-2 w-1/2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Förnamn</label>
                        <input
                            type="text"
                            required
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-purple-500/50 focus:border-purple-500 transition-all font-medium"
                            placeholder="Alex"
                            value={firstName}
                            onChange={(e) => setFirstName(e.target.value)}
                        />
                    </div>
                    <div className="space-y-2 w-1/2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Efternamn</label>
                        <input
                            type="text"
                            required
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-purple-500/50 focus:border-purple-500 transition-all font-medium"
                            placeholder="Jones"
                            value={lastName}
                            onChange={(e) => setLastName(e.target.value)}
                        />
                    </div>
                </div>

                {/* Formulärsektion för e-post. */}
                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">E-post</label>
                    <input
                        type="email"
                        required
                        className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-purple-500/50 focus:border-purple-500 transition-all font-medium"
                        placeholder="namn@okq8.se"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </div>

                {/* Formulärsektion för lösenord. */}
                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Lösenord</label>
                    <input
                        type="password"
                        required
                        minLength={8}
                        className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                        placeholder="••••••••"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </div>

                {/* Knapp för att skicka in formuläret. Inaktiveras under laddning. */}
                <button
                    type="submit"
                    disabled={loading}
                    className="w-full py-4 mt-2 bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 hover:from-indigo-500 hover:to-pink-500 text-white font-black rounded-xl shadow-lg shadow-purple-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 tracking-wide uppercase text-sm"
                >
                    {loading ? 'Jobbar...' : 'Skapa Konto'}
                </button>
            </form>

            {/* Länk till inloggningssidan för användare som redan har ett konto. */}
            <div className="mt-8 pt-6 border-t border-slate-800 text-center">
                <p className="text-slate-500 text-xs font-medium">
                    Har du redan ett konto?
                </p>
                <Link to="/login" className="text-xs font-bold text-blue-400 hover:text-blue-300 transition-colors uppercase tracking-widest">
                    Logga in här
                </Link>
            </div>
        </AuthLayout>
    );
};

export default Register;
