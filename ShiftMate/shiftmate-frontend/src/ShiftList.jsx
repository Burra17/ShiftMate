import { useState, useEffect } from 'react';
import axios from 'axios';

const ShiftList = () => {
    const [shifts, setShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [actionLoading, setActionLoading] = useState(null); // För att visa laddning på specifik knapp

    useEffect(() => {
        const fetchShifts = async () => {
            try {
                const token = localStorage.getItem('token');
                const response = await axios.get('https://localhost:7215/api/Shifts/mine', {
                    headers: { Authorization: `Bearer ${token}` }
                });
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
            const token = localStorage.getItem('token');
            // Anrop till din SwapRequest-endpoint
            await axios.post('https://localhost:7215/api/SwapRequests/initiate',
                { shiftId: shiftId },
                { headers: { Authorization: `Bearer ${token}` } }
            );

            alert("Passet ligger nu ute för byte! 🎉");

            // Uppdatera listan lokalt så gränssnittet reagerar direkt
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

    const formatDate = (dateString) => {
        if (!dateString) return "";
        const options = { weekday: 'short', day: 'numeric', month: 'short' };
        return new Date(dateString).toLocaleDateString('sv-SE', options).toUpperCase();
    };

    const formatTime = (dateString) => {
        if (!dateString) return "";
        return new Date(dateString).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
    };

    if (loading) return <div className="p-10 text-center text-gray-500 font-medium">Hämtar dina pass... ⏳</div>;
    if (error) return <div className="p-10 text-center text-red-500 font-bold">{error}</div>;

    return (
        <div className="space-y-4">
            <h2 className="text-xl font-bold text-gray-800 mb-4 px-1">Mina Arbetspass</h2>

            {shifts.length === 0 ? (
                <div className="bg-white p-8 rounded-2xl shadow-sm text-center text-gray-500 border border-dashed border-gray-200">
                    Inga inbokade pass hittades.
                </div>
            ) : (
                shifts.map((shift) => (
                    <div key={shift.id} className="bg-white p-5 rounded-2xl shadow-md border border-gray-100 flex flex-col relative overflow-hidden transition-all hover:shadow-lg">
                        {/* Dynamisk dekorationskant baserat på status */}
                        <div className={`absolute left-0 top-0 bottom-0 w-2 ${shift.isUpForSwap ? 'bg-yellow-400' : 'bg-orange-500'}`}></div>

                        <div className="flex justify-between items-start pl-4">
                            <div className="flex-1">
                                <p className="text-xs font-bold text-gray-400 mb-1 tracking-wider">
                                    {formatDate(shift.startTime)}
                                </p>
                                <h3 className="text-xl font-black text-gray-900 leading-tight">
                                    {formatTime(shift.startTime)} — {formatTime(shift.endTime)}
                                </h3>

                                <div className="flex flex-wrap items-center gap-2 mt-3">
                                    <span className="text-xs font-bold text-gray-600 bg-gray-100 px-2.5 py-1 rounded-lg">
                                        {shift.durationHours} timmar
                                    </span>
                                    {shift.isUpForSwap && (
                                        <span className="text-xs font-bold text-yellow-700 bg-yellow-100 px-2.5 py-1 rounded-lg flex items-center gap-1">
                                            Ligger ute till byte 🔄
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Knapp som bara visas om passet inte redan är ute för byte */}
                        {!shift.isUpForSwap && (
                            <div className="mt-5 pl-4">
                                <button
                                    onClick={() => handleInitiateSwap(shift.id)}
                                    disabled={actionLoading === shift.id}
                                    className="w-full py-3 bg-orange-50 hover:bg-orange-100 text-orange-600 text-sm font-extrabold rounded-xl border border-orange-200 transition-all active:scale-[0.98] disabled:opacity-50"
                                >
                                    {actionLoading === shift.id ? 'Publicerar...' : 'LÄGG UT PASS'}
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