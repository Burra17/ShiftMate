import { useState, useEffect } from 'react';
import { fetchShifts, getCurrentUserId } from './api';
import ViewToggle from './components/schedule/ViewToggle';
import NavigationBar from './components/schedule/NavigationBar';
import DayView from './components/schedule/DayView';
import WeekView from './components/schedule/WeekView';
import MonthView from './components/schedule/MonthView';
import LoadingSpinner from './components/LoadingSpinner';

/**
 * Schedule — Orkestreringskomponent för schemavyn.
 * Hanterar vyläge (dag/vecka/månad), navigation och datahämtning.
 */
const Schedule = () => {
    const [viewMode, setViewMode] = useState('week');
    const [currentDate, setCurrentDate] = useState(new Date());
    const [allShifts, setAllShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const currentUserId = getCurrentUserId();

    // Hämta alla pass vid mount
    useEffect(() => {
        const loadShifts = async () => {
            try {
                const data = await fetchShifts();
                setAllShifts(data);
            } catch (err) {
                console.error("Kunde inte hämta schemat:", err);
                setError("Kunde inte ladda schemat just nu.");
            } finally {
                setLoading(false);
            }
        };
        loadShifts();
    }, []);

    // Klick på en dag i månadsvyn → byt till dagsvy
    const handleDayClick = (day) => {
        setCurrentDate(day);
        setViewMode('day');
    };

    if (loading) return <LoadingSpinner message="Laddar schema..." />;

    if (error) return (
        <div className="p-6 bg-red-900/20 border border-red-800 rounded-2xl text-center text-red-400 font-bold">
            {error}
        </div>
    );

    return (
        <div className="space-y-6">
            {/* Kontrollpanel: vyväljare + navigation */}
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <NavigationBar
                    viewMode={viewMode}
                    currentDate={currentDate}
                    onDateChange={setCurrentDate}
                />
                <ViewToggle viewMode={viewMode} onViewChange={setViewMode} />
            </div>

            {/* Aktiv vy */}
            {viewMode === 'day' && (
                <DayView
                    shifts={allShifts}
                    currentDate={currentDate}
                    currentUserId={currentUserId}
                />
            )}
            {viewMode === 'week' && (
                <WeekView
                    shifts={allShifts}
                    currentDate={currentDate}
                    currentUserId={currentUserId}
                />
            )}
            {viewMode === 'month' && (
                <MonthView
                    shifts={allShifts}
                    currentDate={currentDate}
                    currentUserId={currentUserId}
                    onDayClick={handleDayClick}
                />
            )}
        </div>
    );
};

export default Schedule;
