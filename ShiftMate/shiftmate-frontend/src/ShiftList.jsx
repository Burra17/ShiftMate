import { useState, useEffect } from 'react';
import { fetchShifts as fetchShiftsApi, fetchMyShifts, fetchReceivedSwapRequests, acceptSwapRequest, declineSwapRequest, initiateSwap, cancelShiftSwap, proposeDirectSwap } from './api';
import { formatDate, formatTime } from './utils/dateUtils';
import { useToast } from './contexts/ToastContext';

const ShiftList = () => {
    // --- HOOKS ---
    const toast = useToast();

    // --- STATES ---
    const [shifts, setShifts] = useState([]); // Användarens egna pass
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [actionLoading, setActionLoading] = useState(null);

    // State för modalen (Direktbyte)
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modalLoading, setModalLoading] = useState(false);
    const [modalError, setModalError] = useState(null);
    const [availableShifts, setAvailableShifts] = useState([]);
    const [selectedShift, setSelectedShift] = useState(null);

    // State för inkommande förfrågningar
    const [pendingRequests, setPendingRequests] = useState([]);
    const [requestsLoading, setRequestsLoading] = useState(true);

    // --- DATAHÄMTNING ---

    // Hämtar pass som tillhör den inloggade användaren
    const fetchShifts = async () => {
        try {
            const data = await fetchMyShifts();
            setShifts(data);
        } catch (err) {
            console.error("Kunde inte hämta pass:", err);
            setError("Kunde inte ladda dina pass just nu.");
        } finally {
            setLoading(false);
        }
    };

    // Hämtar bytesförfrågningar som skickats TILL användaren
    const fetchReceivedRequests = async () => {
        try {
            setRequestsLoading(true);
            const data = await fetchReceivedSwapRequests();
            setPendingRequests(data);
        } catch (err) {
            console.error("Kunde inte hämta inkommande förfrågningar:", err);
        } finally {
            setRequestsLoading(false);
        }
    };

    // Körs vid sidladdning
    useEffect(() => {
        fetchShifts();
        fetchReceivedRequests();
    }, []);

    // --- HANDLERS FÖR FÖRFRÅGNINGAR ---

    const handleDecline = async (requestId) => {
        setActionLoading(requestId);
        try {
            await declineSwapRequest(requestId);
            toast.success("Förfrågan har nekats.");
            setPendingRequests(prev => prev.filter(r => r.id !== requestId));
        } catch (err) {
            toast.error(err.response?.data?.message || "Något gick fel.");
        } finally {
            setActionLoading(null);
        }
    };

    const handleAccept = async (requestId) => {
        setActionLoading(requestId);
        try {
            await acceptSwapRequest(requestId);
            toast.success("Bytet har accepterats! Ditt schema uppdateras.");
            setPendingRequests(prev => prev.filter(r => r.id !== requestId));
            fetchShifts();
        } catch (err) {
            toast.error(err.response?.data?.message || "Kunde inte acceptera bytet.");
        } finally {
            setActionLoading(null);
        }
    };

    // --- HANDLERS FÖR EGNA PASS ---

    // Lägger ut ett pass på den öppna marknaden (MarketPlace)
    const handleInitiateSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
            await initiateSwap(shiftId);
            toast.success("Passet ligger nu ute för byte!");
            setShifts(prev => prev.map(s => s.id === shiftId ? { ...s, isUpForSwap: true } : s));
        } catch (err) {
            toast.error(err.response?.data?.message || "Gick inte att lägga ut passet.");
        } finally {
            setActionLoading(null);
        }
    };

    // Tar bort passet från marknaden
    const handleCancelSwap = async (shiftId) => {
        setActionLoading(shiftId);
        try {
            await cancelShiftSwap(shiftId);
            setShifts(prev => prev.map(s => s.id === shiftId ? { ...s, isUpForSwap: false } : s));
        } finally {
            setActionLoading(null);
        }
    };

    // --- MODAL-LOGIK (DIREKTBYTE) ---

    const handleOpenModal = async (shiftToSwap) => {
        setSelectedShift(shiftToSwap);
        setIsModalOpen(true);
        setModalLoading(true);
        setModalError(null);

        try {
            // HÄR ANVÄNDS DEN NYA LOGIKEN: 
            // Vi skickar 'true' för att endast hämta pass som har en UserId (ej vakanta).
            const allShifts = await fetchShiftsApi(true);

            const now = new Date();
            // Filtrera bort egna pass och pass som redan ligger på marknaden
            const filteredShifts = allShifts.filter(s =>
                s.userId !== shiftToSwap.userId &&
                new Date(s.endTime) > now &&
                !s.isUpForSwap
            );

            setAvailableShifts(filteredShifts);
        } catch (err) {
            setModalError("Kunde inte ladda tillgängliga pass.");
        } finally {
            setModalLoading(false);
        }
    };

    const handleProposeSwap = async (targetShiftId) => {
        if (!selectedShift) return;
        setActionLoading(targetShiftId);

        try {
            await proposeDirectSwap(selectedShift.id, targetShiftId);
            toast.success("Förslag om direktbyte har skickats!");
            setIsModalOpen(false);
            setAvailableShifts([]);
        } catch (err) {
            toast.error(err.response?.data?.message || "Kunde inte föreslå byte.");
        } finally {
            setActionLoading(null);
        }
    };

    // --- HJÄLPFUNKTIONER FÖR TID/DATUM ---

    const formatShiftTime = (shift) => {
        if (!shift?.startTime || !shift?.endTime) return "Okänd tid";
        return `${formatTime(shift.startTime)} - ${formatTime(shift.endTime)}`;
    };

    // --- RENDERINGS-LOGIK ---

    if (loading || requestsLoading) return (
        <div className="p-10 text-center text-blue-400 font-bold animate-pulse tracking-widest uppercase">
            Hämtar schema...
        </div>
    );

    if (error) return (
        <div className="p-6 bg-red-900/20 border border-red-800 rounded-2xl text-center text-red-400 font-bold">
            {error}
        </div>
    );

    return (
        <div className="space-y-8">
            {/* 1. SEKTION: INKOMMANDE FÖRFRÅGNINGAR */}
            {pendingRequests.length > 0 && (
                <div className="space-y-4">
                    <h2 className="text-xl font-black text-white tracking-tight flex items-center gap-3 uppercase">
                        <span className="w-3 h-3 bg-yellow-400 rounded-full animate-pulse shadow-[0_0_15px_#eab308]"></span>
                        Inkommande förfrågningar
                    </h2>
                    <div className="space-y-4">
                        {pendingRequests.map(req => (
                            <div key={req.id} className="bg-slate-800/50 border-2 border-slate-700 rounded-2xl p-4 space-y-4 relative overflow-hidden">
                                <div className="absolute top-0 left-0 bottom-0 w-1 bg-yellow-400"></div>
                                <p className="text-sm font-bold text-slate-300">
                                    <span className="font-black text-white">{req.requestingUser?.firstName} {req.requestingUser?.lastName}</span> vill byta pass:
                                </p>

                                <div className="bg-slate-900/70 p-4 rounded-xl space-y-2 border border-slate-700/50">
                                    <div className="flex items-center gap-3">
                                        <span className="text-green-400 text-[10px] font-black uppercase w-12">Du får</span>
                                        <p className="flex-1 text-white font-bold text-sm">
                                            {formatShiftTime(req.shift)} <span className="text-slate-400 font-normal text-xs">({formatDate(req.shift.startTime)})</span>
                                        </p>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        <span className="text-red-400 text-[10px] font-black uppercase w-12">Du ger</span>
                                        <p className="flex-1 text-white font-bold text-sm">
                                            {formatShiftTime(req.targetShift)} <span className="text-slate-400 font-normal text-xs">({formatDate(req.targetShift.startTime)})</span>
                                        </p>
                                    </div>
                                </div>

                                <div className="grid grid-cols-2 gap-3">
                                    <button
                                        onClick={() => handleAccept(req.id)}
                                        disabled={actionLoading === req.id}
                                        className="w-full py-2 bg-green-500/10 border border-green-500/30 text-green-400 hover:bg-green-500 hover:text-white text-xs font-black rounded-lg transition-all disabled:opacity-50"
                                    >
                                        {actionLoading === req.id ? 'VÄNTAR...' : '✅ GODKÄNN'}
                                    </button>
                                    <button
                                        onClick={() => handleDecline(req.id)}
                                        disabled={actionLoading === req.id}
                                        className="w-full py-2 bg-red-500/10 border border-red-500/30 text-red-400 hover:bg-red-500 hover:text-white text-xs font-black rounded-lg transition-all disabled:opacity-50"
                                    >
                                        {actionLoading === req.id ? 'VÄNTAR...' : '❌ NEKA'}
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* 2. SEKTION: MINA PASS */}
            <div>
                <h2 className="text-xl font-black text-white tracking-tight mb-4 uppercase">Mina Pass</h2>
                {shifts.length === 0 ? (
                    <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                        <p className="text-4xl mb-4">💤</p>
                        <p className="text-slate-400 font-medium">Du har inga inbokade pass just nu.</p>
                    </div>
                ) : (
                    <div className="space-y-4">
                        {shifts.map((shift) => (
                            <div key={shift.id} className="bg-slate-900/60 backdrop-blur-md p-5 rounded-2xl border border-slate-800 flex flex-col relative overflow-hidden transition-all hover:bg-slate-800/80">
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
                                            <span className="text-[10px] font-black text-yellow-400 bg-yellow-500/10 px-3 py-1 rounded-lg border border-yellow-500/20 uppercase tracking-tighter">
                                                🔄 Ute för byte 
                                            </span>
                                        </div>
                                    )}
                                </div>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mt-5 pl-4">
                                    {!shift.isUpForSwap ? (
                                        <>
                                            <button
                                                onClick={() => handleInitiateSwap(shift.id)}
                                                disabled={actionLoading === shift.id}
                                                className="w-full py-2.5 bg-green-500/10 border border-green-500/30 text-green-400 hover:bg-green-500 hover:text-white text-[10px] font-black rounded-xl transition-all uppercase tracking-widest disabled:opacity-50"
                                            >
                                                {actionLoading === shift.id ? 'VÄNTAR...' : '📤 Lägg ut passet'}
                                            </button>
                                            <button
                                                onClick={() => handleOpenModal(shift)}
                                                disabled={actionLoading === shift.id}
                                                className="w-full py-2.5 bg-violet-500/10 border border-violet-500/30 text-violet-400 hover:bg-violet-500 hover:text-white text-[10px] font-black rounded-xl transition-all uppercase tracking-widest disabled:opacity-50"
                                            >
                                                🔄 Föreslå direktbyte
                                            </button>
                                        </>
                                    ) : (
                                        <button
                                            onClick={() => handleCancelSwap(shift.id)}
                                            disabled={actionLoading === shift.id}
                                            className="w-full py-2.5 bg-yellow-500/10 border border-yellow-500/30 text-yellow-400 hover:bg-yellow-500 hover:text-white text-[10px] font-black rounded-xl transition-all uppercase tracking-widest disabled:opacity-50 md:col-span-2"
                                        >
                                            {actionLoading === shift.id ? 'VÄNTAR...' : '↩️ Ångra'}
                                        </button>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* 3. MODAL FÖR DIREKTBYTE */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
                    <div className="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-lg max-h-[80vh] flex flex-col shadow-2xl">
                        <div className="p-4 border-b border-slate-800 flex justify-between items-center bg-slate-800/30">
                            <h2 className="text-white font-black text-xs uppercase tracking-widest">Välj ett pass att byta med</h2>
                            <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-white text-xl">✕</button>
                        </div>
                        <div className="p-4 overflow-y-auto">
                            {modalLoading && <p className="text-slate-400 text-center animate-pulse py-10 uppercase text-xs font-bold">Laddar kollegor...</p>}
                            {modalError && <p className="text-red-400 text-center py-10 font-bold">{modalError}</p>}
                            {!modalLoading && !modalError && (
                                <div className="space-y-2">
                                    {availableShifts.length > 0 ? (
                                        availableShifts.map(availShift => (
                                            <button
                                                key={availShift.id}
                                                onClick={() => handleProposeSwap(availShift.id)}
                                                disabled={actionLoading === availShift.id}
                                                className="w-full text-left p-4 bg-slate-800/50 rounded-xl border border-slate-700 hover:border-violet-500 transition-all disabled:opacity-50 flex justify-between items-center group"
                                            >
                                                <div>
                                                    <p className="font-black text-white text-sm uppercase">
                                                        {availShift.user?.firstName} {availShift.user?.lastName}
                                                    </p>
                                                    <p className="text-[10px] text-slate-400 font-bold tracking-tight">
                                                        {formatDate(availShift.startTime)} | {formatTime(availShift.startTime)} - {formatTime(availShift.endTime)}
                                                    </p>
                                                </div>
                                                <span className="text-[10px] font-black text-violet-400 opacity-0 group-hover:opacity-100 transition-opacity uppercase">Välj</span>
                                            </button>
                                        ))
                                    ) : (
                                        <div className="text-center py-10">
                                            <p className="text-slate-500 font-bold text-sm">Hittade inga kollegor tillgängliga för byte just nu.</p>
                                        </div>
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