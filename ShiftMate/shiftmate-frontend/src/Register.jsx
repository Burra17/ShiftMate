import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from './api';
import AuthLayout from './components/AuthLayout';
import { useToast } from './contexts/ToastContext';

const Register = () => {
    useEffect(() => {
        document.title = 'Registrera - ShiftMate';
    }, []);

    const navigate = useNavigate();
    const toast = useToast();

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [inviteCode, setInviteCode] = useState('');

    const [showPassword, setShowPassword] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        if (!inviteCode.trim()) {
            setError('Ange en inbjudningskod.');
            setLoading(false);
            return;
        }

        const payload = {
            firstName,
            lastName,
            email: email.trim().toLowerCase(),
            password,
            inviteCode: inviteCode.trim().toUpperCase()
        };

        try {
            await api.post('/Users/register', payload);
            toast.success("Konto skapat! Du omdirigeras nu till inloggningssidan.");
            navigate('/login');
        } catch (err) {
            console.error("Registreringsfel:", err.response || err);
            if (err.response?.data?.Message) {
                setError(err.response.data.Message);
            } else if (err.response?.data && typeof err.response.data === 'string') {
                setError(err.response.data);
            } else {
                setError("Nätverksfel eller så kunde inte servern nås.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <AuthLayout title="ShiftMate" subtitle="Skapa konto för att komma igång">
            <form onSubmit={handleSubmit} className="space-y-6">
                {error && <div className="bg-red-500/20 border border-red-500/30 text-red-300 text-xs font-semibold p-3 rounded-lg text-center">{error}</div>}

                <div className="flex flex-col sm:flex-row sm:space-x-4 space-y-6 sm:space-y-0">
                    <div className="space-y-2 sm:w-1/2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Förnamn</label>
                        <input
                            type="text"
                            required
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Alex"
                            value={firstName}
                            onChange={(e) => setFirstName(e.target.value)}
                        />
                    </div>
                    <div className="space-y-2 sm:w-1/2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Efternamn</label>
                        <input
                            type="text"
                            required
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Jones"
                            value={lastName}
                            onChange={(e) => setLastName(e.target.value)}
                        />
                    </div>
                </div>

                {/* Inbjudningskod */}
                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Inbjudningskod</label>
                    <input
                        type="text"
                        required
                        maxLength={8}
                        className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium uppercase tracking-[0.3em] text-center text-lg"
                        placeholder="XXXXXXXX"
                        value={inviteCode}
                        onChange={(e) => setInviteCode(e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, ''))}
                    />
                    <p className="text-[10px] text-slate-600 ml-1">Fråga din chef om inbjudningskoden till din organisation</p>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">E-post</label>
                    <input
                        type="email"
                        required
                        className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                        placeholder="namn@example.com"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Lösenord</label>
                    <div className="relative">
                        <input
                            type={showPassword ? 'text' : 'password'}
                            required
                            minLength={8}
                            className="w-full px-5 py-4 pr-12 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="••••••••"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        <button type="button" onClick={() => setShowPassword(!showPassword)} className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white transition-colors">
                            {showPassword ? (
                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94" /><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19" /><path d="M14.12 14.12a3 3 0 1 1-4.24-4.24" /><line x1="1" y1="1" x2="23" y2="23" /></svg>
                            ) : (
                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                            )}
                        </button>
                    </div>
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="w-full py-4 mt-2 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-black rounded-xl shadow-lg shadow-blue-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 tracking-wide uppercase text-sm"
                >
                    {loading ? (
                        <span className="flex items-center justify-center gap-2">
                            <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                            Skapar konto...
                        </span>
                    ) : 'Skapa Konto'}
                </button>
            </form>

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
