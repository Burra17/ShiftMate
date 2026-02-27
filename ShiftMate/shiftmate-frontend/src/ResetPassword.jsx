import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { resetPassword } from './api';
import AuthLayout from './components/AuthLayout';

const ResetPassword = () => {
    useEffect(() => {
        document.title = 'Nytt lösenord - ShiftMate';
    }, []);

    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const token = searchParams.get('token') || '';
    const email = searchParams.get('email') || '';

    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (password.length < 8) {
            setError('Lösenordet måste vara minst 8 tecken.');
            return;
        }

        if (password !== confirmPassword) {
            setError('Lösenorden matchar inte.');
            return;
        }

        if (!token || !email) {
            setError('Ogiltig återställningslänk. Försök begära en ny.');
            return;
        }

        setLoading(true);

        try {
            await resetPassword(token, email, password);
            setSuccess(true);
            setTimeout(() => navigate('/login'), 3000);
        } catch (err) {
            if (err.code === 'ERR_NETWORK') {
                setError('Kunde inte nå servern. Kontrollera att den är igång.');
            } else {
                setError(err.response?.data?.message || err.response?.data?.Message || 'Något gick fel. Försök igen.');
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <AuthLayout title="ShiftMate" subtitle="Välj ett nytt lösenord">
            {success ? (
                <div className="space-y-6">
                    <div className="bg-green-500/20 border border-green-500/30 text-green-300 text-xs font-semibold p-4 rounded-lg text-center">
                        Lösenordet har återställts! Du skickas till inloggningen...
                    </div>
                    <div className="text-center">
                        <Link to="/login" className="text-xs font-bold text-blue-400 hover:text-blue-300 transition-colors uppercase tracking-widest">
                            Gå till inloggning
                        </Link>
                    </div>
                </div>
            ) : (
                <form onSubmit={handleSubmit} className="space-y-6">
                    {error && <div className="bg-red-500/20 border border-red-500/30 text-red-300 text-xs font-semibold p-3 rounded-lg text-center">{error}</div>}

                    <div className="space-y-2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Nytt lösenord</label>
                        <input
                            type="password"
                            required
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Minst 8 tecken"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>

                    <div className="space-y-2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Bekräfta lösenord</label>
                        <input
                            type="password"
                            required
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Upprepa lösenordet"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full py-4 mt-2 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-black rounded-xl shadow-lg shadow-blue-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 tracking-wide uppercase text-sm"
                    >
                        {loading ? 'Sparar...' : 'Spara nytt lösenord'}
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

export default ResetPassword;
