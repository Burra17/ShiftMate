import { useState, useEffect } from 'react';
import api from './api';

const Schedule = () => {
    const [schedule, setSchedule] = useState({});
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchSchedule = async () => {
            try {
                const response = await api.get('/Shifts');
                const grouped = groupShiftsByDate(response.data);
                setSchedule(grouped);
            } catch (err) {
                console.error("Kunde inte hämta schemat:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchSchedule();
    }, []);

    const groupShiftsByDate = (shifts) => {
        const sortedShifts = shifts.sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

        return sortedShifts.reduce((groups, shift) => {
            const date = new Date(shift.startTime).toLocaleDateString('sv-SE', {
                weekday: 'long',
                day: 'numeric',
                month: 'long'
            });

            const capitalizedDate = date.charAt(0).toUpperCase() + date.slice(1);

            if (!groups[capitalizedDate]) {
                groups[capitalizedDate] = [];
            }
            groups[capitalizedDate].push(shift);
            return groups;
        }, {});
    };

    const formatTime = (dateString) => {
        return new Date(dateString).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
    };

    // FIX: Hämtar initialer från Namn istället för Email
    const getInitials = (user) => {
        if (!user) return "??";
        // Om vi har för- och efternamn
        if (user.firstName && user.lastName) {
            return (user.firstName[0] + user.lastName[0]).toUpperCase();
        }
        // Fallback till email om namn saknas
        return user.email.substring(0, 2).toUpperCase();
    };

    // FIX: Hjälpfunktion för att visa hela namnet snyggt
    const getFullName = (user) => {
        if (!user) return 'Okänd kollega';
        if (user.firstName && user.lastName) return `${user.firstName} ${user.lastName}`;
        return user.firstName || user.email.split('@')[0];
    }

    if (loading) return <div className="p-10 text-center text-purple-400 font-bold animate-pulse tracking-widest">LADDAR SCHEMA...</div>;

    return (
        <div className="space-y-8">
            {Object.keys(schedule).length === 0 ? (
                <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                    <p className="text-4xl mb-4">📅</p>
                    <p className="text-slate-400 font-medium">Schemat är tomt just nu.</p>
                </div>
            ) : (
                Object.entries(schedule).map(([date, shifts]) => (
                    <div key={date} className="animate-in fade-in slide-in-from-bottom-4 duration-500">
                        <h3 className="text-lg font-black text-purple-400 mb-4 sticky top-0 bg-slate-950/80 backdrop-blur-md p-2 rounded-xl border border-purple-500/20 inline-block shadow-[0_0_15px_rgba(168,85,247,0.2)]">
                            {date}
                        </h3>

                        <div className="grid gap-4">
                            {shifts.map((shift) => (
                                <div key={shift.id} className="bg-slate-900/60 backdrop-blur-md p-5 rounded-2xl border border-slate-800 flex items-center justify-between hover:bg-slate-800 transition-all group">

                                    <div className="flex items-center gap-4">
                                        {/* Avatar med initialer */}
                                        <div className="w-12 h-12 rounded-full bg-gradient-to-br from-purple-600 to-pink-600 flex items-center justify-center shadow-lg shadow-purple-900/40 text-white font-bold text-sm border border-purple-400/30">
                                            {getInitials(shift.user)}
                                        </div>

                                        <div>
                                            {/* HÄR VAR FELET: Nu visar vi Namn istället för Email */}
                                            <p className="text-white font-bold text-sm capitalize">
                                                {getFullName(shift.user)}
                                            </p>

                                            <div className="flex items-center gap-2 mt-1">
                                                <span className="text-xs font-medium text-slate-400 bg-slate-950 px-2 py-0.5 rounded-md border border-slate-800">
                                                    {shift.durationHours}h
                                                </span>
                                                {shift.isUpForSwap && (
                                                    <span className="text-[10px] font-bold text-yellow-400 bg-yellow-500/10 px-2 py-0.5 rounded border border-yellow-500/20">
                                                        BYTES
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </div>

                                    <div className="text-right">
                                        <p className="text-xl font-black text-white tracking-tight">
                                            {formatTime(shift.startTime)}
                                        </p>
                                        <p className="text-xs font-bold text-slate-500">
                                            {formatTime(shift.endTime)}
                                        </p>
                                    </div>

                                </div>
                            ))}
                        </div>
                    </div>
                ))
            )}
        </div>
    );
};

export default Schedule;