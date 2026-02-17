import { isSameDay } from '../../utils/dateUtils';
import ShiftCard from './ShiftCard';
import EmptyState from '../EmptyState';

/**
 * DayView — Visar alla pass för en enskild dag med fullständiga kort.
 */
const DayView = ({ shifts, currentDate, currentUserId }) => {
    // Filtrera pass som startar på det valda datumet
    const dayShifts = shifts
        .filter(s => isSameDay(new Date(s.startTime), currentDate))
        .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

    if (dayShifts.length === 0) {
        return <EmptyState icon="📋" message="Inga pass denna dag." />;
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
