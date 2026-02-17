import { useState } from 'react';
import { getMonday, addDays, isSameDay, isToday, getShortWeekday } from '../../utils/dateUtils';
import ShiftCard from './ShiftCard';

/**
 * WeekView — 7 kolumner på desktop, dagtabbar på mobil.
 */
const WeekView = ({ shifts, currentDate, currentUserId }) => {
    const monday = getMonday(currentDate);
    const weekDays = Array.from({ length: 7 }, (_, i) => addDays(monday, i));

    // Hitta dagens index (eller mån som default) för mobilvy
    const todayIndex = weekDays.findIndex(d => isToday(d));
    const [selectedDayIndex, setSelectedDayIndex] = useState(todayIndex >= 0 ? todayIndex : 0);

    const getShiftsForDay = (day) =>
        shifts
            .filter(s => isSameDay(new Date(s.startTime), day))
            .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

    const selectedDay = weekDays[selectedDayIndex];
    const selectedDayShifts = getShiftsForDay(selectedDay);

    return (
        <>
            {/* === MOBILVY: Dagtabbar + vald dags pass === */}
            <div className="md:hidden">
                {/* Dagtabbar */}
                <div className="flex gap-1 mb-4">
                    {weekDays.map((day, i) => {
                        const today = isToday(day);
                        const selected = i === selectedDayIndex;
                        const hasShifts = getShiftsForDay(day).length > 0;

                        return (
                            <button
                                key={day.toISOString()}
                                onClick={() => setSelectedDayIndex(i)}
                                className={`flex-1 flex flex-col items-center py-2 rounded-xl transition-all text-center
                                    ${selected
                                        ? 'bg-blue-600/15 border border-blue-500/30 shadow-[0_0_10px_rgba(59,130,246,0.1)]'
                                        : 'border border-transparent hover:bg-slate-800/50'
                                    }`}
                            >
                                <span className={`text-[9px] font-bold uppercase tracking-wider ${selected ? 'text-blue-400' : 'text-slate-500'}`}>
                                    {getShortWeekday(day)}
                                </span>
                                <span className={`text-base font-black mt-0.5 ${selected ? 'text-blue-400' : today ? 'text-blue-300' : 'text-white'}`}>
                                    {day.getDate()}
                                </span>
                                {hasShifts && (
                                    <div className={`w-1 h-1 rounded-full mt-1 ${selected ? 'bg-blue-400 shadow-[0_0_6px_#3b82f6]' : 'bg-slate-600'}`} />
                                )}
                            </button>
                        );
                    })}
                </div>

                {/* Vald dags pass */}
                <div className="space-y-2">
                    {selectedDayShifts.length > 0 ? (
                        selectedDayShifts.map(shift => (
                            <ShiftCard
                                key={shift.id}
                                shift={shift}
                                isOwn={shift.userId === currentUserId}
                            />
                        ))
                    ) : (
                        <p className="text-sm text-slate-500 text-center py-8">Inga pass denna dag</p>
                    )}
                </div>
            </div>

            {/* === DESKTOPVY: 7-kolumnsgrid (oförändrad) === */}
            <div className="hidden md:grid grid-cols-7 gap-3">
                {weekDays.map(day => {
                    const dayShifts = getShiftsForDay(day);
                    const today = isToday(day);

                    return (
                        <div key={day.toISOString()} className="min-w-0">
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
        </>
    );
};

export default WeekView;
