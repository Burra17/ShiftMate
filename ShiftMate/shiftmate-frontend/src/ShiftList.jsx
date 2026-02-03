import { useState, useEffect } from 'react';
import api from './api';

const ShiftList = () => {
    // --- BEHÅLLEN LOGIK (Rör ej) ---
    const [shifts, setShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [actionLoading, setActionLoading] = useState(null);

    useEffect(() => {
        const fetchShifts = async () => {
            try {
                const response = await api.get('/Shifts/mine');
                setShifts(response.data);
            } catch (err) {
                console.error("Kunde inte hämta pass:", err);
                setError("Kunde inte ladda dina pass just nu.");
            } finally {
                setLoading(false);
            }
        };
        fetchShifts();
    }, []);

    const handleInitiateSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
            await api.post('/SwapRequests/initiate', { shiftId: shiftId });

            alert("Passet ligger nu ute för byte! 🎉");

            setShifts(prevShifts =>
                prevShifts.map(s => s.id === shiftId ? { ...s, isUpForSwap: true } : s)
            );
        } catch (err) {
            console.error("Kunde inte lägga ut passet:", err);
            alert("Något gick fel. Kanske ligger passet redan ute?");
        } finally {
            setActionLoading(null);
        }
    };

    // Funktion för att ångra att ett pass ligger ute för byte
    const handleCancelSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
            // Anropa den nya endpointen för att ångra
            await api.put(`/Shifts/${shiftId}/cancel-swap`, {});

            alert("Ditt pass är inte längre ute för byte.");

            // Uppdatera state för att reflektera ändringen direkt i UI
            setShifts(prevShifts =>
                prevShifts.map(s => s.id === shiftId ? { ...s, isUpForSwap: false } : s)
            );
        } catch (err) {
            console.error("Kunde inte ångra bytet:", err);
            alert("Något gick fel när bytet skulle ångras.");
        } finally {
            setActionLoading(null);
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return "";
        const options = { weekday: 'short', day: 'numeric', month: 'short' };
        return new Date(dateString).toLocaleDateString('sv-SE', options).toUpperCase();
    };

    const formatTime = (dateString) => {
        if (!dateString) return "";
        return new Date(dateString).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
    };

    // --- NY DESIGN (Dark Mode) ---

    if (loading) return <div className="p-10 text-center text-blue-400 font-bold animate-pulse tracking-widest">HÄMTAR SCHEMA...</div>;
    if (error) return <div className="p-6 bg-red-900/20 border border-red-800 rounded-2xl text-center text-red-400 font-bold">{error}</div>;

    return (
        <div className="space-y-6">

            {shifts.length === 0 ? (
                <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                    <p className="text-4xl mb-4">💤</p>
                    <p className="text-slate-400 font-medium">Du har inga inbokade pass just nu.</p>
                </div>
            ) : (
                shifts.map((shift) => (
                    <div
                        key={shift.id}
                        className="bg-slate-900/60 backdrop-blur-md p-6 rounded-3xl border border-slate-800 flex flex-col relative overflow-hidden transition-all hover:bg-slate-800/80 hover:border-slate-700 hover:scale-[1.01] hover:shadow-[0_0_20px_rgba(59,130,246,0.1)] group"
                    >
                        {/* Neon-kant (Blå för vanliga, Gul för de som byts bort) */}
                        <div className={`absolute left-0 top-0 bottom-0 w-1.5 ${shift.isUpForSwap ? 'bg-yellow-500 shadow-[0_0_15px_#eab308]' : 'bg-blue-500 shadow-[0_0_15px_#3b82f6]'}`}></div>

                        <div className="flex justify-between items-start pl-4">
                            <div className="flex-1">
                                <div className="flex items-center gap-2 mb-1">
                                    <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                                        {formatDate(shift.startTime)}
                                    </span>
                                </div>

                                <h3 className="text-2xl font-black text-white leading-tight tracking-tight flex items-center gap-2">
                                    {formatTime(shift.startTime)}
                                    <span className="text-slate-600 font-normal text-lg">→</span>
                                    {formatTime(shift.endTime)}
                                </h3>

                                <div className="flex flex-wrap items-center gap-2 mt-4">
                                    <span className="text-xs font-bold text-slate-300 bg-slate-800 px-3 py-1.5 rounded-lg border border-slate-700">
                                        ⏱ {shift.durationHours}h
                                    </span>

                                    {shift.isUpForSwap && (
                                        <span className="text-xs font-bold text-yellow-400 bg-yellow-500/10 px-3 py-1.5 rounded-lg border border-yellow-500/20 flex items-center gap-1 shadow-[0_0_10px_rgba(234,179,8,0.1)]">
                                            🔄 Ute för byte
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Knapp - Byt ut pass */}
                        {!shift.isUpForSwap && (
                            <div className="mt-6 pl-4">
                                <button
                                    onClick={() => handleInitiateSwap(shift.id)}
                                    disabled={actionLoading === shift.id}
                                    className="w-full py-3 
                                    bg-green-500/10 border border-green-500/30 text-green-400 
                                    hover:bg-green-500 hover:text-white hover:border-green-400 hover:shadow-[0_0_30px_rgba(74,222,128,0.4)]
                                    text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] 
                                    uppercase tracking-widest flex justify-center items-center gap-2 
                                    shadow-[0_0_15px_rgba(74,222,128,0.1)] disabled:opacity-50"
                                >
                                    {actionLoading === shift.id ? (
                                        <>
                                            <span className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin"></span>
                                            Publicerar...
                                        </>
                                    ) : (
                                        <>
                                            📤 LÄGG UT PASS
                                        </>
                                    )}
                                </button>
                            </div>
                        )}
                        
                        {/* Knapp - Ångra byte */}
                        {shift.isUpForSwap && (
                            <div className="mt-6 pl-4">
                                <button
                                    onClick={() => handleCancelSwap(shift.id)}
                                    disabled={actionLoading === shift.id}
                                    className="w-full py-3 
                                    bg-yellow-500/10 border border-yellow-500/30 text-yellow-400 
                                    hover:bg-yellow-500 hover:text-white hover:border-yellow-400 hover:shadow-[0_0_30px_rgba(234,179,8,0.4)]
                                    text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] 
                                    uppercase tracking-widest flex justify-center items-center gap-2 
                                    shadow-[0_0_15px_rgba(234,179,8,0.1)] disabled:opacity-50"
                                >
                                    {actionLoading === shift.id ? (
                                        <>
                                            <span className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin"></span>
                                            Ångrar...
                                        </>
                                    ) : (
                                        <>
                                            ↩️ ÅNGRA
                                        </>
                                    )}
                                </button>
                            </div>
                        )}
                    </div>
                ))
            )}
        </div>
    );
};

export default ShiftList;