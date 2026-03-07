import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import api from './api';
import AuthLayout from './components/AuthLayout';

const VerifyEmail = () => {
    useEffect(() => {
        document.title = 'Verifiera e-post - ShiftMate';
    }, []);

    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState('loading'); // loading, success, error
    const [message, setMessage] = useState('');

    useEffect(() => {
        const token = searchParams.get('token');
        const email = searchParams.get('email');

        if (!token || !email) {
            setStatus('error');
            setMessage('Ogiltig verifieringslänk.');
            return;
        }

        const verify = async () => {
            try {
                const response = await api.post('/Users/verify-email', { token, email });
                setStatus('success');
                setMessage(response.data.Message || 'E-postadressen har verifierats!');
            } catch (err) {
                setStatus('error');
                const data = err.response?.data;
                setMessage(data?.Message || 'Något gick fel vid verifieringen.');
            }
        };

        verify();
    }, [searchParams]);

    return (
        <AuthLayout title="ShiftMate" subtitle="E-postverifiering">
            <div className="text-center space-y-6">
                {status === 'loading' && (
                    <div className="flex flex-col items-center gap-4">
                        <span className="w-8 h-8 border-3 border-blue-400/30 border-t-blue-400 rounded-full animate-spin" />
                        <p className="text-slate-400 text-sm font-medium">Verifierar din e-post...</p>
                    </div>
                )}

                {status === 'success' && (
                    <div className="space-y-4">
                        <div className="w-16 h-16 mx-auto bg-green-500/20 rounded-full flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-green-400">
                                <path d="M20 6 9 17l-5-5" />
                            </svg>
                        </div>
                        <p className="text-green-300 text-sm font-semibold">{message}</p>
                    </div>
                )}

                {status === 'error' && (
                    <div className="space-y-4">
                        <div className="w-16 h-16 mx-auto bg-red-500/20 rounded-full flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-red-400">
                                <circle cx="12" cy="12" r="10" /><line x1="15" y1="9" x2="9" y2="15" /><line x1="9" y1="9" x2="15" y2="15" />
                            </svg>
                        </div>
                        <p className="text-red-300 text-sm font-semibold">{message}</p>
                    </div>
                )}

                {status !== 'loading' && (
                    <Link
                        to="/login"
                        className="inline-block px-8 py-3 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-bold rounded-xl shadow-lg shadow-blue-900/40 transition-all hover:scale-[1.02] active:scale-[0.98] tracking-wide uppercase text-xs"
                    >
                        Gå till inloggning
                    </Link>
                )}
            </div>
        </AuthLayout>
    );
};

export default VerifyEmail;
