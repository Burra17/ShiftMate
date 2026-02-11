import { formatDayLabel, formatWeekLabel, formatMonthYear, addDays, addMonths, getMonday, isToday } from '../../utils/dateUtils';

// Pilikoner
const ChevronLeft = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m15 18-6-6 6-6" /></svg>
);
const ChevronRight = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m9 18 6-6-6-6" /></svg>
);

/**
 * NavigationBar — Navigera framåt/bakåt + "Idag"-knapp + periodlabel.
 */
const NavigationBar = ({ viewMode, currentDate, onDateChange }) => {
    // Beräkna periodlabel beroende på vy
    const getLabel = () => {
        if (viewMode === 'day') return formatDayLabel(currentDate);
        if (viewMode === 'week') return formatWeekLabel(getMonday(currentDate));
        return formatMonthYear(currentDate);
    };

    // Stega framåt/bakåt
    const step = (direction) => {
        const dir = direction === 'next' ? 1 : -1;
        if (viewMode === 'day') return onDateChange(addDays(currentDate, dir));
        if (viewMode === 'week') return onDateChange(addDays(currentDate, dir * 7));
        return onDateChange(addMonths(currentDate, dir));
    };

    // Hoppa till idag
    const goToday = () => onDateChange(new Date());

    // Visa "Idag"-knappen bara om vi inte redan är på dagens datum
    const showTodayBtn = !isToday(currentDate);

    return (
        <div className="flex items-center gap-3 flex-wrap">
            <div className="flex items-center gap-1">
                <button onClick={() => step('prev')} className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors">
                    <ChevronLeft />
                </button>
                <button onClick={() => step('next')} className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors">
                    <ChevronRight />
                </button>
            </div>

            <h3 className="text-lg font-bold text-white tracking-tight">
                {getLabel()}
            </h3>

            {showTodayBtn && (
                <button
                    onClick={goToday}
                    className="px-3 py-1 rounded-lg text-xs font-bold text-blue-400 border border-blue-500/30 hover:bg-blue-600/10 transition-colors"
                >
                    Idag
                </button>
            )}
        </div>
    );
};

export default NavigationBar;
