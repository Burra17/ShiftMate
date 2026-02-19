import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import EmptyState from './components/EmptyState';
import LoadingSpinner from './components/LoadingSpinner';
import {
    fetchMyShifts,
    fetchReceivedSwapRequests,
    fetchSentSwapRequests,
    acceptSwapRequest,
    declineSwapRequest,
    decodeToken
} from './api';
import { formatTime, formatDate, isSameDay, getMonday, getShortWeekday } from './utils/dateUtils';

const Dashboard = () => {
    const [shifts, setShifts] = useState([]);
    const [requests, setRequests] = useState([]);
    const [sentRequests, setSentRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState(null); // requestId som bearbetas
    const [now] = useState(new Date());

    // Hämta användarens förnamn från JWT
    const getFirstName = () => {
        const payload = decodeToken();
        if (!payload) return '';
        return payload.FirstName || payload.given_name || '';
    };

    const firstName = getFirstName();

    useEffect(() => {
        const loadData = async () => {
            try {
                const [shiftsData, requestsData, sentData] = await Promise.all([
                    fetchMyShifts(),
                    fetchReceivedSwapRequests(),
                    fetchSentSwapRequests()
                ]);
                setShifts(shiftsData);
                setRequests(requestsData);
                setSentRequests(sentData);
            } catch (err) {
                console.error("Dashboard: kunde inte ladda data", err);
            } finally {
                setLoading(false);
            }
        };
        loadData();
    }, []);

    // Filtrera pass: idag och kommande (sorterade)
    const upcomingShifts = shifts
        .filter(s => new Date(s.endTime) >= now)
        .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

    const todayShifts = upcomingShifts.filter(s => isSameDay(new Date(s.startTime), now));
    const futureShifts = upcomingShifts.filter(s => !isSameDay(new Date(s.startTime), now));
    const nextShift = upcomingShifts[0];

    // Veckodata: måndag–söndag med pass-information (inkluderar historiska pass denna vecka)
    const getWeekDays = () => {
        const monday = getMonday(now);
        return Array.from({ length: 7 }, (_, i) => {
            const day = new Date(monday);
            day.setDate(day.getDate() + i);
            const dayShifts = shifts.filter(s => isSameDay(new Date(s.startTime), day));
            return {
                date: day,
                hasShift: dayShifts.length > 0,
                shiftCount: dayShifts.length,
                isToday: isSameDay(day, now),
            };
        });
    };

    const weekDays = getWeekDays();

    // Beräkna timmar och antal pass denna vecka
    const getWeekStats = () => {
        const monday = getMonday(now);
        const sunday = new Date(monday);
        sunday.setDate(sunday.getDate() + 7);
        const weekShifts = shifts.filter(s => {
            const start = new Date(s.startTime);
            return start >= monday && start < sunday;
        });
        const hours = weekShifts.reduce((sum, s) => {
            return sum + (new Date(s.endTime) - new Date(s.startTime)) / 3600000;
        }, 0);
        return { hours, count: weekShifts.length };
    };

    const { hours: weekHours, count: weekShiftCount } = getWeekStats();

    // Hälsningsfras baserat på tid på dagen
    const getGreeting = () => {
        const hour = now.getHours();
        if (hour < 6) return 'God natt';
        if (hour < 10) return 'God morgon';
        if (hour < 18) return 'Hej';
        return 'God kväll';
    };

    // Datum-label för nästa pass (Idag / Imorgon / datum)
    const getNextShiftDateLabel = () => {
        if (!nextShift) return null;
        if (isSameDay(new Date(nextShift.startTime), now)) return 'Idag';
        const tomorrow = new Date(now);
        tomorrow.setDate(tomorrow.getDate() + 1);
        if (isSameDay(new Date(nextShift.startTime), tomorrow)) return 'Imorgon';
        return formatDate(nextShift.startTime);
    };

    // Countdown till nästa pass
    const getCountdown = () => {
        if (!nextShift) return null;
        const start = new Date(nextShift.startTime);
        const diffMs = start - now;
        if (diffMs <= 0) return 'Pågår nu';

        const hours = Math.floor(diffMs / 3600000);
        const minutes = Math.floor((diffMs % 3600000) / 60000);

        if (hours >= 24) {
            const days = Math.floor(hours / 24);
            return `om ${days} ${days === 1 ? 'dag' : 'dagar'}`;
        }
        if (hours > 0) return `om ${hours}h ${minutes}min`;
        return `om ${minutes} min`;
    };

    // Hantera godkänn bytesförfrågan
    const handleAccept = async (requestId) => {
        setActionLoading(requestId);
        try {
            await acceptSwapRequest(requestId);
            setRequests(prev => prev.filter(r => r.id !== requestId));
            window.dispatchEvent(new CustomEvent('swaps-updated'));
        } catch (err) {
            console.error("Kunde inte godkänna förfrågan:", err);
        } finally {
            setActionLoading(null);
        }
    };

    // Hantera neka bytesförfrågan
    const handleDecline = async (requestId) => {
        setActionLoading(requestId);
        try {
            await declineSwapRequest(requestId);
            setRequests(prev => prev.filter(r => r.id !== requestId));
            window.dispatchEvent(new CustomEvent('swaps-updated'));
        } catch (err) {
            console.error("Kunde inte neka förfrågan:", err);
        } finally {
            setActionLoading(null);
        }
    };

    if (loading) return <LoadingSpinner message="Laddar dashboard..." />;

    return (
        <div className="space-y-6">

            {/* ── Hälsning ── */}
            <div>
                <h2 className="text-2xl md:text-3xl font-black text-white tracking-tight">
                    {getGreeting()}{firstName ? `, ${firstName}` : ''}
                </h2>
                <p className="text-slate-400 text-sm mt-1">
                    {now.toLocaleDateString('sv-SE', { weekday: 'long', day: 'numeric', month: 'long' })}
                </p>
            </div>

            {/* ── Snabbkort (4 st) ── */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">

                {/* Nästa pass – visar tid, dag-label och countdown */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-blue-500 shadow-[0_0_15px_#3b82f6]"></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2">Nästa pass</p>
                    {nextShift ? (
                        <>
                            <p className="text-xl font-black text-white leading-none">{formatTime(nextShift.startTime)}</p>
                            <p className="text-xs font-bold text-blue-400 mt-1">{getNextShiftDateLabel()}</p>
                            <p className="text-xs text-slate-500 mt-0.5">{getCountdown()}</p>
                        </>
                    ) : (
                        <p className="text-sm text-slate-500">Inga pass</p>
                    )}
                </div>

                {/* Idag */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-green-500 shadow-[0_0_15px_#22c55e]"></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2">Idag</p>
                    <p className="text-xl font-black text-white leading-none">{todayShifts.length} pass</p>
                    <p className="text-xs text-slate-400 mt-1">
                        {todayShifts.length > 0
                            ? todayShifts.map(s => formatTime(s.startTime)).join(', ')
                            : 'Ledig idag'}
                    </p>
                </div>

                {/* Denna vecka – med progress bar mot 40h */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-purple-500 shadow-[0_0_15px_#a855f7]"></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2">Denna vecka</p>
                    <p className="text-xl font-black text-white leading-none">{weekHours.toFixed(1)}h</p>
                    <div className="mt-2 h-1 bg-slate-700 rounded-full overflow-hidden">
                        <div
                            className="h-full bg-purple-500 rounded-full transition-all duration-500"
                            style={{ width: `${Math.min((weekHours / 40) * 100, 100)}%` }}
                        ></div>
                    </div>
                    <p className="text-xs text-slate-400 mt-1">{weekShiftCount} pass inbokade</p>
                </div>

                {/* Förfrågningar */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className={`absolute left-0 top-0 bottom-0 w-1 ${requests.length > 0
                        ? 'bg-yellow-500 shadow-[0_0_15px_#eab308]'
                        : sentRequests.length > 0
                            ? 'bg-indigo-500 shadow-[0_0_15px_#6366f1]'
                            : 'bg-slate-700'}`}
                    ></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2">Förfrågningar</p>
                    <p className="text-xl font-black text-white leading-none">{requests.length + sentRequests.length}</p>
                    <p className="text-xs text-slate-400 mt-1">
                        {requests.length > 0 && <span className="text-yellow-400">{requests.length} inkommande</span>}
                        {requests.length > 0 && sentRequests.length > 0 && ' · '}
                        {sentRequests.length > 0 && <span className="text-indigo-400">{sentRequests.length} skickade</span>}
                        {requests.length === 0 && sentRequests.length === 0 && 'Inga väntande'}
                    </p>
                </div>
            </div>

            {/* ── Veckoöversikt ── */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4">
                <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-3">Veckans schema</p>
                <div className="grid grid-cols-7 gap-1.5">
                    {weekDays.map((day, i) => (
                        <div
                            key={i}
                            className={`flex flex-col items-center gap-1.5 py-2.5 px-1 rounded-xl transition-all
                                ${day.isToday
                                    ? 'bg-blue-500/15 border border-blue-500/30'
                                    : 'border border-transparent'}`}
                        >
                            <span className={`text-[10px] font-black uppercase tracking-wider
                                ${day.isToday ? 'text-blue-400' : 'text-slate-500'}`}>
                                {getShortWeekday(day.date).slice(0, 2)}
                            </span>
                            <span className={`text-sm font-black
                                ${day.isToday ? 'text-blue-300' : day.hasShift ? 'text-white' : 'text-slate-600'}`}>
                                {day.date.getDate()}
                            </span>
                            {/* Pass-indikator prick */}
                            <div className={`w-1.5 h-1.5 rounded-full transition-all
                                ${day.hasShift
                                    ? day.isToday
                                        ? 'bg-blue-400 shadow-[0_0_6px_#60a5fa]'
                                        : 'bg-slate-400'
                                    : 'bg-transparent'}`}
                            ></div>
                        </div>
                    ))}
                </div>
            </div>

            {/* ── Inkommande bytesförfrågningar med åtgärdsknappar ── */}
            {requests.length > 0 && (
                <div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2">
                        {requests.length} inkommande {requests.length === 1 ? 'bytesförfrågan' : 'bytesförfrågningar'}
                    </p>
                    <div className="space-y-2">
                        {requests.map(req => (
                            <div key={req.id} className="bg-yellow-500/5 border border-yellow-500/20 rounded-2xl p-4">
                                <div className="flex items-center justify-between gap-3">
                                    <div className="flex items-center gap-3 flex-1 min-w-0">
                                        {/* Initialbokstavs-avatar */}
                                        <div className="w-9 h-9 rounded-xl bg-slate-800 border border-slate-700 flex items-center justify-center shrink-0">
                                            <span className="text-xs font-black text-slate-300">
                                                {req.requestingUser?.firstName?.[0]}{req.requestingUser?.lastName?.[0]}
                                            </span>
                                        </div>
                                        {/* Förfrågningsinfo */}
                                        <div className="min-w-0">
                                            <p className="text-sm font-black text-white truncate">
                                                {req.requestingUser?.firstName} {req.requestingUser?.lastName}
                                            </p>
                                            <p className="text-xs text-slate-400">
                                                {req.shift && (
                                                    <>{formatTime(req.shift.startTime)} – {formatTime(req.shift.endTime)} · {formatDate(req.shift.startTime)}</>
                                                )}
                                            </p>
                                        </div>
                                    </div>
                                    {/* Godkänn / Neka */}
                                    <div className="flex gap-2 shrink-0">
                                        <button
                                            onClick={() => handleAccept(req.id)}
                                            disabled={actionLoading === req.id}
                                            className="px-3 py-1.5 bg-green-500/10 hover:bg-green-500/20 border border-green-500/30 text-green-400 text-xs font-black rounded-lg transition-all disabled:opacity-50 cursor-pointer"
                                        >
                                            {actionLoading === req.id ? '...' : 'Godkänn'}
                                        </button>
                                        <button
                                            onClick={() => handleDecline(req.id)}
                                            disabled={actionLoading === req.id}
                                            className="px-3 py-1.5 bg-red-500/10 hover:bg-red-500/20 border border-red-500/30 text-red-400 text-xs font-black rounded-lg transition-all disabled:opacity-50 cursor-pointer"
                                        >
                                            {actionLoading === req.id ? '...' : 'Neka'}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* ── Skickade förfrågningar ── */}
            {sentRequests.length > 0 && (
                <div className="bg-indigo-500/5 border border-indigo-500/20 rounded-2xl p-4 space-y-3">
                    <div className="flex items-center gap-3">
                        <span className="w-2.5 h-2.5 bg-indigo-400 rounded-full shadow-[0_0_10px_#6366f1] shrink-0"></span>
                        <p className="text-sm font-black text-indigo-400">
                            {sentRequests.length} skickade {sentRequests.length === 1 ? 'förfrågan' : 'förfrågningar'} väntar på svar
                        </p>
                    </div>
                    <div className="space-y-2 ml-5">
                        {sentRequests.map(req => (
                            <div key={req.id} className="flex items-center gap-2 text-xs text-slate-400">
                                <span className="text-indigo-400/60 shrink-0">→</span>
                                <span>
                                    {req.shift && (
                                        <span className="text-white font-bold">
                                            {formatTime(req.shift.startTime)} – {formatTime(req.shift.endTime)}
                                        </span>
                                    )}
                                    {req.shift && <span className="ml-1">({formatDate(req.shift.startTime)})</span>}
                                    {req.targetUser && (
                                        <span className="ml-1">
                                            till <span className="text-white">{req.targetUser.firstName} {req.targetUser.lastName}</span>
                                        </span>
                                    )}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* ── Dagens pass (bug fix: visar BARA dagens pass, inte alla kommande) ── */}
            {todayShifts.length > 0 && (
                <div>
                    <h3 className="text-base font-black text-white uppercase tracking-widest mb-3">Dagens pass</h3>
                    <div className="space-y-2">
                        {todayShifts.map(shift => {
                            const isOngoing = new Date(shift.startTime) <= now && new Date(shift.endTime) >= now;
                            return (
                                <div
                                    key={shift.id}
                                    className={`bg-slate-900/60 border rounded-2xl p-4 flex items-center gap-4 relative overflow-hidden
                                        ${isOngoing ? 'border-green-500/30 bg-green-500/5' : 'border-blue-500/20 bg-blue-500/5'}`}
                                >
                                    <div className={`absolute left-0 top-0 bottom-0 w-1.5
                                        ${isOngoing
                                            ? 'bg-green-500 shadow-[0_0_15px_#22c55e]'
                                            : 'bg-blue-500 shadow-[0_0_15px_#3b82f6]'}`}
                                    ></div>

                                    {/* Datum-box */}
                                    <div className="ml-3 w-14 h-14 rounded-xl bg-blue-500/10 border border-blue-500/20 flex flex-col items-center justify-center shrink-0">
                                        <span className="text-[10px] font-black text-slate-400 uppercase">idag</span>
                                        <span className="text-lg font-black text-blue-400">{new Date(shift.startTime).getDate()}</span>
                                    </div>

                                    {/* Pass-info */}
                                    <div className="flex-1 min-w-0">
                                        <p className="text-base font-black text-white">
                                            {formatTime(shift.startTime)} – {formatTime(shift.endTime)}
                                        </p>
                                        <p className="text-xs text-slate-400">
                                            {((new Date(shift.endTime) - new Date(shift.startTime)) / 3600000).toFixed(1)}h
                                            {isOngoing && <span className="text-green-400 font-bold"> · Pågår nu</span>}
                                        </p>
                                    </div>

                                    {shift.isUpForSwap && (
                                        <span className="text-[9px] font-black text-yellow-400 bg-yellow-500/10 px-2 py-1 rounded-lg border border-yellow-500/20 uppercase shrink-0">
                                            Ute för byte
                                        </span>
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}

            {/* ── Kommande pass (ej idag) ── */}
            <div>
                <h3 className="text-base font-black text-white uppercase tracking-widest mb-3">Kommande pass</h3>
                {futureShifts.length === 0 && todayShifts.length === 0 ? (
                    <EmptyState icon="💤" message="Inga kommande pass inbokade." linkTo="/market" linkText="Kolla lediga pass" />
                ) : futureShifts.length === 0 ? (
                    <p className="text-sm text-slate-500 py-2">Inga fler pass inbokade den här veckan.</p>
                ) : (
                    <div className="space-y-2">
                        {futureShifts.slice(0, 4).map(shift => {
                            const shiftDate = new Date(shift.startTime);
                            const tomorrow = new Date(now);
                            tomorrow.setDate(tomorrow.getDate() + 1);
                            const isTomorrow = isSameDay(shiftDate, tomorrow);

                            return (
                                <div key={shift.id} className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 flex items-center gap-4 relative overflow-hidden">
                                    <div className="absolute left-0 top-0 bottom-0 w-1.5 bg-slate-700"></div>

                                    {/* Datum-box */}
                                    <div className={`ml-3 w-14 h-14 rounded-xl flex flex-col items-center justify-center shrink-0
                                        ${isTomorrow ? 'bg-blue-500/5 border border-blue-500/15' : 'bg-slate-800/50 border border-slate-700/50'}`}
                                    >
                                        <span className={`text-[10px] font-black uppercase
                                            ${isTomorrow ? 'text-blue-400/70' : 'text-slate-500'}`}>
                                            {shiftDate.toLocaleDateString('sv-SE', { weekday: 'short' })}
                                        </span>
                                        <span className={`text-lg font-black ${isTomorrow ? 'text-blue-300' : 'text-white'}`}>
                                            {shiftDate.getDate()}
                                        </span>
                                    </div>

                                    {/* Pass-info */}
                                    <div className="flex-1 min-w-0">
                                        <p className="text-base font-black text-white">
                                            {formatTime(shift.startTime)} – {formatTime(shift.endTime)}
                                        </p>
                                        <p className="text-xs text-slate-400">
                                            {((new Date(shift.endTime) - new Date(shift.startTime)) / 3600000).toFixed(1)}h
                                            {isTomorrow && <span className="text-blue-400/70"> · Imorgon</span>}
                                        </p>
                                    </div>

                                    {shift.isUpForSwap && (
                                        <span className="text-[9px] font-black text-yellow-400 bg-yellow-500/10 px-2 py-1 rounded-lg border border-yellow-500/20 uppercase shrink-0">
                                            Ute för byte
                                        </span>
                                    )}
                                </div>
                            );
                        })}

                        {futureShifts.length > 4 && (
                            <Link to="/mine" className="block text-center text-sm text-blue-400 font-bold hover:underline py-2">
                                Visa alla {futureShifts.length} kommande pass →
                            </Link>
                        )}
                    </div>
                )}
            </div>

            {/* ── Snabblänkar ── */}
            <div className="grid grid-cols-2 gap-3">
                <Link to="/market" className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 hover:bg-slate-800/60 transition-all group text-center">
                    <p className="text-2xl mb-1">🏪</p>
                    <p className="text-xs font-black text-slate-300 uppercase tracking-widest group-hover:text-white transition-colors">Lediga pass</p>
                </Link>
                <Link to="/schedule" className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 hover:bg-slate-800/60 transition-all group text-center">
                    <p className="text-2xl mb-1">📅</p>
                    <p className="text-xs font-black text-slate-300 uppercase tracking-widest group-hover:text-white transition-colors">Schema</p>
                </Link>
            </div>

        </div>
    );
};

export default Dashboard;
