import { useState } from 'react';
import { createManagerShift } from '../../api';
import { useToast } from '../../contexts/ToastContext';
import DatePicker from '../ui/DatePicker';
import TimePicker from '../ui/TimePicker';
import UserSelect from '../ui/UserSelect';

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
    if (mins <= 0) mins += 24 * 60;
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return m > 0 ? `${h}h ${m}min` : `${h}h`;
};

const ShiftForm = ({ users }) => {
    const toast = useToast();
    const [date, setDate] = useState('');
    const [startTime, setStartTime] = useState('');
    const [endTime, setEndTime] = useState('');
    const [userId, setUserId] = useState('');
    const [activeQuick, setActiveQuick] = useState(null);
    const [loading, setLoading] = useState(false);

    const handleQuickSelect = (quick) => {
        setStartTime(quick.start);
        setEndTime(quick.end);
        setActiveQuick(quick.label);
    };

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
            let endDate = date;
            if (endTime <= startTime) {
                const next = new Date(date);
                next.setDate(next.getDate() + 1);
                endDate = next.toISOString().split('T')[0];
            }

            const localStart = new Date(`${date}T${startTime}`);
            const localEnd = new Date(`${endDate}T${endTime}`);

            await createManagerShift({
                startTime: localStart.toISOString(),
                endTime: localEnd.toISOString(),
                userId: userId === '' ? null : userId
            });

            toast.success("Passet har skapats!");
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
        <div className="animate-fade-up">
            <form onSubmit={handleSubmit} className="space-y-5">

                {/* ── Steg 1: Datum + Snabbval ── */}
                <div className="bg-slate-900/50 border border-slate-800/60 rounded-2xl p-5 space-y-5 overflow-visible relative z-20">
                    <div className="flex items-center gap-2 mb-1">
                        <div className="w-5 h-5 rounded-md bg-purple-500/15 border border-purple-500/25 flex items-center justify-center">
                            <span className="text-[10px] font-extrabold text-purple-400">1</span>
                        </div>
                        <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">Välj datum & tid</span>
                    </div>

                    <DatePicker label="Datum" value={date} onChange={setDate} required />

                    {/* Snabbval */}
                    <div className="space-y-1.5">
                        <label className="text-[11px] font-semibold text-slate-500 ml-1">Snabbval</label>
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                            {QUICK_SHIFTS.map((quick) => (
                                <button
                                    key={quick.label}
                                    type="button"
                                    onClick={() => handleQuickSelect(quick)}
                                    className={`py-3 px-2 rounded-xl text-xs font-bold transition-all border group
                                        ${activeQuick === quick.label
                                            ? 'bg-purple-500/15 border-purple-500/40 text-purple-300 shadow-[0_0_20px_rgba(168,85,247,0.12)]'
                                            : 'bg-slate-800/40 border-slate-700/50 text-slate-400 hover:border-slate-600 hover:text-slate-200'
                                        }`}
                                >
                                    <span className="block text-base mb-1 group-hover:scale-110 transition-transform">{quick.icon}</span>
                                    <span className="uppercase tracking-wider">{quick.label}</span>
                                    <span className="block text-[10px] text-slate-500 font-medium mt-0.5">
                                        {quick.start} – {quick.end}
                                    </span>
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Start- och sluttid */}
                    <div className="grid grid-cols-2 gap-3">
                        <TimePicker label="Starttid" value={startTime} onChange={handleStartChange} required />
                        <TimePicker label="Sluttid" value={endTime} onChange={handleEndChange} required />
                    </div>

                    {duration && (
                        <div className="flex items-center gap-2 bg-purple-500/8 border border-purple-500/15 rounded-lg px-3 py-2">
                            <span className="w-1.5 h-1.5 rounded-full bg-purple-400 shadow-[0_0_6px_#a855f7]"></span>
                            <span className="text-xs font-semibold text-purple-300">Passlängd: {duration}</span>
                        </div>
                    )}
                </div>

                {/* ── Steg 2: Tilldela personal ── */}
                <div className="bg-slate-900/50 border border-slate-800/60 rounded-2xl p-5 space-y-3 overflow-visible relative z-10">
                    <div className="flex items-center gap-2 mb-1">
                        <div className="w-5 h-5 rounded-md bg-purple-500/15 border border-purple-500/25 flex items-center justify-center">
                            <span className="text-[10px] font-extrabold text-purple-400">2</span>
                        </div>
                        <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">Tilldela personal</span>
                    </div>

                    <UserSelect users={users} value={userId} onChange={setUserId} />
                    <p className="text-[11px] text-slate-500 ml-1">Lämnas det tomt hamnar passet direkt på Lediga pass.</p>
                </div>

                {/* ── Skapa-knapp ── */}
                <button
                    type="submit"
                    disabled={loading}
                    className={`w-full py-4 rounded-xl font-bold uppercase tracking-wider transition-all text-sm
                        ${loading
                            ? 'bg-slate-800 text-slate-500 cursor-not-allowed'
                            : 'bg-gradient-to-r from-purple-600 to-indigo-600 text-white shadow-lg shadow-purple-900/30 hover:shadow-purple-500/20 hover:scale-[1.02] active:scale-[0.98]'
                        }`}
                >
                    {loading ? (
                        <span className="flex items-center justify-center gap-2">
                            <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                            Skapar...
                        </span>
                    ) : 'Skapa pass'}
                </button>

            </form>
        </div>
    );
};

export default ShiftForm;
