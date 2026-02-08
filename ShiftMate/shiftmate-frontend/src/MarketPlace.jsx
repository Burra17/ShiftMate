import { useState, useEffect } from 'react';
import api from './api';

const MarketPlace = () => {
    const [availableShifts, setAvailableShifts] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchAvailableShifts = async () => {
            try {
                // Vi hämtar "claimable" som innehåller både öppna pass och bytes-pass
                const response = await api.get('/Shifts/claimable');
                setAvailableShifts(response.data);
            } catch (err) {
                console.error("Kunde inte hämta lediga pass:", err);
            } finally {
                setLoading(false);
            }
        };
        fetchAvailableShifts();
    }, []);

    const handleTakeShift = async (shiftId) => {
        try {
            const url = `/Shifts/${shiftId}/take`;
            await api.put(url, {});

            alert("Snyggt! Passet är nu ditt och syns i ditt schema. ✅");
            // Uppdatera listan direkt utan att ladda om
            setAvailableShifts(prev => prev.filter(s => s.id !== shiftId));
        } catch (err) {
            const errorMessage = err.response?.data?.message || "Okänt fel";
            alert(`Kunde inte ta passet: ${errorMessage}`);
        }
    };

    const formatDate = (dateStr) => {
        if (!dateStr) return "OKÄNT DATUM";
        return new Date(dateStr).toLocaleDateString('sv-SE', { weekday: 'short', day: 'numeric', month: 'short' }).toUpperCase();
    };

    const formatTime = (startStr, endStr) => {
        if (!startStr || !endStr) return "--:--";
        const start = new Date(startStr).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
        const end = new Date(endStr).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
        return `${start} - ${end}`;
    };

    if (loading) return <div className="p-10 text-center text-green-400 font-bold animate-pulse tracking-widest">HÄMTAR MARKNADEN...</div>;

    return (
        <div className="space-y-6">
            {availableShifts.length === 0 ? (
                <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                    <p className="text-4xl mb-4">🌴</p>
                    <p className="text-slate-400 font-medium">Inga lediga pass just nu.</p>
                </div>
            ) : (
                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {availableShifts.map((shift) => {
                        // Logik: Om shift.userId är null så är det ett "Öppet pass".
                        // Om det finns ett userId så är det en kollega som lagt ut det.
                        const isOpenShift = !shift.userId;

                        return (
                            <div key={shift.id} className={`bg-slate-900/80 backdrop-blur-xl p-6 rounded-3xl border ${isOpenShift ? 'border-blue-500/30' : 'border-amber-500/30'} flex flex-col relative overflow-hidden transition-all hover:scale-[1.02] group`}>

                                {/* Färgkodad kant: Blå för öppet, Bärnsten (Amber) för kollegor */}
                                <div className={`absolute left-0 top-0 bottom-0 w-1.5 ${isOpenShift ? 'bg-blue-400 shadow-[0_0_20px_#60a5fa]' : 'bg-amber-400 shadow-[0_0_20px_#fbbf24]'}`}></div>

                                <div className="flex flex-col items-center text-center mb-6">
                                    {/* Etikett: Här ändrade vi texten! */}
                                    <span className={`text-[10px] font-black px-4 py-1.5 rounded-full uppercase tracking-widest mb-4 border shadow-sm ${isOpenShift
                                            ? 'text-blue-300 bg-blue-500/10 border-blue-400/30'
                                            : 'text-amber-300 bg-amber-500/10 border-amber-400/30'
                                        }`}>
                                        {isOpenShift ? '✨ NYTT PASS' : `👤 FRÅN ${shift.user?.firstName || 'KOLLEGA'}`}
                                    </span>

                                    <h3 className="text-2xl font-black text-white tracking-tight mb-1">
                                        {formatTime(shift.startTime, shift.endTime)}
                                    </h3>

                                    <p className="text-xs font-bold text-slate-500 uppercase tracking-widest">
                                        {formatDate(shift.startTime)}
                                    </p>
                                </div>

                                <button
                                    onClick={() => handleTakeShift(shift.id)}
                                    className={`w-full py-3 text-xs font-black rounded-xl transition-all uppercase tracking-widest flex justify-center items-center gap-2
                                        ${isOpenShift
                                            ? 'bg-blue-500/10 border border-blue-500/30 text-blue-400 hover:bg-blue-500 hover:text-white hover:shadow-[0_0_20px_rgba(96,165,250,0.4)]'
                                            : 'bg-amber-500/10 border border-amber-500/30 text-amber-400 hover:bg-amber-500 hover:text-white hover:shadow-[0_0_20px_rgba(251,191,36,0.4)]'
                                        }`}
                                >
                                    <span>{isOpenShift ? '🙋‍♂️' : '🤝'}</span> {isOpenShift ? 'TA PASSET' : 'TA ÖVER'}
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