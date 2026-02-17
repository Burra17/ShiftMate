import { useState, useEffect } from 'react';
import api from '../api';
import { useToast } from '../contexts/ToastContext';

// Snabbval för vanliga passtyper
const QUICK_SHIFTS = [
    { label: 'Öppning', start: '05:45', end: '13:00', icon: '🌅' },
    { label: 'Örjan', start: '06:13', end: '15:00', icon: '☀️' },
    { label: 'Dagpass', start: '11:00', end: '20:00', icon: '🕐' },
    { label: 'Kvällspass', start: '14:00', end: '22:15', icon: '🌙' },
];

// Beräkna passtid i timmar från start- och sluttid
const calcDuration = (start, end) => {
    if (!start || !end) return null;
    const [sh, sm] = start.split(':').map(Number);
    const [eh, em] = end.split(':').map(Number);
    let mins = (eh * 60 + em) - (sh * 60 + sm);
    if (mins <= 0) mins += 24 * 60; // Hanterar nattpass som passerar midnatt
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return m > 0 ? `${h}h ${m}min` : `${h}h`;
};

const AdminPanel = () => {
    const [date, setDate] = useState('');
    const [startTime, setStartTime] = useState('');
    const [endTime, setEndTime] = useState('');
    const [userId, setUserId] = useState('');
    const [activeQuick, setActiveQuick] = useState(null);

    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(false);
    const toast = useToast();

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

    // Hantera snabbval — fyller i start/sluttid automatiskt
    const handleQuickSelect = (quick) => {
        setStartTime(quick.start);
        setEndTime(quick.end);
        setActiveQuick(quick.label);
    };

    // Rensa snabbval-markering om användaren ändrar tid manuellt
    const handleStartChange = (val) => {
        setStartTime(val);
        setActiveQuick(null);
    };
    const handleEndChange = (val) => {
        setEndTime(val);
        setActiveQuick(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);

        try {
            // Bygg ihop datum + tid till datetime-strängar
            // Om nattpass (slut < start) sätter vi slutdatum till nästa dag
            let endDate = date;
            if (endTime <= startTime) {
                const next = new Date(date);
                next.setDate(next.getDate() + 1);
                endDate = next.toISOString().split('T')[0];
            }

            const payload = {
                startTime: `${date}T${startTime}`,
                endTime: `${endDate}T${endTime}`,
                userId: userId === '' ? null : userId
            };

            await api.post('/Shifts/admin', payload);

            toast.success("Passet har skapats!");

            // Återställ formuläret efter lyckad skapning
            setDate('');
            setStartTime('');
            setEndTime('');
            setUserId('');
            setActiveQuick(null);

        } catch (err) {
            const errorMsg = err.response?.data?.message || "Något gick fel.";
            toast.error(errorMsg);
        } finally {
            setLoading(false);
        }
    };

    const duration = calcDuration(startTime, endTime);

    return (
        <div className="bg-slate-900/80 backdrop-blur-xl p-6 md:p-8 rounded-3xl border border-slate-800 shadow-2xl relative overflow-hidden">
            <div className="absolute top-0 right-0 w-64 h-64 bg-purple-500/10 rounded-full blur-3xl -mr-32 -mt-32 pointer-events-none"></div>

            <form onSubmit={handleSubmit} className="space-y-6 relative z-10">

                {/* Datum */}
                <div className="space-y-2">
                    <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Datum</label>
                    <input
                        type="date"
                        value={date}
                        onChange={(e) => setDate(e.target.value)}
                        required
                        className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all"
                    />
                </div>

                {/* Snabbval */}
                <div className="space-y-2">
                    <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Snabbval</label>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                        {QUICK_SHIFTS.map((quick) => (
                            <button
                                key={quick.label}
                                type="button"
                                onClick={() => handleQuickSelect(quick)}
                                className={`py-3 px-2 rounded-xl text-xs font-black uppercase tracking-wider transition-all border
                                    ${activeQuick === quick.label
                                        ? 'bg-purple-600/20 border-purple-500 text-purple-300 shadow-[0_0_15px_rgba(168,85,247,0.2)]'
                                        : 'bg-slate-800/50 border-slate-700 text-slate-400 hover:border-slate-600 hover:text-white'
                                    }`}
                            >
                                <span className="block text-lg mb-1">{quick.icon}</span>
                                {quick.label}
                                <span className="block text-[10px] text-slate-500 font-medium mt-0.5">
                                    {quick.start.replace(':', '.')} - {quick.end.replace(':', '.')}
                                </span>
                            </button>
                        ))}
                    </div>
                </div>

                {/* Start- och sluttid */}
                <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                        <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Starttid</label>
                        <input
                            type="time"
                            value={startTime}
                            onChange={(e) => handleStartChange(e.target.value)}
                            required
                            className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all"
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Sluttid</label>
                        <input
                            type="time"
                            value={endTime}
                            onChange={(e) => handleEndChange(e.target.value)}
                            required
                            className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all"
                        />
                    </div>
                </div>

                {/* Förhandsgranskning av längd */}
                {duration && (
                    <div className="flex items-center gap-2 px-1">
                        <span className="w-2 h-2 rounded-full bg-purple-400 shadow-[0_0_8px_#a855f7]"></span>
                        <span className="text-xs font-bold text-purple-300">Passlängd: {duration}</span>
                    </div>
                )}

                {/* Välj Personal */}
                <div className="space-y-2">
                    <label className="text-xs font-bold text-slate-500 uppercase tracking-widest ml-1">Tilldela Personal</label>
                    <select
                        value={userId}
                        onChange={(e) => setUserId(e.target.value)}
                        className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-3 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all appearance-none"
                    >
                        <option value="">Öppet pass (ingen ägare)</option>
                        <option disabled>──────────</option>
                        {users.map(user => (
                            <option key={user.id} value={user.id}>
                                {user.firstName} {user.lastName}
                            </option>
                        ))}
                    </select>
                    <p className="text-xs text-slate-500 ml-1">Lämnas det tomt hamnar passet direkt på Lediga pass.</p>
                </div>

                {/* Knapp */}
                <button
                    type="submit"
                    disabled={loading}
                    className={`w-full py-4 rounded-xl font-black uppercase tracking-widest transition-all shadow-lg text-sm
                        ${loading
                            ? 'bg-slate-700 text-slate-500 cursor-not-allowed'
                            : 'bg-gradient-to-r from-purple-600 to-indigo-600 text-white hover:shadow-purple-500/25 hover:scale-[1.02] active:scale-[0.98]'
                        }`}
                >
                    {loading ? 'Skapar...' : 'Skapa pass'}
                </button>

            </form>
        </div>
    );
};

export default AdminPanel;
