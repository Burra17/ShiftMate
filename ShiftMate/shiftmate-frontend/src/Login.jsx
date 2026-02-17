// importera nödvändiga funktioner och komponenter
import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from './api'; // Api-instans för att göra anrop till backend
import AuthLayout from './components/AuthLayout'; // Gemensam layout för autentiseringssidor

// Komponent för inloggningssidan.
// Tar emot onLoginSuccess som en prop, vilken anropas vid lyckad inloggning.
const Login = ({ onLoginSuccess }) => {
    // Sätt sidtiteln
    useEffect(() => {
        document.title = 'Logga in - ShiftMate';
    }, []);

    // State-variabler för att hantera formulärdata, laddningsstatus och felmeddelanden.
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Funktion som hanterar formulärinskickning.
    const handleSubmit = async (e) => {
        e.preventDefault(); // Förhindrar standardbeteendet för formuläret (att ladda om sidan).
        setLoading(true); // Indikerar att en process har startat.
        setError(''); // Återställer eventuella tidigare felmeddelanden.

        // Förbereder payload med användardata.
        // E-post trimmas och konverteras till gemener för konsekvens.
        const payload = { 
            email: email.trim().toLowerCase(), 
            password 
        };

        try {
            // Skickar en POST-förfrågan till /Users/login-slutpunkten med användardata.
            const response = await api.post('/Users/login', payload);
            const token = response.data.token; // Extraherar JWT-token från svaret.
            
            // Sparar token i webbläsarens localStorage för att bibehålla inloggningsstatus.
            localStorage.setItem('token', token);
            
            // Anropar onLoginSuccess för att meddela förälderkomponenten om lyckad inloggning.
            onLoginSuccess();

        } catch (err) {
            // Loggar felet till konsolen för felsökning.
            console.error("Inloggningsfel:", err.response || err);

            // Hanterar olika typer av fel för att ge användaren relevant feedback.
            if (err.code === "ERR_NETWORK") {
                setError("Kunde inte nå servern. Kontrollera att den är igång.");
            } else if (err.response?.status === 401) {
                setError("Fel e-post eller lösenord."); // Felaktiga inloggningsuppgifter.
            } else {
                setError("Något gick fel vid inloggning."); // Generiskt felmeddelande.
            }
        } finally {
            // Återställer laddningsstatus oavsett om inloggningen lyckades eller misslyckades.
            setLoading(false);
        }
    };

    return (
        // Använder AuthLayout för en konsekvent design på autentiseringssidor.
        <AuthLayout title="ShiftMate" subtitle="Välkommen tillbaka!">
            <form onSubmit={handleSubmit} className="space-y-6">
                {/* Visar felmeddelande om ett sådant finns. */}
                {error && <div className="bg-red-500/20 border border-red-500/30 text-red-300 text-xs font-semibold p-3 rounded-lg text-center">{error}</div>}

                {/* Fält för e-post. */}
                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">E-post</label>
                    <input
                        type="email"
                        required
                        className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                        placeholder="namn@okq8.se"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </div>

                {/* Fält för lösenord. */}
                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Lösenord</label>
                    <input
                        type="password"
                        required
                        className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                        placeholder="••••••••"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </div>

                {/* Inloggningsknapp. Inaktiveras när laddningsstatus är true. */}
                <button
                    type="submit"
                    disabled={loading}
                    className="w-full py-4 mt-2 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-black rounded-xl shadow-lg shadow-blue-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 tracking-wide uppercase text-sm"
                >
                    {loading ? 'Jobbar...' : 'Logga In'}
                </button>
            </form>

            {/* Länk till registreringssidan för nya användare. */}
            <div className="mt-8 pt-6 border-t border-slate-800 text-center">
                <p className="text-slate-500 text-xs mb-3 font-medium">
                    Ny på macken?
                </p>
                <Link to="/register" className="text-xs font-bold text-blue-400 hover:text-blue-300 transition-colors uppercase tracking-widest">
                    Registrera nytt konto
                </Link>
            </div>
        </AuthLayout>
    );
};

export default Login;