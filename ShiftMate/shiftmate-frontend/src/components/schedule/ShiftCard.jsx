import { formatTime } from '../../utils/dateUtils';

// Hämtar initialer från namn (eller email som fallback)
const getInitials = (user) => {
    if (!user) return "??";
    if (user.firstName && user.lastName) {
        return (user.firstName[0] + user.lastName[0]).toUpperCase();
    }
    return user.email.substring(0, 2).toUpperCase();
};

// Visar hela namnet snyggt
const getFullName = (user) => {
    if (!user) return 'Okänd kollega';
    if (user.firstName && user.lastName) return `${user.firstName} ${user.lastName}`;
    return user.firstName || user.email.split('@')[0];
};

/**
 * ShiftCard — Visar ett pass med avatar, namn, tid och bytestatus.
 * compact=true: Kompakt variant för veckovyn (mindre padding, ingen avatar).
 * isOwn=true: Blå accentkant + tonad bakgrund för användarens egna pass.
 */
const ShiftCard = ({ shift, isOwn = false, compact = false }) => {
    const ownStyles = isOwn
        ? 'border-blue-500/40 bg-blue-950/20'
        : 'border-slate-800 bg-slate-900/60';

    if (compact) {
        return (
            <div className={`px-2 py-1.5 rounded-lg border text-xs ${ownStyles} hover:bg-slate-800/60 transition-colors`}>
                <p className={`font-bold truncate ${isOwn ? 'text-blue-300' : 'text-white'}`}>
                    {getFullName(shift.user)}
                </p>
                <p className="text-slate-400">
                    {formatTime(shift.startTime)} - {formatTime(shift.endTime)}
                </p>
                {shift.isUpForSwap && (
                    <span className="text-[9px] font-bold text-yellow-400">BYTES</span>
                )}
            </div>
        );
    }

    return (
        <div className={`backdrop-blur-md p-4 rounded-2xl border flex items-center justify-between hover:bg-slate-800/60 transition-all group ${ownStyles}`}>
            <div className="flex items-center gap-3">
                {/* Avatar med initialer */}
                <div className={`w-10 h-10 rounded-full flex items-center justify-center shadow-lg text-white font-bold text-xs border ${isOwn
                        ? 'bg-gradient-to-br from-blue-600 to-cyan-600 border-blue-400/30 shadow-blue-900/40'
                        : 'bg-gradient-to-br from-purple-600 to-pink-600 border-purple-400/30 shadow-purple-900/40'
                    }`}>
                    {getInitials(shift.user)}
                </div>

                <div>
                    <p className={`font-bold text-sm capitalize ${isOwn ? 'text-blue-200' : 'text-white'}`}>
                        {getFullName(shift.user)}
                    </p>
                    <div className="flex items-center gap-2 mt-0.5">
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
                <p className="text-lg font-black text-white tracking-tight">
                    {formatTime(shift.startTime)}
                </p>
                <p className="text-xs font-bold text-slate-500">
                    {formatTime(shift.endTime)}
                </p>
            </div>
        </div>
    );
};

export default ShiftCard;
