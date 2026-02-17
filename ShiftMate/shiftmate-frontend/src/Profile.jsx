import { useState, useEffect } from 'react';
import { decodeToken, fetchMyShifts, updateProfile, changePassword, getUserRole } from './api';
import { useToast } from './contexts/ToastContext';
import LoadingSpinner from './components/LoadingSpinner';

// Översätt rollnamn till svenska för visning
const roleLabels = {
    Admin: "Admin",
    Manager: "Chef",
    Employee: "Anställd"
};

const Profile = ({ onLogout }) => {
    const toast = useToast();
    const [stats, setStats] = useState({
        monthShifts: 0, monthHours: 0,
        totalShifts: 0, totalHours: 0,
        avgMonthlyHours: 0
    });
    const [userData, setUserData] = useState({
        firstName: "",
        lastName: "",
        fullName: "",
        initials: "",
        email: "",
        role: ""
    });
    const [loading, setLoading] = useState(true);

    // Redigeringsläge
    const [isEditing, setIsEditing] = useState(false);
    const [form, setForm] = useState({ firstName: "", lastName: "", email: "" });
    const [saving, setSaving] = useState(false);

    // Lösenordsbyte
    const [isChangingPassword, setIsChangingPassword] = useState(false);
    const [passwordForm, setPasswordForm] = useState({
        currentPassword: "", newPassword: "", confirmPassword: ""
    });
    const [savingPassword, setSavingPassword] = useState(false);

    useEffect(() => {
        // 1. LÄS NAMN OCH ROLL FRÅN TOKEN
        const payload = decodeToken();
        const role = getUserRole() || "";
        if (payload) {
            try {
                const firstName = payload.FirstName || "";
                const lastName = payload.LastName || "";
                const email = payload.email || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || "Användare";

                // Räkna ut namn och initialer
                let fullName = email.split('@')[0];
                let initials = fullName.substring(0, 2).toUpperCase();

                if (firstName && lastName) {
                    fullName = `${firstName} ${lastName}`;
                    initials = (firstName[0] + lastName[0]).toUpperCase();
                }

                setUserData({ firstName, lastName, fullName, initials, email, role });

            } catch (e) {
                console.error("Kunde inte läsa token", e);
            }
        }

        // 2. Hämta statistik
        const fetchStats = async () => {
            try {
                const shifts = await fetchMyShifts();

                // Beräkna totaler
                const totalHours = shifts.reduce((sum, shift) => sum + shift.durationHours, 0);

                // Beräkna månadsstatistik — filtrera på nuvarande månad/år
                const now = new Date();
                const currentMonth = now.getMonth();
                const currentYear = now.getFullYear();

                const monthShifts = shifts.filter(shift => {
                    const start = new Date(shift.startTime);
                    return start.getMonth() === currentMonth && start.getFullYear() === currentYear;
                });

                const monthHours = monthShifts.reduce((sum, shift) => sum + shift.durationHours, 0);

                // Beräkna genomsnittliga timmar per månad för progress-indikatorn
                let avgMonthlyHours = 0;
                if (shifts.length > 0) {
                    const months = new Set(shifts.map(s => {
                        const d = new Date(s.startTime);
                        return `${d.getFullYear()}-${d.getMonth()}`;
                    }));
                    avgMonthlyHours = totalHours / months.size;
                }

                setStats({
                    monthShifts: monthShifts.length,
                    monthHours: Math.round(monthHours * 10) / 10,
                    totalShifts: shifts.length,
                    totalHours: Math.round(totalHours * 10) / 10,
                    avgMonthlyHours: Math.round(avgMonthlyHours * 10) / 10
                });
            } catch (err) {
                console.error("Kunde inte hämta statistik");
            } finally {
                setLoading(false);
            }
        };

        fetchStats();
    }, []);

    // Beräkna procent för progress-baren (nuvarande månad vs genomsnitt)
    const monthProgress = stats.avgMonthlyHours > 0
        ? Math.min(Math.round((stats.monthHours / stats.avgMonthlyHours) * 100), 100)
        : 0;

    // Öppna redigeringsläge och fyll formuläret med aktuella värden
    const handleEdit = () => {
        setForm({
            firstName: userData.firstName,
            lastName: userData.lastName,
            email: userData.email
        });
        setIsEditing(true);
    };

    // Spara ändringar via API
    const handleSave = async () => {
        setSaving(true);
        try {
            await updateProfile({
                firstName: form.firstName,
                lastName: form.lastName,
                email: form.email
            });

            // Uppdatera lokal state med nya värden
            const fullName = form.firstName && form.lastName
                ? `${form.firstName} ${form.lastName}`
                : form.email.split('@')[0];
            const initials = form.firstName && form.lastName
                ? (form.firstName[0] + form.lastName[0]).toUpperCase()
                : fullName.substring(0, 2).toUpperCase();

            setUserData(prev => ({
                ...prev,
                firstName: form.firstName,
                lastName: form.lastName,
                fullName,
                initials,
                email: form.email
            }));

            toast.success("Profilen har uppdaterats!");
            setIsEditing(false);
        } catch (err) {
            const message = err.response?.data || "Kunde inte uppdatera profilen.";
            toast.error(typeof message === 'string' ? message : "Kunde inte uppdatera profilen.");
        } finally {
            setSaving(false);
        }
    };

    // Hantera lösenordsbyte
    const handleChangePassword = async () => {
        // Frontend-validering: kontrollera att nya lösenord matchar
        if (passwordForm.newPassword !== passwordForm.confirmPassword) {
            toast.error("Nya lösenorden matchar inte.");
            return;
        }
        if (passwordForm.newPassword.length < 8) {
            toast.error("Lösenordet måste vara minst 8 tecken.");
            return;
        }

        setSavingPassword(true);
        try {
            await changePassword({
                currentPassword: passwordForm.currentPassword,
                newPassword: passwordForm.newPassword
            });
            toast.success("Lösenordet har ändrats!");
            setIsChangingPassword(false);
            setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
        } catch (err) {
            const message = err.response?.data || "Kunde inte byta lösenord.";
            toast.error(typeof message === 'string' ? message : "Kunde inte byta lösenord.");
        } finally {
            setSavingPassword(false);
        }
    };

    // Rollbadge — färg baserat på roll
    const roleBadge = () => {
        const label = roleLabels[userData.role] || userData.role;
        const styles = {
            Admin: "text-red-400 bg-red-500/10 border-red-500/30",
            Manager: "text-amber-400 bg-amber-500/10 border-amber-500/30",
            Employee: "text-blue-400 bg-blue-500/10 border-blue-500/30"
        };
        const style = styles[userData.role] || "text-slate-400 bg-slate-500/10 border-slate-500/30";
        return (
            <span className={`text-[10px] font-black px-3 py-1 rounded-full uppercase tracking-widest border ${style}`}>
                {label}
            </span>
        );
    };

    if (loading) return <LoadingSpinner message="Laddar profil..." />;

    return (
        <div className="space-y-8">

            {/* 1. PROFILKORT */}
            <div className="bg-slate-900/60 backdrop-blur-xl p-8 rounded-3xl border border-slate-800 flex flex-col items-center text-center relative overflow-hidden group">
                <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-pink-500 via-purple-500 to-indigo-500"></div>

                {/* Avatar med initialer */}
                <div className="w-24 h-24 rounded-full bg-gradient-to-tr from-pink-600 to-purple-600 mb-4 flex items-center justify-center shadow-[0_0_30px_rgba(236,72,153,0.3)] border-4 border-slate-900">
                    <span className="text-3xl font-black text-white">
                        {userData.initials}
                    </span>
                </div>

                {isEditing ? (
                    /* Redigeringsformulär */
                    <div className="w-full max-w-sm space-y-4 mt-2">
                        <div className="flex space-x-4">
                            <div className="space-y-2 w-1/2">
                                <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Förnamn</label>
                                <input
                                    type="text"
                                    required
                                    className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                                    value={form.firstName}
                                    onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2 w-1/2">
                                <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Efternamn</label>
                                <input
                                    type="text"
                                    required
                                    className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                                    value={form.lastName}
                                    onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                                />
                            </div>
                        </div>
                        <div className="space-y-2">
                            <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">E-post</label>
                            <input
                                type="email"
                                required
                                className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                                value={form.email}
                                onChange={(e) => setForm({ ...form, email: e.target.value })}
                            />
                        </div>
                        <div className="flex space-x-3 pt-2">
                            <button
                                onClick={handleSave}
                                disabled={saving}
                                className="flex-1 py-3 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-500 hover:to-purple-500 text-white font-bold rounded-xl transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 uppercase tracking-widest text-xs"
                            >
                                {saving ? 'Sparar...' : 'Spara'}
                            </button>
                            <button
                                onClick={() => setIsEditing(false)}
                                disabled={saving}
                                className="flex-1 py-3 bg-slate-800 hover:bg-slate-700 text-slate-300 font-bold rounded-xl transition-all uppercase tracking-widest text-xs"
                            >
                                Avbryt
                            </button>
                        </div>
                    </div>
                ) : (
                    /* Visningsläge */
                    <>
                        <h2 className="text-2xl font-black text-white tracking-tight capitalize">
                            {userData.fullName}
                        </h2>
                        <p className="text-sm font-medium text-slate-500 mt-1 mb-3">
                            {userData.email}
                        </p>
                        {/* Rollbadge */}
                        {userData.role && roleBadge()}
                    </>
                )}
            </div>

            {/* 2. STATISTIK-SEKTION */}
            <div className="space-y-4">
                <h3 className="text-xl font-black text-white tracking-tight uppercase">Statistik</h3>

                {/* 2x2 Grid */}
                <div className="grid grid-cols-2 gap-4">
                    {/* Månadens pass */}
                    <div className="bg-pink-500/5 p-6 rounded-3xl border border-slate-800 hover:border-pink-500/30 transition-all group hover:bg-slate-800/80 relative overflow-hidden">
                        <div className="absolute left-0 top-0 bottom-0 w-1 bg-pink-500 shadow-[0_0_15px_#ec4899]"></div>
                        <p className="text-xs font-bold text-pink-400 uppercase tracking-widest mb-1">Pass denna månad</p>
                        <p className="text-4xl font-black text-white group-hover:scale-110 transition-transform origin-left">
                            {stats.monthShifts}
                        </p>
                    </div>

                    {/* Månadens timmar + progress */}
                    <div className="bg-purple-500/5 p-6 rounded-3xl border border-slate-800 hover:border-purple-500/30 transition-all group hover:bg-slate-800/80 relative overflow-hidden">
                        <div className="absolute left-0 top-0 bottom-0 w-1 bg-purple-500 shadow-[0_0_15px_#a855f7]"></div>
                        <p className="text-xs font-bold text-purple-400 uppercase tracking-widest mb-1">Timmar denna månad</p>
                        <p className="text-4xl font-black text-white group-hover:scale-110 transition-transform origin-left">
                            {stats.monthHours}
                        </p>
                        {/* Progress-bar: nuvarande månad vs genomsnitt */}
                        {stats.avgMonthlyHours > 0 && (
                            <div className="mt-3 space-y-1">
                                <div className="w-full h-1.5 bg-slate-800 rounded-full overflow-hidden">
                                    <div
                                        className="h-full bg-gradient-to-r from-purple-500 to-pink-500 rounded-full transition-all duration-1000"
                                        style={{ width: `${monthProgress}%` }}
                                    />
                                </div>
                                <p className="text-[10px] text-slate-500 font-medium">
                                    {monthProgress}% av snitt ({stats.avgMonthlyHours}h/mån)
                                </p>
                            </div>
                        )}
                    </div>

                    {/* Totala pass */}
                    <div className="bg-blue-500/5 p-6 rounded-3xl border border-slate-800 hover:border-blue-500/30 transition-all group hover:bg-slate-800/80 relative overflow-hidden">
                        <div className="absolute left-0 top-0 bottom-0 w-1 bg-blue-500 shadow-[0_0_15px_#3b82f6]"></div>
                        <p className="text-xs font-bold text-blue-400 uppercase tracking-widest mb-1">Totala pass</p>
                        <p className="text-4xl font-black text-white group-hover:scale-110 transition-transform origin-left">
                            {stats.totalShifts}
                        </p>
                    </div>

                    {/* Totala timmar */}
                    <div className="bg-indigo-500/5 p-6 rounded-3xl border border-slate-800 hover:border-indigo-500/30 transition-all group hover:bg-slate-800/80 relative overflow-hidden">
                        <div className="absolute left-0 top-0 bottom-0 w-1 bg-indigo-500 shadow-[0_0_15px_#6366f1]"></div>
                        <p className="text-xs font-bold text-indigo-400 uppercase tracking-widest mb-1">Totala timmar</p>
                        <p className="text-4xl font-black text-white group-hover:scale-110 transition-transform origin-left">
                            {stats.totalHours}
                        </p>
                    </div>
                </div>
            </div>

            {/* 3. REDIGERA PROFIL-KNAPP */}
            {!isEditing && !isChangingPassword && (
                <button
                    onClick={handleEdit}
                    className="w-full py-4 bg-purple-500/10 border border-purple-500/30 text-purple-400 hover:bg-purple-600 hover:text-white hover:border-purple-500 hover:shadow-[0_0_30px_rgba(168,85,247,0.4)] font-black rounded-2xl transition-all duration-300 uppercase tracking-widest"
                >
                    ✏️ Redigera profil
                </button>
            )}

            {/* 4. BYT LÖSENORD */}
            {isChangingPassword ? (
                <div className="bg-slate-900/60 backdrop-blur-xl p-6 rounded-3xl border border-slate-800 space-y-4 relative overflow-hidden">
                    <div className="absolute left-0 top-0 bottom-0 w-1 bg-blue-500 shadow-[0_0_15px_#3b82f6]"></div>
                    <h3 className="text-sm font-black text-white uppercase tracking-widest">🔒 Byt lösenord</h3>
                    <div className="space-y-2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Nuvarande lösenord</label>
                        <input
                            type="password"
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Ange nuvarande lösenord"
                            value={passwordForm.currentPassword}
                            onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Nytt lösenord</label>
                        <input
                            type="password"
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Minst 8 tecken"
                            value={passwordForm.newPassword}
                            onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Bekräfta nytt lösenord</label>
                        <input
                            type="password"
                            className="w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium"
                            placeholder="Upprepa nytt lösenord"
                            value={passwordForm.confirmPassword}
                            onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                        />
                    </div>
                    <div className="flex space-x-3 pt-2">
                        <button
                            onClick={handleChangePassword}
                            disabled={savingPassword}
                            className="flex-1 py-3 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-500 hover:to-purple-500 text-white font-bold rounded-xl transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 uppercase tracking-widest text-xs"
                        >
                            {savingPassword ? 'Sparar...' : '✅ Spara lösenord'}
                        </button>
                        <button
                            onClick={() => {
                                setIsChangingPassword(false);
                                setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
                            }}
                            disabled={savingPassword}
                            className="flex-1 py-3 bg-slate-800 hover:bg-slate-700 text-slate-300 font-bold rounded-xl transition-all uppercase tracking-widest text-xs"
                        >
                            ❌ Avbryt
                        </button>
                    </div>
                </div>
            ) : !isEditing && (
                <button
                    onClick={() => setIsChangingPassword(true)}
                    className="w-full py-4 bg-blue-500/10 border border-blue-500/30 text-blue-400 hover:bg-blue-600 hover:text-white hover:border-blue-500 hover:shadow-[0_0_30px_rgba(59,130,246,0.4)] font-black rounded-2xl transition-all duration-300 uppercase tracking-widest"
                >
                    🔒 Byt lösenord
                </button>
            )}

            {/* 5. LOGGA UT KNAPP */}
            <button
                onClick={onLogout}
                className="w-full py-4 bg-red-500/10 border border-red-500/30 text-red-400 hover:bg-red-600 hover:text-white hover:border-red-500 hover:shadow-[0_0_30px_rgba(239,68,68,0.4)] font-black rounded-2xl transition-all duration-300 uppercase tracking-widest"
            >
                🚪 Logga ut
            </button>

            <p className="text-center text-xs text-slate-600 font-mono">
                ShiftMate v1.0 • Built with ⚛️ & ☕
            </p>
        </div>
    );
};

export default Profile;
