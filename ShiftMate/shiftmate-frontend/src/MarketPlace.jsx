import { useState, useEffect } from 'react';
import { fetchClaimableShifts, takeShift } from './api';
import { formatDate, formatTimeRange } from './utils/dateUtils';

const MarketPlace = () => {
    const [availableShifts, setAvailableShifts] = useState([]);
    const [loading, setLoading] = useState(true);

    // ---------------------------------------------------------
    // 1. HÄMTA DATA VID START
    // ---------------------------------------------------------
    useEffect(() => {
        const fetchAvailableShifts = async () => {
            try {
                setLoading(true);
                const data = await fetchClaimableShifts();
                setAvailableShifts(data);
            } catch (err) {
                console.error("Kunde inte hämta lediga pass:", err);
            } finally {
                setLoading(false);
            }
        };
        fetchAvailableShifts();
    }, []);

    // ---------------------------------------------------------
    // 2. HANTERA "TA PASS"
    // ---------------------------------------------------------
    const handleTakeShift = async (shiftId) => {
        if (!window.confirm("Vill du ta detta pass?")) return;

        try {
            await takeShift(shiftId);

            alert("Snyggt! Passet är nu ditt och syns i ditt schema. ✅");

            // Uppdatera listan lokalt så passet försvinner direkt från marknaden
            setAvailableShifts(prev => prev.filter(s => s.id !== shiftId));
        } catch (err) {
            // Vi hämtar felmeddelandet från backend (t.ex. "Du har redan ett pass denna dag")
            const errorMessage = err.response?.data?.message || "Okänt fel uppstod";
            alert(`Kunde inte ta passet: ${errorMessage}`);
        }
    };

    // ---------------------------------------------------------
    // 4. RENDERING (Laddningsvy)
    // ---------------------------------------------------------
    if (loading) return (
        <div className="p-10 text-center text-green-400 font-bold animate-pulse tracking-widest uppercase">
            Hämtar lediga pass...
        </div>
    );

    return (
        <div className="space-y-6">
            {/* Om listan är tom */}
            {availableShifts.length === 0 ? (
                <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                    <p className="text-4xl mb-4">🌴</p>
                    <p className="text-slate-400 font-medium">Inga lediga pass tillgängliga just nu.</p>
                </div>
            ) : (
                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {availableShifts.map((shift) => {
                        // LOGIK: 
                        // isOpenShift = Sant om ingen äger passet (UserId är null i databasen)
                        // Annars är det ett pass som en kollega lagt ut för byte.
                        const isOpenShift = !shift.userId;

                        return (
                            <div
                                key={shift.id}
                                className={`bg-slate-900/80 backdrop-blur-xl p-6 rounded-3xl border transition-all hover:scale-[1.01] group flex flex-col relative overflow-hidden
                                    ${isOpenShift ? 'border-blue-500/30' : 'border-amber-500/30'}`}
                            >
                                {/* Dekorativ färgkant på vänster sida */}
                                <div className={`absolute left-0 top-0 bottom-0 w-1.5 
                                    ${isOpenShift ? 'bg-blue-400 shadow-[0_0_20px_#60a5fa]' : 'bg-amber-400 shadow-[0_0_20px_#fbbf24]'}`}>
                                </div>

                                <div className="flex flex-col items-center text-center mb-6">
                                    {/* Dynamisk etikett baserat på om passet är nytt eller från en kollega */}
                                    <span className={`text-[10px] font-black px-4 py-1.5 rounded-full uppercase tracking-widest mb-4 border shadow-sm 
                                        ${isOpenShift
                                            ? 'text-blue-300 bg-blue-500/10 border-blue-400/30'
                                            : 'text-amber-300 bg-amber-500/10 border-amber-400/30'
                                        }`}
                                    >
                                        {isOpenShift ? '✨ ÖPPET PASS' : `👤 FRÅN ${shift.user?.firstName || 'KOLLEGA'}`}
                                    </span>

                                    <h3 className="text-2xl font-black text-white tracking-tight mb-1">
                                        {formatTimeRange(shift.startTime, shift.endTime)}
                                    </h3>

                                    <p className="text-xs font-bold text-slate-500 uppercase tracking-widest">
                                        {formatDate(shift.startTime)}
                                    </p>
                                </div>

                                {/* Knapp - Olika färg och ikon beroende på typ av pass */}
                                <button
                                    onClick={() => handleTakeShift(shift.id)}
                                    className={`w-full py-3 text-xs font-black rounded-xl transition-all uppercase tracking-widest flex justify-center items-center gap-2
                                        ${isOpenShift
                                            ? 'bg-blue-500/10 border border-blue-500/30 text-blue-400 hover:bg-blue-500 hover:text-white hover:shadow-[0_0_20px_rgba(96,165,250,0.4)]'
                                            : 'bg-amber-500/10 border border-amber-500/30 text-amber-400 hover:bg-amber-500 hover:text-white hover:shadow-[0_0_20px_rgba(251,191,36,0.4)]'
                                        }`}
                                >
                                    <span>{isOpenShift ? '🙋‍♂️' : '🤝'}</span>
                                    {isOpenShift ? 'TA PASSET' : 'TA ÖVER PASS'}
                                </button>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};

export default MarketPlace;