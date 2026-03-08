import { useState, useRef, useEffect } from 'react';

const WEEKDAYS = ['Mån', 'Tis', 'Ons', 'Tor', 'Fre', 'Lör', 'Sön'];
const MONTHS = ['Januari', 'Februari', 'Mars', 'April', 'Maj', 'Juni', 'Juli', 'Augusti', 'September', 'Oktober', 'November', 'December'];

const DatePicker = ({ value, onChange, label, required }) => {
    const [open, setOpen] = useState(false);
    const ref = useRef(null);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Vilken månad som visas i kalendern
    const [viewDate, setViewDate] = useState(() => {
        if (value) return new Date(value + 'T00:00:00');
        return new Date();
    });

    // Stäng vid klick utanför
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (ref.current && !ref.current.contains(e.target)) setOpen(false);
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    // Uppdatera vy när value ändras utifrån
    useEffect(() => {
        if (value) setViewDate(new Date(value + 'T00:00:00'));
    }, [value]);

    const viewYear = viewDate.getFullYear();
    const viewMonth = viewDate.getMonth();

    // Navigera månader
    const prevMonth = () => setViewDate(new Date(viewYear, viewMonth - 1, 1));
    const nextMonth = () => setViewDate(new Date(viewYear, viewMonth + 1, 1));
    const goToToday = () => {
        setViewDate(new Date());
        handleSelect(today);
    };

    // Bygg kalendergrid
    const getDays = () => {
        const firstDay = new Date(viewYear, viewMonth, 1);
        // Måndag = 0, Söndag = 6
        let startOffset = firstDay.getDay() - 1;
        if (startOffset < 0) startOffset = 6;

        const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();
        const daysInPrevMonth = new Date(viewYear, viewMonth, 0).getDate();

        const days = [];

        // Föregående månads dagar
        for (let i = startOffset - 1; i >= 0; i--) {
            days.push({ day: daysInPrevMonth - i, current: false, date: new Date(viewYear, viewMonth - 1, daysInPrevMonth - i) });
        }

        // Aktuell månads dagar
        for (let i = 1; i <= daysInMonth; i++) {
            days.push({ day: i, current: true, date: new Date(viewYear, viewMonth, i) });
        }

        // Fyll ut till 42 (6 rader)
        const remaining = 42 - days.length;
        for (let i = 1; i <= remaining; i++) {
            days.push({ day: i, current: false, date: new Date(viewYear, viewMonth + 1, i) });
        }

        return days;
    };

    const handleSelect = (date) => {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        onChange(`${y}-${m}-${d}`);
        setOpen(false);
    };

    const isSameDay = (a, b) => a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();

    const selectedDate = value ? new Date(value + 'T00:00:00') : null;

    // Formatera valt datum för display
    const formatDisplay = () => {
        if (!value) return null;
        const d = new Date(value + 'T00:00:00');
        return d.toLocaleDateString('sv-SE', { weekday: 'short', day: 'numeric', month: 'long' });
    };

    return (
        <div className="space-y-1.5 relative" ref={ref}>
            {label && <label className="text-[11px] font-semibold text-slate-500 ml-1">{label}</label>}

            {/* Klickbar display */}
            <button
                type="button"
                onClick={() => setOpen(!open)}
                className={`w-full bg-slate-800/60 border text-left rounded-xl px-4 py-3 transition-all flex items-center justify-between group
                    ${open
                        ? 'border-purple-500/50 shadow-[0_0_12px_rgba(168,85,247,0.1)]'
                        : 'border-slate-700/60 hover:border-slate-600'}`}
            >
                <span className={formatDisplay() ? 'text-white font-semibold' : 'text-slate-500'}>
                    {formatDisplay() || 'Välj datum'}
                </span>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-slate-500 group-hover:text-slate-300 transition-colors">
                    <rect width="18" height="18" x="3" y="4" rx="2" ry="2" /><line x1="16" x2="16" y1="2" y2="6" /><line x1="8" x2="8" y1="2" y2="6" /><line x1="3" x2="21" y1="10" y2="10" />
                </svg>
            </button>

            {/* Dold input för formulärvalidering */}
            <input type="hidden" value={value || ''} required={required} />

            {/* Kalender-dropdown */}
            {open && (
                <div className="absolute z-50 top-full left-0 mt-2 bg-slate-900 border border-slate-700/60 rounded-xl shadow-2xl shadow-black/40 overflow-hidden animate-fade-up w-[250px]">

                    {/* Månad-navigation */}
                    <div className="flex items-center justify-between px-3 py-2 border-b border-slate-800">
                        <button type="button" onClick={prevMonth} className="w-6 h-6 flex items-center justify-center rounded-md hover:bg-slate-800 text-slate-400 hover:text-white transition-all">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="15 18 9 12 15 6" /></svg>
                        </button>
                        <span className="text-xs font-bold text-white">{MONTHS[viewMonth]} {viewYear}</span>
                        <button type="button" onClick={nextMonth} className="w-6 h-6 flex items-center justify-center rounded-md hover:bg-slate-800 text-slate-400 hover:text-white transition-all">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="9 18 15 12 9 6" /></svg>
                        </button>
                    </div>

                    {/* Veckodagar */}
                    <div className="grid grid-cols-7 px-2 pt-2 pb-0.5">
                        {WEEKDAYS.map(d => (
                            <div key={d} className="text-center text-[9px] font-bold text-slate-500 uppercase tracking-wider py-0.5">{d}</div>
                        ))}
                    </div>

                    {/* Dagar */}
                    <div className="grid grid-cols-7 px-2 pb-1.5 gap-px">
                        {getDays().map((d, i) => {
                            const isToday = isSameDay(d.date, today);
                            const isSelected = selectedDate && isSameDay(d.date, selectedDate);
                            const isPast = d.date < today && !isToday;

                            return (
                                <button
                                    key={i}
                                    type="button"
                                    onClick={() => { handleSelect(d.date); if (!d.current) setViewDate(new Date(d.date.getFullYear(), d.date.getMonth(), 1)); }}
                                    className={`h-7 flex items-center justify-center rounded-md text-xs font-medium transition-all
                                        ${isSelected
                                            ? 'bg-purple-500/25 text-purple-300 border border-purple-500/40 font-bold'
                                            : isToday
                                                ? 'bg-blue-500/10 text-blue-400 border border-blue-500/25 font-bold'
                                                : d.current
                                                    ? isPast
                                                        ? 'text-slate-600 hover:bg-slate-800/60 hover:text-slate-400 border border-transparent'
                                                        : 'text-slate-300 hover:bg-slate-800 hover:text-white border border-transparent'
                                                    : 'text-slate-700 hover:bg-slate-800/40 hover:text-slate-500 border border-transparent'
                                        }`}
                                >
                                    {d.day}
                                </button>
                            );
                        })}
                    </div>

                    {/* Snabbknapp: Idag */}
                    <div className="px-2 pb-2 pt-0.5 border-t border-slate-800">
                        <button
                            type="button"
                            onClick={goToToday}
                            className="w-full py-1.5 text-[10px] font-bold text-blue-400 hover:text-blue-300 hover:bg-blue-500/10 rounded-md transition-all uppercase tracking-wider"
                        >
                            Idag
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default DatePicker;
