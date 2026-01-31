import { useState } from 'react';
import axios from 'axios';

const Login = ({ onLoginSuccess }) => {
    // Vi använder din uppdaterade e-post från databasen
    const [email, setEmail] = useState('andre.new@shiftmate.com');
    const [password, setPassword] = useState('dummy_hash_123');
    const [loading, setLoading] = useState(false);

    const handleLogin = async (e) => {
        e.preventDefault();
        setLoading(true);

        try {
            // Anropa din backend på localhost:7215
            const response = await axios.post('https://localhost:7215/api/Users/login', {
                email: email.trim(), // Tar bort mellanslag i början/slutet
                password: password
            });

            // Hämta token från svaret
            const token = response.data;

            // 1. Spara polletten säkert
            localStorage.setItem('token', token);

            // 2. Skicka användaren vidare
            onLoginSuccess();

        } catch (error) {
            console.error("Inloggningsfel:", error.response || error);

            // Ge tips om certifikat-felet dyker upp
            if (error.code === "ERR_NETWORK") {
                alert("Kunde inte nå servern! \n\nTIPS: Öppna https://localhost:7215/api/Users i en ny flik och godkänn säkerhetscertifikatet.");
            } else {
                alert("Fel e-post eller lösenord.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex flex-col justify-center bg-gray-50 px-6 py-12">
            <div className="sm:mx-auto sm:w-full sm:max-w-md bg-white p-10 rounded-[2.5rem] shadow-2xl shadow-orange-100 border border-gray-100">
                <div className="text-center">
                    <h2 className="text-4xl font-black text-gray-900 mb-2">ShiftMate</h2>
                    <p className="text-gray-500 font-medium mb-10">Välkommen tillbaka!</p>
                </div>

                <form onSubmit={handleLogin} className="space-y-5">
                    <input
                        type="email"
                        placeholder="Din e-post"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="w-full px-6 py-4 bg-gray-50 border-0 rounded-2xl focus:ring-2 focus:ring-okorange outline-none transition-all placeholder:text-gray-400"
                        required
                    />
                    <input
                        type="password"
                        placeholder="Lösenord"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full px-6 py-4 bg-gray-50 border-0 rounded-2xl focus:ring-2 focus:ring-okorange outline-none transition-all placeholder:text-gray-400"
                        required
                    />
                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full py-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-2xl shadow-lg shadow-orange-200 transition-all active:scale-95 disabled:opacity-50"
                    >
                        {loading ? 'Loggar in...' : 'Logga in'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default Login;