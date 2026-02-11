import { getMonday, addDays, isSameDay, isToday, getShortWeekday } from '../../utils/dateUtils';
import ShiftCard from './ShiftCard';

/**
 * WeekView — 7 kolumner (mån–sön) på desktop, staplat på mobil.
 */
const WeekView = ({ shifts, currentDate, currentUserId }) => {
    const monday = getMonday(currentDate);

    // Bygg array med 7 dagar (mån–sön)
    const weekDays = Array.from({ length: 7 }, (_, i) => addDays(monday, i));

    // Gruppera pass per dag
    const getShiftsForDay = (day) =>
        shifts
            .filter(s => isSameDay(new Date(s.startTime), day))
            .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

    return (
        <div className="grid grid-cols-1 md:grid-cols-7 gap-3">
            {weekDays.map(day => {
                const dayShifts = getShiftsForDay(day);
                const today = isToday(day);

                return (
                    <div key={day.toISOString()} className="min-w-0">
                        {/* Dagheader */}
                        <div className={`text-center mb-2 pb-2 border-b ${today ? 'border-blue-500/40' : 'border-slate-800'}`}>
                            <p className={`text-[10px] font-bold uppercase tracking-wider ${today ? 'text-blue-400' : 'text-slate-500'}`}>
                                {getShortWeekday(day)}
                            </p>
                            <p className={`text-lg font-black ${today ? 'text-blue-400' : 'text-white'}`}>
                                {day.getDate()}
                            </p>
                            {today && (
                                <div className="w-1.5 h-1.5 rounded-full bg-blue-400 shadow-[0_0_6px_#3b82f6] mx-auto mt-1"></div>
                            )}
                        </div>

                        {/* Pass för dagen */}
                        <div className="space-y-1.5">
                            {dayShifts.length > 0 ? (
                                dayShifts.map(shift => (
                                    <ShiftCard
                                        key={shift.id}
                                        shift={shift}
                                        isOwn={shift.userId === currentUserId}
                                        compact
                                    />
                                ))
                            ) : (
                                <p className="text-[10px] text-slate-600 text-center py-4">—</p>
                            )}
                        </div>
                    </div>
                );
            })}
        </div>
    );
};

export default WeekView;
