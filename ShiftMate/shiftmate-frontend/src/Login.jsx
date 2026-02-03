import { useState } from 'react';
import api from './api';

const Login = ({ onLoginSuccess }) => {
    // UI-state för att växla mellan login/register i designen
    const [isRegistering, setIsRegistering] = useState(false);

    // Dina fungerande states
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);

        // Välj rätt URL beroende på om vi loggar in eller registrerar
        const url = isRegistering
            ? '/Users/register' // Antagen register-endpoint (kan behöva ändras om den heter annat)
            : '/Users/login';   // DIN FUNGERANDE LOGIN-URL

        try {
            const response = await api.post(url, {
                email: email.trim(), // Tar bort mellanslag precis som i din fungerande kod
                password: password
            });

            if (!isRegistering) {
                // Spara token och logga in (precis som förut)
                const token = response.data.token;
                localStorage.setItem('token', token);
                onLoginSuccess();
            } else {
                alert("Konto skapat! Byt till 'Logga in' för att komma in.");
                setIsRegistering(false);
            }

        } catch (error) {
            console.error("Inloggningsfel:", error.response || error);

            // Din specifika felhantering för certifikat (viktigt för localhost)
            if (error.code === "ERR_NETWORK") {
                alert("Kunde inte nå servern! \n\nTIPS: Öppna https://localhost:7215/api/Users i en ny flik och godkänn säkerhetscertifikatet.");
            } else if (error.response?.status === 400 || error.response?.status === 401) {
                alert("Fel e-post eller lösenord.");
            } else {
                alert(`Något gick fel: ${error.response?.data || error.message}`);
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-950 relative overflow-hidden font-sans selection:bg-purple-500 selection:text-white">

            {/* Bakgrunds-effekter (Futuristiska blobs) */}
            <div className="absolute top-[-20%] left-[-10%] w-[500px] h-[500px] bg-purple-600/20 rounded-full blur-[120px] animate-pulse duration-[4000ms]"></div>
            <div className="absolute bottom-[-20%] right-[-10%] w-[500px] h-[500px] bg-blue-600/20 rounded-full blur-[120px] animate-pulse duration-[5000ms]"></div>

            {/* Själva kortet med glas-effekt */}
            <div className="w-full max-w-md bg-slate-900/60 backdrop-blur-2xl border border-slate-800 p-8 md:p-12 rounded-3xl shadow-2xl relative z-10 animate-in fade-in zoom-in duration-500 mx-4">

                <div className="text-center mb-10">
                    <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-indigo-500 via-purple-500 to-pink-500 rounded-2xl mb-6 shadow-lg shadow-purple-500/30 animate-bounce duration-[3000ms]">
                        <span className="text-3xl filter drop-shadow-md">⛽</span>
                    </div>
                    <h1 className="text-4xl font-black text-white tracking-tight mb-2">ShiftMate</h1>
                    <p className="text-slate-400 text-sm font-medium tracking-wide">
                        {isRegistering ? 'Skapa konto för att komma igång' : 'Välkommen tillbaka!'}
                    </p>
                </div>

                <form onSubmit={handleSubmit} className="space-y-6">
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

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full py-4 mt-2 bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 hover:from-indigo-500 hover:to-pink-500 text-white font-black rounded-xl shadow-lg shadow-purple-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 tracking-wide uppercase text-sm"
                    >
                        {loading ? 'Jobbar...' : isRegistering ? 'SKAPA KONTO' : 'LOGGA IN'}
                    </button>
                </form>

                <div className="mt-8 pt-6 border-t border-slate-800 text-center">
                    <p className="text-slate-500 text-xs mb-3 font-medium">
                        {isRegistering ? 'Har du redan ett konto?' : 'Ny på macken?'}
                    </p>
                    <button
                        onClick={() => setIsRegistering(!isRegistering)}
                        className="text-xs font-bold text-blue-400 hover:text-blue-300 transition-colors uppercase tracking-widest"
                    >
                        {isRegistering ? 'Logga in här' : 'Registrera nytt konto'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default Login;