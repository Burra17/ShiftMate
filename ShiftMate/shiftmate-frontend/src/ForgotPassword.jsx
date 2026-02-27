import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { forgotPassword } from './api';
import AuthLayout from './components/AuthLayout';

const ForgotPassword = () => {
    useEffect(() => {
        document.title = 'Glömt lösenord - ShiftMate';
    }, []);

    const [email, setEmail] = useState('');
    const [loading, setLoading] = useState(false);
    const [sent, setSent] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            await forgotPassword(email.trim().toLowerCase());
            setSent(true);
        } catch (err) {
            if (err.code === 'ERR_NETWORK') {
                setError('Kunde inte nå servern. Kontrollera att den är igång.');
            } else {
                setError('Något gick fel. Försök igen.');
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <AuthLayout title="ShiftMate" subtitle="Återställ ditt lösenord">
            {sent ? (
                <div className="space-y-6">
                    <div className="bg-green-500/20 border border-green-500/30 text-green-300 text-xs font-semibold p-4 rounded-lg text-center">
                        Om e-postadressen finns i systemet har vi skickat en återställningslänk. Kolla din inkorg!
                    </div>
                </div>
            ) : (
                <form onSubmit={handleSubmit} className="space-y-6">
                    {error && <div className="bg-red-500/20 border border-red-500/30 text-red-300 text-xs font-semibold p-3 rounded-lg text-center">{error}</div>}

                    <p className="text-slate-400 text-xs font-medium">
                        Ange din e-postadress så skickar vi en länk för att återställa ditt lösenord.
                    </p>

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

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full py-4 mt-2 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-black rounded-xl shadow-lg shadow-blue-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 tracking-wide uppercase text-sm"
                    >
                        {loading ? (
                            <span className="flex items-center justify-center gap-2">
                                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                                Skickar...
                            </span>
                        ) : 'Skicka återställningslänk'}
                    </button>
                </form>
            )}

            <div className="mt-8 pt-6 border-t border-slate-800 text-center">
                <Link to="/login" className="text-xs font-bold text-blue-400 hover:text-blue-300 transition-colors uppercase tracking-widest">
                    Tillbaka till inloggning
                </Link>
            </div>
        </AuthLayout>
    );
};

export default ForgotPassword;
