import { useState, useEffect } from 'react';
import api from './api'; // Importera default-instansen av axios från api.js

const ShiftList = () => {
    // Original states
    const [shifts, setShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [actionLoading, setActionLoading] = useState(null);

    // State för modalen
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modalLoading, setModalLoading] = useState(false);
    const [modalError, setModalError] = useState(null);
    const [availableShifts, setAvailableShifts] = useState([]);
    const [selectedShift, setSelectedShift] = useState(null);

    // --- NY STATE FÖR INKOMMANDE FÖRFRÅGNINGAR ---
    const [pendingRequests, setPendingRequests] = useState([]);
    const [requestsLoading, setRequestsLoading] = useState(true);

    const fetchShifts = async () => {
        try {
            const response = await api.get('/shifts/mine');
            setShifts(response.data);
        } catch (err) {
            console.error("Kunde inte hämta pass:", err);
            setError("Kunde inte ladda dina pass just nu.");
        } finally {
            setLoading(false);
        }
    };
    
    const fetchReceivedRequests = async () => {
        try {
            setRequestsLoading(true);
            const response = await api.get('/swaprequests/received');
            setPendingRequests(response.data);
        } catch (err) {
            console.error("Kunde inte hämta inkommande förfrågningar:", err);
            // Visa inte ett stort felmeddelande här, det kan vara tomt
        } finally {
            setRequestsLoading(false);
        }
    };

    // Hämta all data vid sidladdning
    useEffect(() => {
        fetchShifts();
        fetchReceivedRequests();
    }, []);

    // --- HANDLER FÖR ATT SVARA PÅ FÖRFRÅGNINGAR ---
    const handleDecline = async (requestId) => {
        setActionLoading(requestId);
        try {
            await api.post(`/swaprequests/${requestId}/decline`);
            alert("Förfrågan har nekats.");
            // Ta bort från listan i UI
            setPendingRequests(prev => prev.filter(r => r.id !== requestId));
        } catch (err) {
            console.error("Kunde inte neka förfrågan:", err);
            alert(err.response?.data?.message || "Något gick fel.");
        } finally {
            setActionLoading(null);
        }
    };
    
    const handleAccept = async (requestId) => {
        setActionLoading(requestId);
        try {
            await api.post('/swaprequests/accept', { swapRequestId: requestId });
            alert("Bytet har accepterats! Ditt schema uppdateras.");
            // Ta bort från listan och ladda om allt
            setPendingRequests(prev => prev.filter(r => r.id !== requestId));
            fetchShifts(); // Ladda om dina pass
        } catch (err) {
            console.error("Kunde inte acceptera bytet:", err);
            alert(err.response?.data?.message || "Något gick fel. Kanske krockar passen?");
        } finally {
            setActionLoading(null);
        }
    };


    // Funktion för "LÄGG UT PASS"
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
            alert(err.response?.data?.message || "Något gick fel. Kanske ligger passet redan ute?");
        } finally {
            setActionLoading(null);
        }
    };

    // Funktion för att ångra att ett pass ligger ute för byte
    const handleCancelSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
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
    
    // --- Logik för modalen ---
    const handleOpenModal = async (shiftToSwap) => {
        setSelectedShift(shiftToSwap);
        setIsModalOpen(true);
        setModalLoading(true);
        setModalError(null);

        try {
            const response = await api.get('/shifts');
            const allShifts = response.data;
            const now = new Date();
            const currentUserId = shiftToSwap.userId; 

            const filteredShifts = allShifts.filter(s =>
                s.userId !== currentUserId && new Date(s.endTime) > now && !s.isUpForSwap
            );
            
            setAvailableShifts(filteredShifts);
        } catch (err) {
            console.error("Kunde inte hämta alla pass:", err);
            setModalError("Kunde inte ladda tillgängliga pass. Försök igen.");
        } finally {
            setModalLoading(false);
        }
    };

    const handleProposeSwap = async (targetShiftId) => {
        if (!selectedShift) return;
        setActionLoading(targetShiftId);
        
        try {
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

    // --- Formateringsfunktioner ---
    const formatShiftTime = (shift) => {
        if (!shift?.startTime || !shift?.endTime) return "Okänd tid";
        return `${formatTime(shift.startTime)} - ${formatTime(shift.endTime)}`;
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

    // --- Renderingslogik ---
    if (loading || requestsLoading) return <div className="p-10 text-center text-blue-400 font-bold animate-pulse tracking-widest">HÄMTAR SCHEMA...</div>;
    if (error) return <div className="p-6 bg-red-900/20 border border-red-800 rounded-2xl text-center text-red-400 font-bold">{error}</div>;

    return (
        <div className="space-y-8">
            {/* --- NY SEKTION: INKOMMANDE FÖRFRÅGNINGAR --- */}
            {pendingRequests.length > 0 && (
                <div className="space-y-4">
                    <h2 className="text-xl font-black text-white tracking-tight flex items-center gap-3">
                        <span className="w-3 h-3 bg-yellow-400 rounded-full animate-pulse shadow-[0_0_15px_#eab308]"></span>
                        Inkommande förfrågningar
                    </h2>
                    <div className="space-y-4">
                        {pendingRequests.map(req => (
                            <div key={req.id} className="bg-slate-800/50 border-2 border-slate-700 rounded-2xl p-4 space-y-4 relative overflow-hidden">
                                <div className="absolute top-0 left-0 bottom-0 w-1 bg-yellow-400"></div>
                                <p className="text-sm font-bold text-slate-300">
                                    <span className="font-black text-white">{req.requestingUser.firstName} {req.requestingUser.lastName}</span> vill byta pass med dig:
                                </p>
                                
                                <div className="bg-slate-900/70 p-4 rounded-xl space-y-2 border border-slate-700/50">
                                     <div className="flex items-center gap-3">
                                        <span className="text-green-400 text-xs font-black uppercase">Du får</span>
                                        <p className="flex-1 text-white font-bold">{formatShiftTime(req.shift)} <span className="text-slate-400 font-normal">({formatDate(req.shift.startTime)})</span></p>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        <span className="text-red-400 text-xs font-black uppercase">Du ger</span>
                                        <p className="flex-1 text-white font-bold">{formatShiftTime(req.targetShift)} <span className="text-slate-400 font-normal">({formatDate(req.targetShift.startTime)})</span></p>
                                    </div>
                                </div>

                                <div className="grid grid-cols-2 gap-3">
                                    <button 
                                        onClick={() => handleAccept(req.id)}
                                        disabled={actionLoading === req.id}
                                        className="w-full py-2 bg-green-500/10 border border-green-500/30 text-green-400 hover:bg-green-500 hover:text-white text-xs font-black rounded-lg transition-all active:scale-95 disabled:opacity-50">
                                        {actionLoading === req.id ? 'ACCEPTERAR...' : '✅ GODKÄNN'}
                                    </button>
                                    <button 
                                        onClick={() => handleDecline(req.id)}
                                        disabled={actionLoading === req.id}
                                        className="w-full py-2 bg-red-500/10 border border-red-500/30 text-red-400 hover:bg-red-500 hover:text-white text-xs font-black rounded-lg transition-all active:scale-95 disabled:opacity-50">
                                        {actionLoading === req.id ? 'NEKAR...' : '❌ NEKA'}
                                        </button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}
            
            {/* --- MINA PASS --- */}
            <div>
                 <h2 className="text-xl font-black text-white tracking-tight mb-4">Mina Pass</h2>
                 {shifts.length === 0 ? (
                    <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                        <p className="text-4xl mb-4">💤</p>
                        <p className="text-slate-400 font-medium">Du har inga inbokade pass just nu.</p>
                    </div>
                ) : (
                    <div className="space-y-4">
                    {shifts.map((shift) => (
                        <div key={shift.id} className="bg-slate-900/60 backdrop-blur-md p-5 rounded-2xl border border-slate-800 flex flex-col relative overflow-hidden transition-all hover:bg-slate-800/80 hover:border-slate-700">
                           <div className={`absolute left-0 top-0 bottom-0 w-1.5 ${shift.isUpForSwap ? 'bg-yellow-500 shadow-[0_0_15px_#eab308]' : 'bg-blue-500 shadow-[0_0_15px_#3b82f6]'}`}></div>
                            <div className="pl-4">
                                <h3 className="text-xl font-black text-white leading-tight tracking-tight">
                                    {formatShiftTime(shift)}
                                </h3>
                                <span className="text-xs font-bold text-slate-400 uppercase tracking-widest">
                                    {formatDate(shift.startTime)}
                                </span>
                                {shift.isUpForSwap && (
                                    <div className="mt-2">
                                        <span className="text-xs font-bold text-yellow-400 bg-yellow-500/10 px-3 py-1 rounded-lg border border-yellow-500/20 flex items-center gap-1.5 w-fit shadow-[0_0_10px_rgba(234,179,8,0.1)]">
                                            🔄 Ute för byte
                                        </span>
                                    </div>
                                )}
                            </div>

                            {/* Knappar */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mt-5 pl-4">
                                {!shift.isUpForSwap ? (
                                    <>
                                        <button
                                            onClick={() => handleInitiateSwap(shift.id)}
                                            disabled={actionLoading === shift.id}
                                            className="w-full py-2.5 bg-green-500/10 border border-green-500/30 text-green-400 hover:bg-green-500 hover:text-white hover:border-green-400 hover:shadow-[0_0_20px_rgba(74,222,128,0.3)] text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] uppercase tracking-widest disabled:opacity-50">
                                            {actionLoading === shift.id ? 'Publicerar...' : '📤 Lägg ut'}
                                        </button>
                                        <button
                                            onClick={() => handleOpenModal(shift)}
                                            disabled={actionLoading === shift.id}
                                            className="w-full py-2.5 bg-violet-500/10 border border-violet-500/30 text-violet-400 hover:bg-violet-500 hover:text-white hover:border-violet-400 hover:shadow-[0_0_20px_rgba(139,92,246,0.4)] text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] uppercase tracking-widest disabled:opacity-50">
                                            Föreslå byte
                                        </button>
                                    </>
                                ) : (
                                    <button
                                        onClick={() => handleCancelSwap(shift.id)}
                                        disabled={actionLoading === shift.id}
                                        className="w-full py-2.5 bg-yellow-500/10 border border-yellow-500/30 text-yellow-400 hover:bg-yellow-500 hover:text-white hover:border-yellow-400 hover:shadow-[0_0_20px_rgba(234,179,8,0.3)] text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] uppercase tracking-widest disabled:opacity-50 md:col-span-2">
                                        {actionLoading === shift.id ? 'Ångrar...' : '↩️ Ångra'}
                                    </button>
                                )}
                            </div>
                        </div>
                    ))}
                    </div>
                )}
            </div>

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
                                                {actionLoading === availShift.id ? <span className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin"></span> : <span className="text-xs font-bold text-violet-400 opacity-0 group-hover:opacity-100 transition-opacity">VÄLJ</span>}
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