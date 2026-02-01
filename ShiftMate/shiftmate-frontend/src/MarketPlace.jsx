import { useState, useEffect } from 'react';
import axios from 'axios';

const MarketPlace = () => {
    // State för att hålla listan över lediga pass
    const [availableShifts, setAvailableShifts] = useState([]);
    // State för att visa ett laddningsmeddelande medan data hämtas
    const [loading, setLoading] = useState(true);

    // useEffect körs när komponenten laddas första gången
    useEffect(() => {
        const fetchAvailableShifts = async () => {
            try {
                // Hämta JWT-token från webbläsarens lokala lagring
                const token = localStorage.getItem('token');
                // Gör ett API-anrop för att hämta alla pass
                const response = await axios.get('https://localhost:7215/api/Shifts', {
                    headers: { Authorization: `Bearer ${token}` }
                });
                // Filtrera listan för att bara visa pass som är markerade som "lediga"
                setAvailableShifts(response.data.filter(shift => shift.isUpForSwap));
            } catch (err) {
                console.error("Kunde inte hämta lediga pass:", err);
            } finally {
                // Dölj laddningsmeddelandet när hämtningen är klar (oavsett om det lyckades eller ej)
                setLoading(false);
            }
        };
        fetchAvailableShifts();
    }, []); // Den tomma arrayen [] betyder att effekten bara körs en gång

    // Funktion som anropas när en användare klickar på "Ta passet"-knappen
    const handleTakeShift = async (shiftId) => {
        try {
            const token = localStorage.getItem('token');
            const url = `https://localhost:7215/api/Shifts/${shiftId}/take`;

            // Skicka en PUT-förfrågan för att meddela servern att passet ska tas
            await axios.put(url, {}, { // Ingen data (body) behövs, bara ID i URL:en
                headers: { Authorization: `Bearer ${token}` }
            });

            // Visa en bekräftelse och uppdatera gränssnittet
            alert("Passet är nu ditt! Snyggt jobbat! 🤝");
            // Ta bort det tagna passet från listan i state för att UI:t ska uppdateras direkt
            setAvailableShifts(prev => prev.filter(s => s.id !== shiftId));
        } catch (err) {
            const errorMessage = err.response?.data?.message || "Okänt fel";
            console.error("Fel vid tagande av pass:", errorMessage);
            alert(`Kunde inte ta passet: ${errorMessage}`);
        }
    };

    // Hjälpfunktioner för att formatera datum och tid snyggt
    const formatDate = (shift) => {
        const dateStr = shift.startTime;
        if (!dateStr) return "OKÄNT DATUM";
        return new Date(dateStr).toLocaleDateString('sv-SE', { weekday: 'short', day: 'numeric', month: 'short' }).toUpperCase();
    };

    const formatTime = (shift) => {
        const startStr = shift.startTime;
        const endStr = shift.endTime;
        if (!startStr || !endStr) return "--:--";
        const start = new Date(startStr).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
        const end = new Date(endStr).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
        return `${start} - ${end}`;
    };

    // Visa laddningsmeddelande om datan fortfarande hämtas
    if (loading) return <div className="p-10 text-center text-green-400 font-bold animate-pulse tracking-widest">HÄMTAR MARKNADEN...</div>;

    return (
        <div className="space-y-6">
            {/* Om det inte finns några lediga pass, visa ett meddelande */}
            {availableShifts.length === 0 ? (
                <div className="bg-slate-900/50 p-12 rounded-3xl text-center border-2 border-dashed border-slate-800">
                    <p className="text-4xl mb-4">🌴</p>
                    <p className="text-slate-400 font-medium">Inga lediga pass just nu.</p>
                    <p className="text-slate-600 text-sm mt-2">Njut av ledigheten!</p>
                </div>
            ) : (
                // Annars, mappa över och visa varje ledigt pass
                availableShifts.map((shift) => (
                    <div key={shift.id} className="bg-slate-900/80 backdrop-blur-xl p-6 rounded-3xl border border-slate-800 flex flex-col relative overflow-hidden transition-all hover:bg-slate-800 hover:scale-[1.01] hover:shadow-[0_0_30px_rgba(74,222,128,0.1)] group">

                        {/* Neon-kant för stil */}
                        <div className="absolute left-0 top-0 bottom-0 w-1.5 bg-green-400 shadow-[0_0_20px_#4ade80]"></div>

                        <div className="flex flex-col items-center text-center mb-6">
                            <span className="text-[10px] font-black text-green-300 bg-green-500/10 px-4 py-1.5 rounded-full uppercase tracking-widest mb-4 border border-green-400/30 shadow-[0_0_10px_rgba(74,222,128,0.2)]">
                                LEDIGT PASS
                            </span>

                            <h3 className="text-3xl font-black text-white tracking-tight mb-1">
                                {formatTime(shift)}
                            </h3>

                            <p className="text-sm font-bold text-slate-500 uppercase tracking-widest">
                                {formatDate(shift)}
                            </p>
                        </div>

                        {/* Knapp för att ta passet */}
                        <button
                            onClick={() => handleTakeShift(shift.id)}
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