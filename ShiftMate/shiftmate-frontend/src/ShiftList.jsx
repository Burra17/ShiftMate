import { useState, useEffect } from 'react';
import api from './api'; // Importera default-instansen av axios från api.js

const ShiftList = () => {
    const [shifts, setShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [actionLoading, setActionLoading] = useState(null); // För knappar på korten

    // --- NY STATE FÖR MODALEN ---
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modalLoading, setModalLoading] = useState(false);
    const [modalError, setModalError] = useState(null);
    const [availableShifts, setAvailableShifts] = useState([]);
    const [selectedShift, setSelectedShift] = useState(null); // Passet jag vill byta BORT

    // Hämta användarens pass vid sidladdning
    useEffect(() => {
        const fetchShifts = async () => {
            try {
                // Använder direkt anrop med default-instansen - KORRIGERAD URL
                const response = await api.get('/shifts/mine');
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

    // Funktion för "LÄGG UT PASS"
    const handleInitiateSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
            // KORRIGERAD URL
            await api.post('/SwapRequests/initiate', { shiftId: shiftId });
            alert("Passet ligger nu ute för byte! 🎉");
            setShifts(prevShifts =>
                prevShifts.map(s => s.id === shiftId ? { ...s, isUpForSwap: true } : s)
            );
        } catch (err) {
            console.error("Kunde inte lägga ut passet:", err);
            alert(err.response?.data?.message || "Något gick fel. Kanske ligger passet redan ute?");
        } finally {
            setActionLoading(null);
        }
    };

    // Funktion för att ångra att ett pass ligger ute för byte
    const handleCancelSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
            // KORRIGERAD URL
            await api.put(`/shifts/${shiftId}/cancel-swap`);
            alert("Ditt pass är inte längre ute för byte.");
            setShifts(prevShifts =>
                prevShifts.map(s => s.id === shiftId ? { ...s, isUpForSwap: false } : s)
            );
        } catch (err) {
            console.error("Kunde inte ångra bytet:", err);
            alert(err.response?.data?.message || "Något gick fel när bytet skulle ångras.");
        } finally {
            setActionLoading(null);
        }
    };
    
    // --- NY LOGIK FÖR MODALEN ---

    // Öppnar modalen och hämtar alla pass
    const handleOpenModal = async (shiftToSwap) => {
        setSelectedShift(shiftToSwap);
        setIsModalOpen(true);
        setModalLoading(true);
        setModalError(null);

        try {
            // KORRIGERAD URL
            const response = await api.get('/shifts');
            const allShifts = response.data;
            const now = new Date();
            
            const currentUserId = shiftToSwap.userId; 

            const filteredShifts = allShifts.filter(s =>
                s.userId !== currentUserId &&
                new Date(s.endTime) > now &&
                !s.isUpForSwap
            );
            
            setAvailableShifts(filteredShifts);
        } catch (err) {
            console.error("Kunde inte hämta alla pass:", err);
            setModalError("Kunde inte ladda tillgängliga pass. Försök igen.");
        } finally {
            setModalLoading(false);
        }
    };

    // Skickar bytesförslaget
    const handleProposeSwap = async (targetShiftId) => {
        if (!selectedShift) return;
        setActionLoading(targetShiftId);
        
        try {
            // KORRIGERAD URL
            await api.post('/SwapRequests/propose-direct', { 
                myShiftId: selectedShift.id, 
                targetShiftId: targetShiftId 
            });
            alert("Förslag om direktbyte har skickats!");
            setIsModalOpen(false);
            setAvailableShifts([]);
        } catch (err) {
            console.error("Kunde inte föreslå byte:", err);
            alert(err.response?.data?.message || "Något gick fel när bytet skulle föreslås.");
        } finally {
            setActionLoading(null);
        }
    };


    // --- FORMATERINGSFUNKTIONER ---
    const formatDate = (dateString) => {
        if (!dateString) return "";
        const options = { weekday: 'short', day: 'numeric', month: 'short' };
        return new Date(dateString).toLocaleDateString('sv-SE', options).toUpperCase();
    };

    const formatTime = (dateString) => {
        if (!dateString) return "";
        return new Date(dateString).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
    };

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
                    <div key={shift.id} className="bg-slate-900/60 backdrop-blur-md p-6 rounded-3xl border border-slate-800 flex flex-col relative overflow-hidden transition-all hover:bg-slate-800/80 hover:border-slate-700">
                        <div className={`absolute left-0 top-0 bottom-0 w-1.5 ${shift.isUpForSwap ? 'bg-yellow-500 shadow-[0_0_15px_#eab308]' : 'bg-blue-500 shadow-[0_0_15px_#3b82f6]'}`}></div>
                        <div className="flex-1 pl-4">
                            <h3 className="text-2xl font-black text-white leading-tight tracking-tight flex items-center gap-2">
                                {formatTime(shift.startTime)}
                                <span className="text-slate-600 font-normal text-lg">→</span>
                                {formatTime(shift.endTime)}
                            </h3>
                            <span className="text-xs font-bold text-slate-400 uppercase tracking-widest">
                                {formatDate(shift.startTime)}
                            </span>
                             <div className="flex flex-wrap items-center gap-2 mt-2">
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

                        {/* Knappar */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6 pl-4">
                            {!shift.isUpForSwap ? (
                                <>
                                    <button
                                        onClick={() => handleInitiateSwap(shift.id)}
                                        disabled={actionLoading === shift.id}
                                        className="w-full py-3 bg-green-500/10 border border-green-500/30 text-green-400 hover:bg-green-500 hover:text-white hover:border-green-400 hover:shadow-[0_0_20px_rgba(74,222,128,0.3)] text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] uppercase tracking-widest disabled:opacity-50">
                                        {actionLoading === shift.id ? (
                                            <div className="flex items-center justify-center"><span className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin mr-2"></span>Publicerar...</div>
                                        ) : (
                                            "📤 LÄGG UT PASS"
                                        )}
                                    </button>
                                    <button
                                        onClick={() => handleOpenModal(shift)}
                                        disabled={actionLoading === shift.id}
                                        className="w-full py-3 bg-violet-500/10 border border-violet-500/30 text-violet-400 hover:bg-violet-500 hover:text-white hover:border-violet-400 hover:shadow-[0_0_20px_rgba(139,92,246,0.4)] text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] uppercase tracking-widest disabled:opacity-50">
                                        FÖRESLÅ BYTE
                                    </button>
                                </>
                            ) : (
                                <button
                                    onClick={() => handleCancelSwap(shift.id)}
                                    disabled={actionLoading === shift.id}
                                    className="w-full py-3 bg-yellow-500/10 border border-yellow-500/30 text-yellow-400 hover:bg-yellow-500 hover:text-white hover:border-yellow-400 hover:shadow-[0_0_20px_rgba(234,179,8,0.3)] text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] uppercase tracking-widest disabled:opacity-50 md:col-span-2">
                                    {actionLoading === shift.id ? (
                                        <div className="flex items-center justify-center"><span className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin mr-2"></span>Ångrar...</div>
                                    ) : (
                                        "↩️ ÅNGRA"
                                    )}
                                </button>
                            )}
                        </div>
                    </div>
                ))
            )}

            {isModalOpen && (
                 <div className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
                    <div className="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-lg max-h-[80vh] flex flex-col">
                        <div className="p-4 border-b border-slate-800 flex justify-between items-center">
                            <h2 className="text-white font-bold">Välj ett pass att byta med</h2>
                            <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-white text-xl">✕</button>
                        </div>

                        <div className="p-4 overflow-y-auto">
                            {modalLoading && <p className="text-slate-400 text-center animate-pulse">Laddar pass...</p>}
                            {modalError && <p className="text-red-400 text-center">{modalError}</p>}
                            
                            {!modalLoading && !modalError && (
                                <div className="space-y-2">
                                    {availableShifts.length > 0 ? (
                                        availableShifts.map(availShift => (
                                            <button 
                                                key={availShift.id}
                                                onClick={() => handleProposeSwap(availShift.id)}
                                                disabled={actionLoading === availShift.id}
                                                className="w-full text-left p-3 bg-slate-800/50 rounded-lg border border-slate-700 hover:bg-slate-700/80 hover:border-violet-500 transition-all disabled:opacity-50 flex justify-between items-center group">
                                                <div>
                                                    <p className="font-bold text-white">{availShift.user?.firstName} {availShift.user?.lastName}</p>
                                                    <p className="text-sm text-slate-400">{formatDate(availShift.startTime)} | {formatTime(availShift.startTime)} - {formatTime(availShift.endTime)}</p>
                                                </div>
                                                {actionLoading === availShift.id ? (
                                                    <span className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin"></span>
                                                ) : (
                                                    <span className="text-xs font-bold text-violet-400 opacity-0 group-hover:opacity-100 transition-opacity">VÄLJ</span>
                                                )}
                                            </button>
                                        ))
                                    ) : (
                                        <p className="text-slate-500 text-center p-8">Hittade inga pass att byta med just nu.</p>
                                    )}
                                </div>
                            )}
                        </div>
                    </div>
                 </div>
            )}
        </div>
    );
};

export default ShiftList;