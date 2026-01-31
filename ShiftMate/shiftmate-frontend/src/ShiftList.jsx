import { useState, useEffect } from 'react';
import axios from 'axios';

const ShiftList = () => {
    const [shifts, setShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

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

    const formatDate = (dateString) => {
        if (!dateString) return "";
        const options = { weekday: 'short', day: 'numeric', month: 'short' };
        return new Date(dateString).toLocaleDateString('sv-SE', options).toUpperCase();
    };

    const formatTime = (dateString) => {
        if (!dateString) return "";
        return new Date(dateString).toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' });
    };

    if (loading) return <div className="p-10 text-center text-gray-500">Laddar... ⏳</div>;
    if (error) return <div className="p-10 text-center text-red-500 font-bold">{error}</div>;

    return (
        <div className="space-y-4">
            <h2 className="text-xl font-bold text-gray-800 mb-4">Kommande Pass</h2>

            {shifts.length === 0 ? (
                <div className="bg-white p-6 rounded-2xl shadow-sm text-center text-gray-500">
                    Inga pass inbokade.
                </div>
            ) : (
                shifts.map((shift) => (
                    <div key={shift.id} className="bg-white p-5 rounded-2xl shadow-md border border-gray-100 flex justify-between items-center relative overflow-hidden">
                        {/* Dekorations-streck */}
                        <div className={`absolute left-0 top-0 bottom-0 w-2 ${shift.isUpForSwap ? 'bg-yellow-400' : 'bg-orange-500'}`}></div>

                        <div className="pl-4">
                            <p className="text-xs font-bold text-gray-500 mb-1">
                                {formatDate(shift.startTime)} {/* RÄTT NAMN HÄR */}
                            </p>
                            <h3 className="text-xl font-black text-gray-900">
                                {formatTime(shift.startTime)} - {formatTime(shift.endTime)} {/* OCH HÄR */}
                            </h3>
                            <div className="flex items-center gap-2 mt-2">
                                <span className="text-sm text-gray-600 bg-gray-100 px-2 py-1 rounded-md">
                                    {shift.durationHours} tim
                                </span>
                                {shift.isUpForSwap && (
                                    <span className="text-xs font-bold text-yellow-700 bg-yellow-100 px-2 py-1 rounded-md">
                                        Bytes begärt 🔄
                                    </span>
                                )}
                            </div>
                        </div>
                    </div>
                ))
            )}
        </div>
    );
};

export default ShiftList;