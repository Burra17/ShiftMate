import { useState, useRef, useEffect } from 'react';

// Generera timmar (00–23) och minuter i 5-minutersintervall
const HOURS = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, '0'));
const MINUTES = Array.from({ length: 12 }, (_, i) => String(i * 5).padStart(2, '0'));

const TimePicker = ({ value, onChange, label, required }) => {
    const [open, setOpen] = useState(false);
    const [step, setStep] = useState('hour'); // 'hour' eller 'minute'
    const [selectedHour, setSelectedHour] = useState('');
    const ref = useRef(null);

    // Synka intern state med externt value
    useEffect(() => {
        if (value) {
            const [h] = value.split(':');
            setSelectedHour(h);
        }
    }, [value]);

    // Stäng vid klick utanför
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (ref.current && !ref.current.contains(e.target)) {
                setOpen(false);
                setStep('hour');
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleHourClick = (h) => {
        setSelectedHour(h);
        setStep('minute');
    };

    const handleMinuteClick = (m) => {
        const newTime = `${selectedHour}:${m}`;
        onChange(newTime);
        setOpen(false);
        setStep('hour');
    };

    // Manuell inmatning — tillåt användaren att skriva direkt
    const handleInputChange = (e) => {
        onChange(e.target.value);
    };

    const displayValue = value || '';
    const currentHour = value ? value.split(':')[0] : '';
    const currentMinute = value ? value.split(':')[1] : '';

    return (
        <div className="space-y-1.5 relative" ref={ref}>
            {label && <label className="text-[11px] font-semibold text-slate-500 ml-1">{label}</label>}

            {/* Klickbar display som öppnar pickern */}
            <button
                type="button"
                onClick={() => { setOpen(!open); setStep('hour'); }}
                className={`w-full bg-slate-800/60 border text-left rounded-xl px-4 py-3 transition-all flex items-center justify-between group
                    ${open
                        ? 'border-purple-500/50 shadow-[0_0_12px_rgba(168,85,247,0.1)]'
                        : 'border-slate-700/60 hover:border-slate-600'}`}
            >
                <span className={displayValue ? 'text-white font-semibold text-lg tracking-wide' : 'text-slate-500'}>
                    {displayValue || 'HH:MM'}
                </span>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-slate-500 group-hover:text-slate-300 transition-colors">
                    <circle cx="12" cy="12" r="10" /><polyline points="12 6 12 12 16 14" />
                </svg>
            </button>

            {/* Dold input för formulärvalidering */}
            <input type="hidden" value={value || ''} required={required} />

            {/* Dropdown-panel */}
            {open && (
                <div className="absolute z-50 top-full left-0 right-0 mt-2 bg-slate-900 border border-slate-700/60 rounded-xl shadow-2xl shadow-black/40 overflow-hidden animate-fade-up">

                    {/* Header med flik-toggle */}
                    <div className="flex border-b border-slate-800">
                        <button
                            type="button"
                            onClick={() => setStep('hour')}
                            className={`flex-1 py-2 text-center text-[10px] font-bold uppercase tracking-wider transition-all
                                ${step === 'hour' ? 'text-purple-400 bg-purple-500/10' : 'text-slate-500 hover:text-slate-300'}`}
                        >
                            Timme {currentHour && <span className="text-white ml-1">{currentHour}</span>}
                        </button>
                        <button
                            type="button"
                            onClick={() => { if (selectedHour) setStep('minute'); }}
                            className={`flex-1 py-2 text-center text-[10px] font-bold uppercase tracking-wider transition-all
                                ${step === 'minute' ? 'text-purple-400 bg-purple-500/10' : 'text-slate-500 hover:text-slate-300'}
                                ${!selectedHour ? 'opacity-40 cursor-not-allowed' : ''}`}
                        >
                            Minut {currentMinute && <span className="text-white ml-1">{currentMinute}</span>}
                        </button>
                    </div>

                    {/* Timme-grid */}
                    {step === 'hour' && (
                        <div className="grid grid-cols-6 gap-px p-2">
                            {HOURS.map(h => (
                                <button
                                    key={h}
                                    type="button"
                                    onClick={() => handleHourClick(h)}
                                    className={`h-7 rounded-md text-xs font-semibold transition-all
                                        ${currentHour === h
                                            ? 'bg-purple-500/25 text-purple-300 border border-purple-500/40'
                                            : 'text-slate-300 hover:bg-slate-800 hover:text-white border border-transparent'}`}
                                >
                                    {h}
                                </button>
                            ))}
                        </div>
                    )}

                    {/* Minut-grid */}
                    {step === 'minute' && (
                        <div className="p-2 space-y-2">
                            {/* Vanliga 5-minutersintervall */}
                            <div className="grid grid-cols-6 gap-px">
                                {MINUTES.map(m => (
                                    <button
                                        key={m}
                                        type="button"
                                        onClick={() => handleMinuteClick(m)}
                                        className={`h-7 rounded-md text-xs font-semibold transition-all
                                            ${currentMinute === m
                                                ? 'bg-purple-500/25 text-purple-300 border border-purple-500/40'
                                                : 'text-slate-300 hover:bg-slate-800 hover:text-white border border-transparent'}`}
                                    >
                                        :{m}
                                    </button>
                                ))}
                            </div>
                            {/* Exakt minut-inmatning */}
                            <div className="flex items-center gap-2 pt-1 border-t border-slate-800">
                                <span className="text-[10px] text-slate-500 font-medium whitespace-nowrap">Exakt:</span>
                                <input
                                    type="number"
                                    min="0"
                                    max="59"
                                    placeholder="00"
                                    className="w-14 bg-slate-800/80 border border-slate-700/60 text-white text-center rounded-md px-2 py-1 text-xs font-semibold focus:border-purple-500 transition-all"
                                    onKeyDown={(e) => {
                                        if (e.key === 'Enter') {
                                            e.preventDefault();
                                            const val = e.target.value.padStart(2, '0');
                                            if (Number(val) >= 0 && Number(val) <= 59) {
                                                handleMinuteClick(val);
                                            }
                                        }
                                    }}
                                    onBlur={(e) => {
                                        const val = e.target.value.padStart(2, '0');
                                        if (e.target.value && Number(val) >= 0 && Number(val) <= 59) {
                                            handleMinuteClick(val);
                                        }
                                    }}
                                />
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default TimePicker;
