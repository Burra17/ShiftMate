// Komponent för notifikationsklocka med dropdown-panel
import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { formatTime, formatDate } from '../utils/dateUtils';

// Klockikon (SVG inline, samma mönster som Icons.* i App.jsx)
const BellIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
        fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
        <path d="M13.73 21a2 2 0 0 1-3.46 0" />
    </svg>
);

/**
 * NotificationDropdown — visar inkommande bytesförfrågningar i en dropdown-panel.
 * Props:
 *   requests      - SwapRequestDto[] från senaste poll
 *   hasUnseen     - bool, styr om röd badge visas
 *   onOpen        - callback som nollställer badge när dropdownen öppnas
 *   align         - 'left' | 'right' (default 'right'), styr åt vilket håll panelen öppnas
 */
const NotificationDropdown = ({ requests, hasUnseen, onOpen, align = 'right' }) => {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef(null);

    // Öppna/stäng dropdown; rensa badge vid öppning
    const handleToggle = () => {
        const opening = !isOpen;
        setIsOpen(opening);
        if (opening) {
            onOpen();
        }
    };

    // Stäng vid klick utanför dropdownen
    useEffect(() => {
        if (!isOpen) return;

        const handleMouseDown = (e) => {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
                setIsOpen(false);
            }
        };

        document.addEventListener('mousedown', handleMouseDown);
        return () => document.removeEventListener('mousedown', handleMouseDown);
    }, [isOpen]);

    // Stäng vid Escape-tangenten
    useEffect(() => {
        if (!isOpen) return;

        const handleKeyDown = (e) => {
            if (e.key === 'Escape') setIsOpen(false);
        };

        document.addEventListener('keydown', handleKeyDown);
        return () => document.removeEventListener('keydown', handleKeyDown);
    }, [isOpen]);

    return (
        // Relativ wrapper för att förankra dropdownen
        <div ref={dropdownRef} className="relative">

            {/* Klockknapp */}
            <button
                onClick={handleToggle}
                className="relative p-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 transition-all"
                aria-label="Notifikationer"
            >
                <BellIcon />
                {/* Röd badge — visas bara när hasUnseen && requests.length > 0 */}
                {hasUnseen && requests.length > 0 && (
                    <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 rounded-full text-[10px] font-black text-white flex items-center justify-center shadow-[0_0_10px_rgba(239,68,68,0.5)]">
                        {requests.length > 9 ? '9+' : requests.length}
                    </span>
                )}
            </button>

            {/* Dropdown-panel — align styr om panelen öppnas åt vänster eller höger */}
            {isOpen && (
                <div className={`absolute ${align === 'left' ? 'left-0' : 'right-0'} top-full mt-2 w-80 bg-slate-900 border border-slate-700 rounded-2xl shadow-2xl z-50 overflow-hidden`}>
                    {/* Rubrik */}
                    <div className="px-4 py-3 border-b border-slate-800 flex items-center justify-between">
                        <h3 className="text-xs font-black text-white uppercase tracking-widest">Inkommande förfrågningar</h3>
                        <span className="text-xs font-bold text-slate-500">{requests.length} st</span>
                    </div>

                    {/* Lista */}
                    <div className="max-h-80 overflow-y-auto">
                        {requests.length === 0 ? (
                            <div className="px-4 py-8 text-center">
                                <p className="text-slate-500 text-sm font-bold">Inga inkommande förfrågningar</p>
                            </div>
                        ) : (
                            requests.map(req => (
                                <Link key={req.id} to="/mine" onClick={() => setIsOpen(false)}
                                    className="block px-4 py-3 border-b border-slate-800/50 hover:bg-slate-800/50 transition-colors">
                                    <p className="text-sm font-bold text-white">
                                        {req.requestingUser?.firstName} {req.requestingUser?.lastName}
                                    </p>
                                    <p className="text-xs text-slate-400 mt-0.5">
                                        {formatTime(req.shift?.startTime)} – {formatTime(req.shift?.endTime)}
                                        <span className="ml-1 text-slate-500">({formatDate(req.shift?.startTime)})</span>
                                    </p>
                                </Link>
                            ))
                        )}
                    </div>

                    {/* Footer */}
                    <div className="px-4 py-3 border-t border-slate-800">
                        <Link
                            to="/mine"
                            onClick={() => setIsOpen(false)}
                            className="block text-center text-xs font-black text-blue-400 hover:text-blue-300 uppercase tracking-widest transition-colors"
                        >
                            Visa alla →
                        </Link>
                    </div>
                </div>
            )}
        </div>
    );
};

export default NotificationDropdown;
