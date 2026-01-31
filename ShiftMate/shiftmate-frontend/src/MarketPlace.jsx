import { useState, useEffect } from 'react';
import axios from 'axios';

const MarketPlace = () => {
    // --- BEHÅLLEN LOGIK (Rör ej) ---
    const [availableSwaps, setAvailableSwaps] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchAvailable = async () => {
            try {
                const token = localStorage.getItem('token');
                const response = await axios.get('https://localhost:7215/api/SwapRequests/available', {
                    headers: { Authorization: `Bearer ${token}` }
                });
                setAvailableSwaps(response.data);
            } catch (err) {
                console.error("Kunde inte hämta lediga pass:", err);
            } finally {
                setLoading(false);
            }
        };
        fetchAvailable();
    }, []);

    const handleAcceptSwap = async (swapId) => {
        try {
            const token = localStorage.getItem('token');
            const url = 'https://localhost:7215/api/SwapRequests/accept';
            const body = { swapRequestId: swapId };

            await axios.post(url, body, {
                headers: { Authorization: `Bearer ${token}` }
            });

            alert("Passet är nu ditt! Snyggt jobbat! 🤝");
            setAvailableSwaps(prev => prev.filter(s => s.id !== swapId));
        } catch (err) {
            const errorMessage = err.response?.data || "Okänt fel";
            console.error("Fel vid accept:", errorMessage);
            alert(`Kunde inte ta passet: ${errorMessage}`);
        }
    };

    const formatDate = (swap) => {
        const dateStr = swap.shift?.startTime;
        if (!dateStr) return "OKÄNT DATUM";
        return new Date(dateStr).toLocaleDateString('sv-SE', { weekday: 'short', day: 'numeric', month: 'short' }).toUpperCase();
    };

    const formatTime = (swap) => {
        const startStr = swap.shift?.startTime;
        const endStr = swap.shift?.endTime;
        if (!startStr || !endStr) return "--:--";
        const start = new Date(startStr).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
        const end = new Date(endStr).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
        return `${start} - ${end}`;
    };

    if (loading) return <div className="p-10 text-center text-green-400 font-bold animate-pulse tracking-widest">HÄMTAR MARKNADEN...</div>;

    return (
        <div className="space-y-6">

            {availableSwaps.length === 0 ? (
                <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                    <p className="text-4xl mb-4">🌴</p>
                    <p className="text-slate-400 font-medium">Inga lediga pass just nu.</p>
                    <p className="text-slate-600 text-sm mt-2">Njut av ledigheten!</p>
                </div>
            ) : (
                availableSwaps.map((swap) => (
                    <div key={swap.id} className="bg-slate-900/80 backdrop-blur-xl p-6 rounded-3xl border border-slate-800 flex flex-col relative overflow-hidden transition-all hover:bg-slate-800 hover:scale-[1.01] hover:shadow-[0_0_30px_rgba(74,222,128,0.1)] group">

                        {/* Neon-kant */}
                        <div className="absolute left-0 top-0 bottom-0 w-1.5 bg-green-400 shadow-[0_0_20px_#4ade80]"></div>

                        <div className="flex flex-col items-center text-center mb-6">
                            <span className="text-[10px] font-black text-green-300 bg-green-500/10 px-4 py-1.5 rounded-full uppercase tracking-widest mb-4 border border-green-400/30 shadow-[0_0_10px_rgba(74,222,128,0.2)]">
                                {swap.shift?.durationHours} TIMMARS PASS
                            </span>

                            <h3 className="text-3xl font-black text-white tracking-tight mb-1">
                                {formatTime(swap)}
                            </h3>

                            <p className="text-sm font-bold text-slate-500 uppercase tracking-widest">
                                {formatDate(swap)}
                            </p>

                            {swap.requestingUser?.email && (
                                <div className="mt-4 flex items-center gap-2 text-xs text-slate-400 bg-slate-950/80 px-3 py-1.5 rounded-lg border border-slate-700/50">
                                    <span>👤</span>
                                    <span className="italic">Från: {swap.requestingUser.email.split('@')[0]}</span>
                                </div>
                            )}
                        </div>

                        {/* KNAPP MED FILL-EFFECT */}
                        <button
                            onClick={() => handleAcceptSwap(swap.id)}
                            className="w-full py-3 
                            bg-green-500/10 border border-green-500/30 text-green-400 
                            hover:bg-green-500 hover:text-white hover:border-green-400 hover:shadow-[0_0_30px_rgba(74,222,128,0.4)]
                            text-xs font-black rounded-xl transition-all duration-300 active:scale-[0.98] 
                            uppercase tracking-widest flex justify-center items-center gap-2 
                            shadow-[0_0_15px_rgba(74,222,128,0.1)]"
                        >
                            <span>🚀</span> TA PASSET
                        </button>
                    </div>
                ))
            )}
        </div>
    );
};

export default MarketPlace;