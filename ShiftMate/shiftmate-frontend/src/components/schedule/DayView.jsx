import { isSameDay } from '../../utils/dateUtils';
import ShiftCard from './ShiftCard';

/**
 * DayView — Visar alla pass för en enskild dag med fullständiga kort.
 */
const DayView = ({ shifts, currentDate, currentUserId }) => {
    // Filtrera pass som startar på det valda datumet
    const dayShifts = shifts
        .filter(s => isSameDay(new Date(s.startTime), currentDate))
        .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

    if (dayShifts.length === 0) {
        return (
            <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                <p className="text-4xl mb-4">📋</p>
                <p className="text-slate-400 font-medium">Inga pass denna dag.</p>
            </div>
        );
    }

    return (
        <div className="grid gap-3">
            {dayShifts.map(shift => (
                <ShiftCard
                    key={shift.id}
                    shift={shift}
                    isOwn={shift.userId === currentUserId}
                />
            ))}
        </div>
    );
};

export default DayView;
