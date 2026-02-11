import { getCalendarDays, isSameDay, isToday } from '../../utils/dateUtils';

// Veckodagsrubriker (mån–sön)
const WEEKDAY_HEADERS = ['Mån', 'Tis', 'Ons', 'Tor', 'Fre', 'Lör', 'Sön'];

/**
 * MonthView — 42-cells kalenderrutnät med passprickar.
 * Klicka på en dag för att öppna dagsvyn.
 */
const MonthView = ({ shifts, currentDate, currentUserId, onDayClick }) => {
    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();
    const calendarDays = getCalendarDays(year, month);

    // Hjälpfunktion: hämta pass för en specifik dag
    const getShiftsForDay = (day) =>
        shifts.filter(s => isSameDay(new Date(s.startTime), day));

    // Beräkna prickfärg för ett pass
    const getDotColor = (shift) => {
        if (shift.isUpForSwap) return 'bg-yellow-400 shadow-[0_0_4px_rgba(250,204,21,0.5)]';
        if (shift.userId === currentUserId) return 'bg-blue-400 shadow-[0_0_4px_rgba(59,130,246,0.5)]';
        return 'bg-slate-500';
    };

    return (
        <div>
            {/* Veckodagsheader */}
            <div className="grid grid-cols-7 mb-2">
                {WEEKDAY_HEADERS.map(d => (
                    <div key={d} className="text-center text-[10px] font-bold text-slate-500 uppercase tracking-wider py-1">
                        {d}
                    </div>
                ))}
            </div>

            {/* Kalenderrutnät (6 rader × 7 kolumner) */}
            <div className="grid grid-cols-7 gap-px bg-slate-800/30 rounded-xl overflow-hidden border border-slate-800">
                {calendarDays.map((day, idx) => {
                    const dayShifts = getShiftsForDay(day);
                    const inCurrentMonth = day.getMonth() === month;
                    const today = isToday(day);

                    return (
                        <button
                            key={idx}
                            onClick={() => onDayClick(day)}
                            className={`relative p-2 min-h-[60px] md:min-h-[80px] flex flex-col items-center transition-colors
                                ${inCurrentMonth ? 'bg-slate-900/80 hover:bg-slate-800/80' : 'bg-slate-950/60'}
                                ${today ? 'ring-1 ring-inset ring-blue-500/40' : ''}
                            `}
                        >
                            {/* Dagnummer */}
                            <span className={`text-xs font-bold leading-none
                                ${today ? 'text-blue-400' : inCurrentMonth ? 'text-slate-300' : 'text-slate-600'}
                            `}>
                                {day.getDate()}
                            </span>

                            {/* Passprickar (max 4 synliga + "..." vid fler) */}
                            {dayShifts.length > 0 && (
                                <div className="flex flex-wrap gap-0.5 justify-center mt-auto pt-1">
                                    {dayShifts.slice(0, 4).map(shift => (
                                        <div
                                            key={shift.id}
                                            className={`w-1.5 h-1.5 rounded-full ${getDotColor(shift)}`}
                                        />
                                    ))}
                                    {dayShifts.length > 4 && (
                                        <span className="text-[8px] text-slate-500 font-bold leading-none">+{dayShifts.length - 4}</span>
                                    )}
                                </div>
                            )}
                        </button>
                    );
                })}
            </div>

            {/* Förklaring */}
            <div className="flex items-center gap-4 mt-3 justify-center">
                <div className="flex items-center gap-1.5">
                    <div className="w-2 h-2 rounded-full bg-blue-400"></div>
                    <span className="text-[10px] text-slate-400">Dina pass</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <div className="w-2 h-2 rounded-full bg-slate-500"></div>
                    <span className="text-[10px] text-slate-400">Andras pass</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <div className="w-2 h-2 rounded-full bg-yellow-400"></div>
                    <span className="text-[10px] text-slate-400">Kan bytas</span>
                </div>
            </div>
        </div>
    );
};

export default MonthView;
