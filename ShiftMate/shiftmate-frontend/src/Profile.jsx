import { useState, useEffect } from 'react';
import axios from 'axios';

const Profile = ({ onLogout }) => {
    const [stats, setStats] = useState({ totalShifts: 0, totalHours: 0 });
    const [userData, setUserData] = useState({
        fullName: "",
        initials: "",
        email: ""
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // 1. LÄS NAMN FRÅN TOKEN (Biljetten) 🎫
        const token = localStorage.getItem('token');
        if (token) {
            try {
                // Avkoda token (den är base64-kodad)
                const payload = JSON.parse(atob(token.split('.')[1]));

                const firstName = payload.FirstName || "";
                const lastName = payload.LastName || "";
                const email = payload.email || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || "Användare";

                // Räkna ut namn och initialer
                let fullName = email.split('@')[0]; // Default om namn saknas
                let initials = fullName.substring(0, 2).toUpperCase();

                if (firstName && lastName) {
                    fullName = `${firstName} ${lastName}`;
                    initials = (firstName[0] + lastName[0]).toUpperCase();
                }

                setUserData({ fullName, initials, email });

            } catch (e) {
                console.error("Kunde inte läsa token", e);
            }
        }

        // 2. Hämta statistik (samma som förut)
        const fetchStats = async () => {
            try {
                const response = await axios.get('https://localhost:7215/api/Shifts/mine', {
                    headers: { Authorization: `Bearer ${token}` }
                });

                const shifts = response.data;
                const hours = shifts.reduce((sum, shift) => sum + shift.durationHours, 0);

                setStats({
                    totalShifts: shifts.length,
                    totalHours: hours
                });
            } catch (err) {
                console.error("Kunde inte hämta statistik");
            } finally {
                setLoading(false);
            }
        };

        fetchStats();
    }, []);

    if (loading) return <div className="p-10 text-center text-pink-400 font-bold animate-pulse tracking-widest">LADDAR PROFIL...</div>;

    return (
        <div className="space-y-8 animate-in fade-in zoom-in-95 duration-500">

            {/* 1. PROFILKORT */}
            <div className="bg-slate-900/60 backdrop-blur-xl p-8 rounded-3xl border border-slate-800 flex flex-col items-center text-center relative overflow-hidden group">
                <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-pink-500 via-purple-500 to-indigo-500"></div>

                {/* Avatar med RÄTTA initialer */}
                <div className="w-24 h-24 rounded-full bg-gradient-to-tr from-pink-600 to-purple-600 mb-4 flex items-center justify-center shadow-[0_0_30px_rgba(236,72,153,0.3)] border-4 border-slate-900">
                    <span className="text-3xl font-black text-white">
                        {userData.initials}
                    </span>
                </div>

                {/* Namnet (med stor bokstav via CSS) */}
                <h2 className="text-2xl font-black text-white tracking-tight capitalize">
                    {userData.fullName}
                </h2>
                <p className="text-sm font-medium text-slate-500 uppercase tracking-widest mt-1">
                    Team ShiftMate
                </p>
            </div>

            {/* 2. STATISTIK-GRID */}
            <div className="grid grid-cols-2 gap-4">
                <div className="bg-slate-900/60 p-6 rounded-3xl border border-slate-800 hover:border-pink-500/30 transition-all group hover:bg-slate-800 relative overflow-hidden">
                    <p className="text-xs font-bold text-pink-400 uppercase tracking-widest mb-1">Totala Pass</p>
                    <p className="text-4xl font-black text-white group-hover:scale-110 transition-transform origin-left">
                        {stats.totalShifts}
                    </p>
                </div>

                <div className="bg-slate-900/60 p-6 rounded-3xl border border-slate-800 hover:border-purple-500/30 transition-all group hover:bg-slate-800 relative overflow-hidden">
                    <p className="text-xs font-bold text-purple-400 uppercase tracking-widest mb-1">Timmar</p>
                    <p className="text-4xl font-black text-white group-hover:scale-110 transition-transform origin-left">
                        {stats.totalHours}
                    </p>
                </div>
            </div>

            {/* 3. LOGGA UT KNAPP */}
            <button
                onClick={onLogout}
                className="w-full py-4 bg-red-500/10 border border-red-500/30 text-red-400 hover:bg-red-600 hover:text-white hover:border-red-500 hover:shadow-[0_0_30px_rgba(239,68,68,0.4)] font-black rounded-2xl transition-all duration-300 uppercase tracking-widest"
            >
                Logga ut
            </button>

            <p className="text-center text-xs text-slate-600 font-mono">
                ShiftMate v1.0 • Built with ⚛️ & ☕
            </p>
        </div>
    );
};

export default Profile;