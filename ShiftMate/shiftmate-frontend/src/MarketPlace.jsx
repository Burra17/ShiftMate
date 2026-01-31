import { useState, useEffect } from 'react';
import axios from 'axios';

const MarketPlace = () => {
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

            // Här matchar vi exakt vad Swagger visar
            const url = 'https://localhost:7215/api/SwapRequests/accept';
            const body = { swapRequestId: swapId }; // Swagger-namnet på fältet

            await axios.post(url, body, {
                headers: { Authorization: `Bearer ${token}` }
            });

            alert("Passet är nu ditt! Snyggt jobbat! 🤝");

            // Ta bort från listan direkt
            setAvailableSwaps(prev => prev.filter(s => s.id !== swapId));
        } catch (err) {
            const errorMessage = err.response?.data || "Okänt fel";
            console.error("Fel vid accept:", errorMessage);

            // Nu när vi har rätt URL kommer vi se det riktiga felet om det nekas
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

    if (loading) return <div className="p-10 text-center text-gray-500 font-medium italic">Letar efter lediga pass... 🔍</div>;

    return (
        <div className="space-y-6">
            <h2 className="text-2xl font-black text-gray-900 mb-6 text-center tracking-tight">Lediga pass</h2>

            {availableSwaps.length === 0 ? (
                <div className="bg-white p-12 rounded-3xl shadow-sm text-center border-2 border-dashed border-gray-100">
                    <p className="text-gray-400 font-medium">Det finns inga lediga pass att ta just nu. ✨</p>
                </div>
            ) : (
                availableSwaps.map((swap) => (
                    <div key={swap.id} className="bg-white p-6 rounded-3xl shadow-xl shadow-gray-100/50 border border-gray-50 flex flex-col relative overflow-hidden transition-all hover:scale-[1.02] active:scale-[0.98]">
                        <div className="absolute left-0 top-0 bottom-0 w-2 bg-blue-500"></div>

                        <div className="flex flex-col items-center text-center">
                            <span className="text-[10px] font-black text-blue-600 bg-blue-50 px-3 py-1 rounded-full uppercase tracking-widest mb-3">
                                {swap.shift?.durationHours} TIMMARS PASS
                            </span>

                            <h3 className="text-3xl font-black text-gray-900 tracking-tighter">
                                {formatTime(swap)}
                            </h3>

                            <p className="text-sm font-bold text-gray-400 mt-1 uppercase">
                                {formatDate(swap)}
                            </p>

                            {/* Visa vem som vill byta om namnet finns med i datan */}
                            {swap.requestingUser?.email && (
                                <p className="text-xs text-gray-400 mt-4 italic">
                                    Upplagt av: {swap.requestingUser.email.split('@')[0]}
                                </p>
                            )}
                        </div>

                        <button
                            onClick={() => handleAcceptSwap(swap.id)}
                            className="mt-6 py-4 bg-blue-600 hover:bg-blue-700 text-white text-md font-black rounded-2xl shadow-lg shadow-blue-200 transition-all uppercase tracking-wider"
                        >
                            TA PASSET
                        </button>
                    </div>
                ))
            )}
        </div>
    );
};

export default MarketPlace;