import { useState, useRef, useEffect } from 'react';

const UserSelect = ({ users, value, onChange }) => {
    const [open, setOpen] = useState(false);
    const [search, setSearch] = useState('');
    const ref = useRef(null);
    const searchRef = useRef(null);

    // Stäng vid klick utanför
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (ref.current && !ref.current.contains(e.target)) {
                setOpen(false);
                setSearch('');
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    // Fokusera sökfält när dropdown öppnas
    useEffect(() => {
        if (open && searchRef.current) {
            searchRef.current.focus();
        }
    }, [open]);

    const selectedUser = users.find(u => u.id === value);

    const filteredUsers = users.filter(u => {
        if (!search) return true;
        const fullName = `${u.firstName} ${u.lastName}`.toLowerCase();
        return fullName.includes(search.toLowerCase());
    });

    const handleSelect = (userId) => {
        onChange(userId);
        setOpen(false);
        setSearch('');
    };

    // Initialer för avatar
    const getInitials = (user) => {
        return `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`;
    };

    return (
        <div className="relative" ref={ref}>
            {/* Klickbar display */}
            <button
                type="button"
                onClick={() => setOpen(!open)}
                className={`w-full bg-slate-800/60 border text-left rounded-xl px-4 py-3 transition-all flex items-center gap-3
                    ${open
                        ? 'border-purple-500/50 shadow-[0_0_12px_rgba(168,85,247,0.1)]'
                        : 'border-slate-700/60 hover:border-slate-600'}`}
            >
                {selectedUser ? (
                    <>
                        <div className="w-8 h-8 rounded-lg bg-blue-500/15 border border-blue-500/25 flex items-center justify-center shrink-0">
                            <span className="text-[10px] font-bold text-blue-400">{getInitials(selectedUser)}</span>
                        </div>
                        <span className="text-white font-semibold text-sm flex-1">{selectedUser.firstName} {selectedUser.lastName}</span>
                    </>
                ) : (
                    <>
                        <div className="w-8 h-8 rounded-lg bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center shrink-0">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-emerald-400">
                                <circle cx="12" cy="12" r="10" /><line x1="12" x2="12" y1="8" y2="16" /><line x1="8" x2="16" y1="12" y2="12" />
                            </svg>
                        </div>
                        <span className="text-slate-400 text-sm flex-1">Öppet pass (ingen ägare)</span>
                    </>
                )}
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={`text-slate-500 transition-transform ${open ? 'rotate-180' : ''}`}>
                    <polyline points="6 9 12 15 18 9" />
                </svg>
            </button>

            {/* Dropdown */}
            {open && (
                <div className="absolute z-50 top-full left-0 right-0 mt-2 bg-slate-900 border border-slate-700/60 rounded-2xl shadow-2xl shadow-black/40 overflow-hidden animate-fade-up">

                    {/* Sökfält */}
                    {users.length > 5 && (
                        <div className="p-3 border-b border-slate-800">
                            <div className="relative">
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500">
                                    <circle cx="11" cy="11" r="8" /><path d="m21 21-4.3-4.3" />
                                </svg>
                                <input
                                    ref={searchRef}
                                    type="text"
                                    value={search}
                                    onChange={(e) => setSearch(e.target.value)}
                                    placeholder="Sök personal..."
                                    className="w-full bg-slate-800/60 border border-slate-700/60 text-white rounded-lg pl-9 pr-3 py-2 text-sm focus:border-purple-500 transition-all placeholder-slate-500"
                                />
                            </div>
                        </div>
                    )}

                    {/* Alternativ */}
                    <div className="max-h-56 overflow-y-auto py-1">
                        {/* Öppet pass */}
                        <button
                            type="button"
                            onClick={() => handleSelect('')}
                            className={`w-full flex items-center gap-3 px-4 py-2.5 text-left transition-all
                                ${!value ? 'bg-emerald-500/10 text-emerald-300' : 'text-slate-400 hover:bg-slate-800/60 hover:text-white'}`}
                        >
                            <div className="w-7 h-7 rounded-lg bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center shrink-0">
                                <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="text-emerald-400">
                                    <circle cx="12" cy="12" r="10" /><line x1="12" x2="12" y1="8" y2="16" /><line x1="8" x2="16" y1="12" y2="12" />
                                </svg>
                            </div>
                            <span className="text-sm font-medium">Öppet pass</span>
                            {!value && (
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="text-emerald-400 ml-auto">
                                    <polyline points="20 6 9 17 4 12" />
                                </svg>
                            )}
                        </button>

                        {/* Separator */}
                        <div className="mx-3 my-1 border-t border-slate-800"></div>

                        {/* Användare */}
                        {filteredUsers.length === 0 ? (
                            <p className="text-sm text-slate-500 text-center py-4">Ingen personal hittades</p>
                        ) : (
                            filteredUsers.map(user => (
                                <button
                                    key={user.id}
                                    type="button"
                                    onClick={() => handleSelect(user.id)}
                                    className={`w-full flex items-center gap-3 px-4 py-2.5 text-left transition-all
                                        ${value === user.id ? 'bg-blue-500/10 text-blue-300' : 'text-slate-300 hover:bg-slate-800/60 hover:text-white'}`}
                                >
                                    <div className={`w-7 h-7 rounded-lg flex items-center justify-center shrink-0 border
                                        ${value === user.id ? 'bg-blue-500/15 border-blue-500/30' : 'bg-slate-800 border-slate-700/60'}`}>
                                        <span className="text-[10px] font-bold">{getInitials(user)}</span>
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <span className="text-sm font-medium block truncate">{user.firstName} {user.lastName}</span>
                                        <span className="text-[11px] text-slate-500 block truncate">{user.role}</span>
                                    </div>
                                    {value === user.id && (
                                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="text-blue-400 shrink-0">
                                            <polyline points="20 6 9 17 4 12" />
                                        </svg>
                                    )}
                                </button>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

export default UserSelect;
