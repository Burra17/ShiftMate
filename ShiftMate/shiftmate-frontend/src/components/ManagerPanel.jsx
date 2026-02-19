import { useState, useEffect } from 'react';
import {
    fetchAllUsers,
    deleteUser,
    updateUserRole,
    getCurrentUserId,
    createManagerShift,
    fetchShifts,
    updateShift,
    deleteShift
} from '../api';
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

// Beräkna passtid i timmar från Date-objekt
const calcDurationFromDates = (start, end) => {
    const ms = new Date(end) - new Date(start);
    const mins = Math.round(ms / 60000);
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return m > 0 ? `${h}h ${m}min` : `${h}h`;
};

const ManagerPanel = () => {
    // Flikval: 'shifts', 'users' eller 'allShifts'
    const [activeTab, setActiveTab] = useState('shifts');

    // State för pass-formuläret
    const [date, setDate] = useState('');
    const [startTime, setStartTime] = useState('');
    const [endTime, setEndTime] = useState('');
    const [userId, setUserId] = useState('');
    const [activeQuick, setActiveQuick] = useState(null);
    const [loading, setLoading] = useState(false);

    // State för användarlistan
    const [users, setUsers] = useState([]);
    const [updatingUserId, setUpdatingUserId] = useState(null);

    // State för "Alla Pass"-fliken
    const [allShifts, setAllShifts] = useState([]);
    const [shiftsLoading, setShiftsLoading] = useState(false);
    const [editingShiftId, setEditingShiftId] = useState(null);
    const [editDate, setEditDate] = useState('');
    const [editStartTime, setEditStartTime] = useState('');
    const [editEndTime, setEditEndTime] = useState('');
    const [editUserId, setEditUserId] = useState('');
    const [editActiveQuick, setEditActiveQuick] = useState(null);
    const [editLoading, setEditLoading] = useState(false);
    const [deleteLoading, setDeleteLoading] = useState(null);
    const [filterDate, setFilterDate] = useState('');

    const toast = useToast();
    const currentUserId = getCurrentUserId();

    // Hämta alla användare vid komponentladdning
    useEffect(() => {
        const loadUsers = async () => {
            try {
                const data = await fetchAllUsers();
                setUsers(data);
            } catch (err) {
                console.error("Kunde inte hämta användare:", err);
            }
        };
        loadUsers();
    }, []);

    // Hämta alla pass när "Alla Pass"-fliken aktiveras
    useEffect(() => {
        if (activeTab === 'allShifts') {
            loadAllShifts();
        }
    }, [activeTab]);

    const loadAllShifts = async () => {
        setShiftsLoading(true);
        try {
            const data = await fetchShifts();
            setAllShifts(data);
        } catch (err) {
            console.error("Kunde inte hämta pass:", err);
            toast.error("Kunde inte hämta pass.");
        } finally {
            setShiftsLoading(false);
        }
    };

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

            await createManagerShift(payload);

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

    // Byta roll för en användare
    const handleRoleChange = async (targetUserId, newRole) => {
        setUpdatingUserId(targetUserId);
        try {
            await updateUserRole(targetUserId, newRole);
            // Uppdatera lokalt i state
            setUsers(prev =>
                prev.map(u => u.id === targetUserId ? { ...u, role: newRole } : u)
            );
            toast.success("Rollen har uppdaterats.");
        } catch (err) {
            const msg = err.response?.data?.message || "Kunde inte uppdatera rollen.";
            toast.error(msg);
        } finally {
            setUpdatingUserId(null);
        }
    };

    // Radera en användare
    const handleDeleteUser = async (targetUserId, fullName) => {
        if (!window.confirm(`Är du säker på att du vill radera ${fullName}?`)) return;
        try {
            await deleteUser(targetUserId);
            setUsers(prev => prev.filter(u => u.id !== targetUserId));
            toast.success(`${fullName} har tagits bort.`);
        } catch (err) {
            const msg = err.response?.data?.message || "Kunde inte radera användaren.";
            toast.error(msg);
        }
    };

    // --- "Alla Pass"-funktioner ---

    const pad = (n) => String(n).padStart(2, '0');

    // Öppna redigering för ett pass — visa lokala tider (matchar listvisningen)
    const handleEditClick = (shift) => {
        const start = new Date(shift.startTime);
        const end = new Date(shift.endTime);
        setEditingShiftId(shift.id);
        setEditDate(`${start.getFullYear()}-${pad(start.getMonth() + 1)}-${pad(start.getDate())}`);
        setEditStartTime(`${pad(start.getHours())}:${pad(start.getMinutes())}`);
        setEditEndTime(`${pad(end.getHours())}:${pad(end.getMinutes())}`);
        setEditUserId(shift.user?.id ?? '');
        setEditActiveQuick(null);
    };

    // Snabbval i redigeringsläge
    const handleEditQuickSelect = (quick) => {
        setEditStartTime(quick.start);
        setEditEndTime(quick.end);
        setEditActiveQuick(quick.label);
    };

    // Spara redigerat pass — konvertera lokal tid till UTC innan vi skickar
    const handleEditSave = async (shiftId) => {
        setEditLoading(true);
        try {
            let endDate = editDate;
            if (editEndTime <= editStartTime) {
                const next = new Date(editDate);
                next.setDate(next.getDate() + 1);
                endDate = `${next.getFullYear()}-${pad(next.getMonth() + 1)}-${pad(next.getDate())}`;
            }

            // Skapa Date-objekt från lokala värden och konvertera till UTC ISO-sträng
            const localStart = new Date(`${editDate}T${editStartTime}`);
            const localEnd = new Date(`${endDate}T${editEndTime}`);

            const payload = {
                startTime: localStart.toISOString(),
                endTime: localEnd.toISOString(),
                userId: editUserId === '' ? null : editUserId
            };

            await updateShift(shiftId, payload);
            toast.success("Passet har uppdaterats!");
            setEditingShiftId(null);
            await loadAllShifts();
        } catch (err) {
            const msg = err.response?.data?.message || "Kunde inte uppdatera passet.";
            toast.error(msg);
        } finally {
            setEditLoading(false);
        }
    };

    // Radera ett pass
    const handleDeleteShift = async (shiftId) => {
        if (!window.confirm("Är du säker på att du vill radera detta pass? Eventuella väntande bytesförfrågningar kommer avbrytas.")) return;
        setDeleteLoading(shiftId);
        try {
            await deleteShift(shiftId);
            setAllShifts(prev => prev.filter(s => s.id !== shiftId));
            toast.success("Passet har raderats!");
        } catch (err) {
            const msg = err.response?.data?.message || "Kunde inte radera passet.";
            toast.error(msg);
        } finally {
            setDeleteLoading(null);
        }
    };

    const duration = calcDuration(startTime, endTime);
    const editDuration = calcDuration(editStartTime, editEndTime);

    // Filtrera och sortera pass (nyast först)
    const filteredShifts = allShifts
        .filter(s => {
            if (!filterDate) return true;
            const shiftDate = new Date(s.startTime).toISOString().split('T')[0];
            return shiftDate === filterDate;
        })
        .sort((a, b) => new Date(b.startTime) - new Date(a.startTime));

    return (
        <div className="bg-slate-900/80 backdrop-blur-xl p-6 md:p-8 rounded-3xl border border-slate-800 shadow-2xl relative overflow-hidden">
            <div className="absolute top-0 right-0 w-64 h-64 bg-purple-500/10 rounded-full blur-3xl -mr-32 -mt-32 pointer-events-none"></div>

            {/* Fliknavigation */}
            <div className="flex gap-2 mb-6 border-b border-slate-800 pb-4 relative z-10">
                <button
                    onClick={() => setActiveTab('shifts')}
                    className={`px-4 py-2 rounded-lg text-xs font-black uppercase tracking-widest transition-all
                        ${activeTab === 'shifts'
                            ? 'bg-purple-600/20 border border-purple-500 text-purple-300'
                            : 'text-slate-500 hover:text-white'}`}
                >
                    Schemalägg Pass
                </button>
                <button
                    onClick={() => setActiveTab('allShifts')}
                    className={`px-4 py-2 rounded-lg text-xs font-black uppercase tracking-widest transition-all
                        ${activeTab === 'allShifts'
                            ? 'bg-emerald-600/20 border border-emerald-500 text-emerald-300'
                            : 'text-slate-500 hover:text-white'}`}
                >
                    Alla Pass
                </button>
                <button
                    onClick={() => setActiveTab('users')}
                    className={`px-4 py-2 rounded-lg text-xs font-black uppercase tracking-widest transition-all
                        ${activeTab === 'users'
                            ? 'bg-blue-600/20 border border-blue-500 text-blue-300'
                            : 'text-slate-500 hover:text-white'}`}
                >
                    Användare
                </button>
            </div>

            {/* Flik: Schemalägg Pass */}
            {activeTab === 'shifts' && (
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
            )}

            {/* Flik: Alla Pass */}
            {activeTab === 'allShifts' && (
                <div className="space-y-4 relative z-10">
                    {/* Datumfilter */}
                    <div className="flex items-center gap-3">
                        <input
                            type="date"
                            value={filterDate}
                            onChange={(e) => setFilterDate(e.target.value)}
                            className="bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-2 text-sm focus:outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500 transition-all"
                        />
                        {filterDate && (
                            <button
                                onClick={() => setFilterDate('')}
                                className="text-xs text-slate-400 hover:text-white transition-colors"
                            >
                                Rensa filter
                            </button>
                        )}
                        <span className="text-xs text-slate-500 ml-auto">
                            {filteredShifts.length} pass
                        </span>
                    </div>

                    {shiftsLoading ? (
                        <div className="text-center py-12">
                            <div className="w-8 h-8 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin mx-auto mb-3"></div>
                            <p className="text-slate-400 text-sm">Laddar pass...</p>
                        </div>
                    ) : filteredShifts.length === 0 ? (
                        <p className="text-slate-500 text-sm text-center py-8">Inga pass hittades.</p>
                    ) : (
                        <div className="space-y-2">
                            {filteredShifts.map(shift => {
                                const start = new Date(shift.startTime);
                                const end = new Date(shift.endTime);
                                const dateStr = start.toLocaleDateString('sv-SE', { weekday: 'short', month: 'short', day: 'numeric' });
                                const timeStr = `${start.toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' })} - ${end.toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' })}`;
                                const durationStr = calcDurationFromDates(shift.startTime, shift.endTime);
                                const isEditing = editingShiftId === shift.id;
                                const isDeleting = deleteLoading === shift.id;
                                const userName = shift.user
                                    ? `${shift.user.firstName} ${shift.user.lastName}`
                                    : null;

                                return (
                                    <div key={shift.id} className="bg-slate-800/50 border border-slate-700 rounded-xl overflow-hidden">
                                        {/* Passrad */}
                                        <div className="flex items-center gap-3 px-4 py-3">
                                            {/* Datum + tid */}
                                            <div className="flex-1 min-w-0">
                                                <p className="text-white text-sm font-semibold">{dateStr}</p>
                                                <p className="text-slate-400 text-xs">{timeStr} ({durationStr})</p>
                                            </div>

                                            {/* Tilldelad eller öppet */}
                                            <div className="flex-shrink-0">
                                                {userName ? (
                                                    <span className="text-xs text-slate-300 bg-slate-700/50 px-2 py-1 rounded-lg">
                                                        {userName}
                                                    </span>
                                                ) : (
                                                    <span className="text-xs text-emerald-400 bg-emerald-500/10 border border-emerald-500/30 px-2 py-1 rounded-lg">
                                                        Öppet pass
                                                    </span>
                                                )}
                                            </div>

                                            {/* Swap-status */}
                                            {shift.isUpForSwap && (
                                                <span className="text-[10px] font-black uppercase tracking-widest text-amber-400 bg-amber-500/10 border border-amber-500/30 px-2 py-1 rounded-full flex-shrink-0">
                                                    Byte
                                                </span>
                                            )}

                                            {/* Redigera-knapp */}
                                            <button
                                                onClick={() => isEditing ? setEditingShiftId(null) : handleEditClick(shift)}
                                                className={`p-1.5 rounded-lg transition-colors flex-shrink-0 ${isEditing ? 'text-emerald-400 bg-emerald-500/10' : 'text-blue-400 hover:text-blue-300 hover:bg-blue-500/10'}`}
                                                title="Redigera pass"
                                            >
                                                <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                                </svg>
                                            </button>

                                            {/* Radera-knapp */}
                                            <button
                                                onClick={() => handleDeleteShift(shift.id)}
                                                disabled={isDeleting}
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/10 p-1.5 rounded-lg transition-colors disabled:opacity-30 flex-shrink-0"
                                                title="Radera pass"
                                            >
                                                {isDeleting ? (
                                                    <div className="w-4 h-4 border-2 border-red-400 border-t-transparent rounded-full animate-spin"></div>
                                                ) : (
                                                    <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                                        <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                                    </svg>
                                                )}
                                            </button>
                                        </div>

                                        {/* Redigeringsformulär (expanderbart) */}
                                        {isEditing && (
                                            <div className="border-t border-slate-700 px-4 py-4 bg-slate-800/30 space-y-4">
                                                {/* Datum */}
                                                <div className="space-y-1">
                                                    <label className="text-xs font-bold text-slate-500 uppercase tracking-widest">Datum</label>
                                                    <input
                                                        type="date"
                                                        value={editDate}
                                                        onChange={(e) => setEditDate(e.target.value)}
                                                        className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500 transition-all"
                                                    />
                                                </div>

                                                {/* Snabbval */}
                                                <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                                                    {QUICK_SHIFTS.map((quick) => (
                                                        <button
                                                            key={quick.label}
                                                            type="button"
                                                            onClick={() => handleEditQuickSelect(quick)}
                                                            className={`py-2 px-2 rounded-lg text-[10px] font-black uppercase tracking-wider transition-all border
                                                                ${editActiveQuick === quick.label
                                                                    ? 'bg-emerald-600/20 border-emerald-500 text-emerald-300'
                                                                    : 'bg-slate-800/50 border-slate-700 text-slate-400 hover:border-slate-600 hover:text-white'
                                                                }`}
                                                        >
                                                            {quick.icon} {quick.label}
                                                        </button>
                                                    ))}
                                                </div>

                                                {/* Start- och sluttid */}
                                                <div className="grid grid-cols-2 gap-3">
                                                    <div className="space-y-1">
                                                        <label className="text-xs font-bold text-slate-500 uppercase tracking-widest">Start</label>
                                                        <input
                                                            type="time"
                                                            value={editStartTime}
                                                            onChange={(e) => { setEditStartTime(e.target.value); setEditActiveQuick(null); }}
                                                            className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500 transition-all"
                                                        />
                                                    </div>
                                                    <div className="space-y-1">
                                                        <label className="text-xs font-bold text-slate-500 uppercase tracking-widest">Slut</label>
                                                        <input
                                                            type="time"
                                                            value={editEndTime}
                                                            onChange={(e) => { setEditEndTime(e.target.value); setEditActiveQuick(null); }}
                                                            className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500 transition-all"
                                                        />
                                                    </div>
                                                </div>

                                                {/* Passlängd */}
                                                {editDuration && (
                                                    <div className="flex items-center gap-2">
                                                        <span className="w-2 h-2 rounded-full bg-emerald-400 shadow-[0_0_8px_#10b981]"></span>
                                                        <span className="text-xs font-bold text-emerald-300">Passlängd: {editDuration}</span>
                                                    </div>
                                                )}

                                                {/* Tilldela personal */}
                                                <div className="space-y-1">
                                                    <label className="text-xs font-bold text-slate-500 uppercase tracking-widest">Personal</label>
                                                    <select
                                                        value={editUserId}
                                                        onChange={(e) => setEditUserId(e.target.value)}
                                                        className="w-full bg-slate-800 border border-slate-700 text-white rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500 transition-all appearance-none"
                                                    >
                                                        <option value="">Öppet pass (ingen ägare)</option>
                                                        <option disabled>──────────</option>
                                                        {users.map(user => (
                                                            <option key={user.id} value={user.id}>
                                                                {user.firstName} {user.lastName}
                                                            </option>
                                                        ))}
                                                    </select>
                                                </div>

                                                {/* Spara / Avbryt */}
                                                <div className="flex gap-3">
                                                    <button
                                                        onClick={() => handleEditSave(shift.id)}
                                                        disabled={editLoading}
                                                        className={`flex-1 py-2.5 rounded-xl font-black uppercase tracking-widest text-xs transition-all
                                                            ${editLoading
                                                                ? 'bg-slate-700 text-slate-500 cursor-not-allowed'
                                                                : 'bg-gradient-to-r from-emerald-600 to-teal-600 text-white hover:shadow-emerald-500/25 hover:scale-[1.02] active:scale-[0.98]'
                                                            }`}
                                                    >
                                                        {editLoading ? 'Sparar...' : 'Spara'}
                                                    </button>
                                                    <button
                                                        onClick={() => setEditingShiftId(null)}
                                                        className="px-6 py-2.5 rounded-xl font-black uppercase tracking-widest text-xs text-slate-400 border border-slate-700 hover:text-white hover:border-slate-500 transition-all"
                                                    >
                                                        Avbryt
                                                    </button>
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </div>
            )}

            {/* Flik: Användare */}
            {activeTab === 'users' && (
                <div className="space-y-3 relative z-10">
                    {users.length === 0 ? (
                        <p className="text-slate-500 text-sm text-center py-8">Inga användare hittades.</p>
                    ) : (
                        users.map(user => {
                            const isSelf = user.id === currentUserId;
                            const initials = `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase();
                            return (
                                <div
                                    key={user.id}
                                    className="flex items-center gap-4 bg-slate-800/50 border border-slate-700 rounded-xl px-4 py-3"
                                >
                                    {/* Initialer-avatar */}
                                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-600 to-indigo-600 flex items-center justify-center text-white text-sm font-black flex-shrink-0">
                                        {initials}
                                    </div>

                                    {/* Namn + e-post */}
                                    <div className="flex-1 min-w-0">
                                        <p className="text-white text-sm font-semibold truncate">
                                            {user.firstName} {user.lastName}
                                            {isSelf && <span className="ml-2 text-xs text-slate-500">(du)</span>}
                                        </p>
                                        <p className="text-slate-400 text-xs truncate">{user.email}</p>
                                    </div>

                                    {/* Roll-badge */}
                                    <span className={`text-[10px] font-black uppercase tracking-widest px-2 py-1 rounded-full border flex-shrink-0
                                        ${user.role === 'Manager'
                                            ? 'bg-amber-500/10 border-amber-500/30 text-amber-400'
                                            : 'bg-blue-500/10 border-blue-500/30 text-blue-400'
                                        }`}
                                    >
                                        {user.role}
                                    </span>

                                    {/* Roll-dropdown */}
                                    <select
                                        value={user.role}
                                        disabled={isSelf || updatingUserId === user.id}
                                        onChange={(e) => handleRoleChange(user.id, e.target.value)}
                                        className="bg-slate-700 border border-slate-600 text-white text-xs rounded-lg px-2 py-1.5 focus:outline-none focus:border-blue-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0"
                                    >
                                        <option value="Employee">Employee</option>
                                        <option value="Manager">Manager</option>
                                    </select>

                                    {/* Radera-knapp */}
                                    <button
                                        disabled={isSelf}
                                        onClick={() => handleDeleteUser(user.id, `${user.firstName} ${user.lastName}`)}
                                        className="text-red-400 hover:text-red-300 transition-colors disabled:opacity-30 disabled:cursor-not-allowed flex-shrink-0 p-1"
                                        title={isSelf ? 'Du kan inte radera ditt eget konto' : `Radera ${user.firstName}`}
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                            <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                        </svg>
                                    </button>
                                </div>
                            );
                        })
                    )}
                </div>
            )}
        </div>
    );
};

export default ManagerPanel;
