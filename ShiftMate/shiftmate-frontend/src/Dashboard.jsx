import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { fetchMyShifts, fetchReceivedSwapRequests, fetchSentSwapRequests, decodeToken } from './api';
import { formatTime, formatDate, isSameDay } from './utils/dateUtils';

const Dashboard = () => {
    const [shifts, setShifts] = useState([]);
    const [requests, setRequests] = useState([]);
    const [sentRequests, setSentRequests] = useState([]);
    const [loading, setLoading] = useState(true);
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
    const nextShift = upcomingShifts[0];

    // Beräkna timmar denna vecka
    const getWeekHours = () => {
        const monday = new Date(now);
        const day = monday.getDay();
        const diff = day === 0 ? -6 : 1 - day;
        monday.setDate(monday.getDate() + diff);
        monday.setHours(0, 0, 0, 0);

        const sunday = new Date(monday);
        sunday.setDate(sunday.getDate() + 7);

        return shifts
            .filter(s => {
                const start = new Date(s.startTime);
                return start >= monday && start < sunday;
            })
            .reduce((sum, s) => {
                const hours = (new Date(s.endTime) - new Date(s.startTime)) / 3600000;
                return sum + hours;
            }, 0);
    };

    const weekHours = getWeekHours();

    // Hälsningsfras baserat på tid
    const getGreeting = () => {
        const hour = now.getHours();
        if (hour < 6) return 'God natt';
        if (hour < 10) return 'God morgon';
        if (hour < 18) return 'Hej';
        return 'God kväll';
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

    if (loading) return (
        <div className="p-10 text-center text-blue-400 font-bold animate-pulse tracking-widest uppercase">
            Laddar dashboard...
        </div>
    );

    return (
        <div className="space-y-8">
            {/* Hälsning */}
            <div>
                <h2 className="text-2xl md:text-3xl font-black text-white tracking-tight">
                    {getGreeting()}{firstName ? `, ${firstName}` : ''}
                </h2>
                <p className="text-slate-400 text-sm mt-1">
                    {now.toLocaleDateString('sv-SE', { weekday: 'long', day: 'numeric', month: 'long' })}
                </p>
            </div>

            {/* Snabbkort */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {/* Nästa pass */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-blue-500 shadow-[0_0_15px_#3b82f6]"></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Nästa pass</p>
                    {nextShift ? (
                        <>
                            <p className="text-lg font-black text-white">{formatTime(nextShift.startTime)}</p>
                            <p className="text-xs text-slate-400">{getCountdown()}</p>
                        </>
                    ) : (
                        <p className="text-sm text-slate-500">Inga pass</p>
                    )}
                </div>

                {/* Idag */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-green-500 shadow-[0_0_15px_#22c55e]"></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Idag</p>
                    <p className="text-lg font-black text-white">{todayShifts.length} pass</p>
                    <p className="text-xs text-slate-400">
                        {todayShifts.length > 0
                            ? todayShifts.map(s => formatTime(s.startTime)).join(', ')
                            : 'Ledig idag'}
                    </p>
                </div>

                {/* Veckan */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-purple-500 shadow-[0_0_15px_#a855f7]"></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Denna vecka</p>
                    <p className="text-lg font-black text-white">{weekHours.toFixed(1)}h</p>
                    <p className="text-xs text-slate-400">{upcomingShifts.filter(s => {
                        const start = new Date(s.startTime);
                        const monday = new Date(now);
                        const day = monday.getDay();
                        const diff = day === 0 ? -6 : 1 - day;
                        monday.setDate(monday.getDate() + diff);
                        monday.setHours(0, 0, 0, 0);
                        const sunday = new Date(monday);
                        sunday.setDate(sunday.getDate() + 7);
                        return start >= monday && start < sunday;
                    }).length} pass inbokade</p>
                </div>

                {/* Förfrågningar */}
                <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 relative overflow-hidden">
                    <div className={`absolute left-0 top-0 bottom-0 w-1 ${requests.length > 0 ? 'bg-yellow-500 shadow-[0_0_15px_#eab308]' : sentRequests.length > 0 ? 'bg-indigo-500 shadow-[0_0_15px_#6366f1]' : 'bg-slate-700'}`}></div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Förfrågningar</p>
                    <p className="text-lg font-black text-white">{requests.length + sentRequests.length}</p>
                    <p className="text-xs text-slate-400">
                        {requests.length > 0 && <span className="text-yellow-400">{requests.length} inkommande</span>}
                        {requests.length > 0 && sentRequests.length > 0 && ' · '}
                        {sentRequests.length > 0 && <span className="text-indigo-400">{sentRequests.length} skickade</span>}
                        {requests.length === 0 && sentRequests.length === 0 && 'Inga väntande'}
                    </p>
                </div>
            </div>

            {/* Förfrågningar-alert */}
            {requests.length > 0 && (
                <Link to="/mine" className="block bg-yellow-500/5 border border-yellow-500/20 rounded-2xl p-4 hover:bg-yellow-500/10 transition-all group">
                    <div className="flex items-center gap-3">
                        <span className="w-3 h-3 bg-yellow-400 rounded-full animate-pulse shadow-[0_0_10px_#eab308]"></span>
                        <div className="flex-1">
                            <p className="text-sm font-black text-yellow-400">
                                {requests.length} inkommande {requests.length === 1 ? 'bytesförfrågan' : 'bytesförfrågningar'}
                            </p>
                            <p className="text-xs text-slate-400 mt-0.5">Klicka för att hantera</p>
                        </div>
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-yellow-400 group-hover:translate-x-1 transition-transform"><path d="m9 18 6-6-6-6"/></svg>
                    </div>
                </Link>
            )}

            {/* Skickade förfrågningar */}
            {sentRequests.length > 0 && (
                <div className="bg-indigo-500/5 border border-indigo-500/20 rounded-2xl p-4 space-y-3">
                    <div className="flex items-center gap-3">
                        <span className="w-3 h-3 bg-indigo-400 rounded-full shadow-[0_0_10px_#6366f1]"></span>
                        <p className="text-sm font-black text-indigo-400">
                            {sentRequests.length} skickade {sentRequests.length === 1 ? 'förfrågan' : 'förfrågningar'} väntar på svar
                        </p>
                    </div>
                    <div className="space-y-2 ml-6">
                        {sentRequests.map(req => (
                            <div key={req.id} className="flex items-center gap-3 text-xs text-slate-400">
                                <span className="text-indigo-400/60">→</span>
                                <span>
                                    {req.shift && <span className="text-white font-bold">{formatTime(req.shift.startTime)} – {formatTime(req.shift.endTime)}</span>}
                                    {req.shift && <span className="ml-1">({formatDate(req.shift.startTime)})</span>}
                                    {req.requestingUser && <span className="ml-1">till <span className="text-white">{req.requestingUser.firstName} {req.requestingUser.lastName}</span></span>}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* Dagens schema / Kommande pass */}
            <div>
                <h3 className="text-lg font-black text-white tracking-tight mb-4 uppercase">
                    {todayShifts.length > 0 ? 'Dagens pass' : 'Kommande pass'}
                </h3>
                {upcomingShifts.length === 0 ? (
                    <div className="bg-slate-900/50 p-10 rounded-2xl text-center border-2 border-dashed border-slate-800">
                        <p className="text-slate-500 font-medium">Inga kommande pass inbokade.</p>
                        <Link to="/market" className="text-blue-400 text-sm font-bold hover:underline mt-2 inline-block">
                            Kolla lediga pass
                        </Link>
                    </div>
                ) : (
                    <div className="space-y-3">
                        {upcomingShifts.slice(0, 5).map(shift => {
                            const shiftDate = new Date(shift.startTime);
                            const isShiftToday = isSameDay(shiftDate, now);
                            const isPast = new Date(shift.endTime) < now;
                            const isOngoing = new Date(shift.startTime) <= now && new Date(shift.endTime) >= now;

                            return (
                                <div key={shift.id} className={`bg-slate-900/60 border rounded-2xl p-4 flex items-center gap-4 relative overflow-hidden transition-all
                                    ${isOngoing ? 'border-green-500/30 bg-green-500/5' : 'border-slate-800'}
                                    ${isPast ? 'opacity-50' : ''}`}
                                >
                                    <div className={`absolute left-0 top-0 bottom-0 w-1.5
                                        ${isOngoing ? 'bg-green-500 shadow-[0_0_15px_#22c55e]'
                                        : isShiftToday ? 'bg-blue-500 shadow-[0_0_15px_#3b82f6]'
                                        : 'bg-slate-700'}`}
                                    ></div>

                                    {/* Datum-box */}
                                    <div className={`ml-3 w-14 h-14 rounded-xl flex flex-col items-center justify-center shrink-0
                                        ${isShiftToday ? 'bg-blue-500/10 border border-blue-500/20' : 'bg-slate-800/50 border border-slate-700/50'}`}
                                    >
                                        <span className="text-[10px] font-black text-slate-400 uppercase">
                                            {shiftDate.toLocaleDateString('sv-SE', { weekday: 'short' })}
                                        </span>
                                        <span className={`text-lg font-black ${isShiftToday ? 'text-blue-400' : 'text-white'}`}>
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
                                            {isShiftToday && !isOngoing && ' • Idag'}
                                            {isOngoing && <span className="text-green-400 font-bold"> • Pågår nu</span>}
                                        </p>
                                    </div>

                                    {/* Status-badge */}
                                    {shift.isUpForSwap && (
                                        <span className="text-[9px] font-black text-yellow-400 bg-yellow-500/10 px-2 py-1 rounded-lg border border-yellow-500/20 uppercase shrink-0">
                                            Ute för byte
                                        </span>
                                    )}
                                </div>
                            );
                        })}

                        {upcomingShifts.length > 5 && (
                            <Link to="/mine" className="block text-center text-sm text-blue-400 font-bold hover:underline py-2">
                                Visa alla {upcomingShifts.length} pass
                            </Link>
                        )}
                    </div>
                )}
            </div>

            {/* Snabblänkar */}
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
