import { useState, useEffect } from 'react';
import api from '../api';

const AdminPanel = () => {
    // Formulär-data
    const [formData, setFormData] = useState({
        startTime: '',
        endTime: '',
        userId: '' // Tom sträng = Öppet pass
    });

    const [users, setUsers] = useState([]); // Listan från backend
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState({ text: '', type: '' });

    // Hämta användare när sidan laddas
    useEffect(() => {
        const fetchUsers = async () => {
            try {
                const response = await api.get('/Users');
                setUsers(response.data);
            } catch (err) {
                console.error("Kunde inte hämta användare:", err);
            }
        };
        fetchUsers();
    }, []);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setMessage({ text: '', type: '' });

        try {
            // Skapa payload
            // Om userId är tomt ("") skickar vi null, annars ID:t
            const payload = {
                startTime: formData.startTime,
                endTime: formData.endTime,
                userId: formData.userId === "" ? null : formData.userId
            };

            // Anropa din endpoint (Vi använder /shifts/admin som du skapade tidigare)
            await api.post('/Shifts/admin', payload);

            setMessage({ text: '✅ Passet har skapats!', type: 'success' });

            // Återställ formuläret om du vill
            // setFormData({ startTime: '', endTime: '', userId: '' });

        } catch (err) {
            const errorMsg = err.response?.data?.message || "Något gick fel.";
            setMessage({ text: `❌ ${errorMsg}`, type: 'error' });
        } finally {
            setLoading(false);
        }
    };

    return (
            <div className="bg-slate-900/80 backdrop-blur-xl p-8 rounded-3xl border border-slate-800 shadow-2xl relative overflow-hidden">
                {/* Lila bakgrunds-glow */}
                <div className="absolute top-0 right-0 w-64 h-64 bg-purple-500/10 rounded-full blur-3xl -mr-32 -mt-32 pointer-events-none"></div>

                <form onSubmit={handleSubmit} className="space-y-6 relative z-10">

                    {/* Tider */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2">
                            <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Starttid</label>
                            <input
                                type="datetime-local"
                                name="startTime"
                                value={formData.startTime}
                                onChange={handleChange}
                                required
                                className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-purple-500 focus:ring-1 focus:ring-purple-500 transition-all"
                            />
                        </div>

                        <div className="space-y-2">
                            <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Sluttid</label>
                            <input
                                type="datetime-local"
                                name="endTime"
                                value={formData.endTime}
                                onChange={handleChange}
                                required
                                className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-purple-500 focus:ring-1 focus:ring-purple-500 transition-all"
                            />
                        </div>
                    </div>

                    {/* Välj Personal */}
                    <div className="space-y-2">
                        <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Tilldela Personal</label>
                        <select
                            name="userId"
                            value={formData.userId}
                            onChange={handleChange}
                            className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-purple-500 focus:ring-1 focus:ring-purple-500 transition-all appearance-none"
                        >
                            <option value="">✨ ÖPPET PASS (Ingen ägare)</option>
                            <option disabled>──────────</option>
                            {users.map(user => (
                                <option key={user.id} value={user.id}>
                                    👤 {user.firstName} {user.lastName}
                                </option>
                            ))}
                        </select>
                        <p className="text-xs text-slate-500 ml-1">Lämnas det tomt hamnar passet direkt på Lediga pass.</p>
                    </div>

                    {/* Knapp */}
                    <button
                        type="submit"
                        disabled={loading}
                        className={`w-full py-4 rounded-xl font-black uppercase tracking-widest transition-all shadow-lg
                            ${loading
                                ? 'bg-slate-700 text-slate-500 cursor-not-allowed'
                                : 'bg-gradient-to-r from-purple-600 to-indigo-600 text-white hover:shadow-purple-500/25 hover:scale-[1.02] active:scale-[0.98]'
                            }`}
                    >
                        {loading ? 'Skapar...' : '🚀 SKAPA PASS'}
                    </button>

                    {/* Feedback Meddelande */}
                    {message.text && (
                        <div className={`p-4 rounded-xl text-center font-bold text-sm border animate-pulse ${message.type === 'success'
                                ? 'bg-green-500/10 text-green-400 border-green-500/20'
                                : 'bg-red-500/10 text-red-400 border-red-500/20'
                            }`}>
                            {message.text}
                        </div>
                    )}
                </form>
            </div>
    );
};

export default AdminPanel;