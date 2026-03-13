import { useState, useEffect, useRef } from 'react';
import { decodeToken, fetchMyShifts, updateProfile, changePassword, getUserRole, getOrganizationName } from './api';
import { useToast } from './contexts/ToastContext';
import LoadingSpinner from './components/LoadingSpinner';

// Översätt rollnamn till svenska för visning
const roleLabels = {
    Manager: "Chef",
    Employee: "Anställd"
};

const Profile = ({ onLogout }) => {
    const toast = useToast();
    const profileCardRef = useRef(null);
    const [stats, setStats] = useState({ totalShifts: 0, totalHours: 0 });
    const [userData, setUserData] = useState({
        firstName: "",
        lastName: "",
        fullName: "",
        initials: "",
        email: "",
        role: "",
        orgName: ""
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
        const orgName = getOrganizationName() || "";
        if (payload) {
            try {
                const firstName = payload.FirstName || "";
                const lastName = payload.LastName || "";
                const email = payload.email || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || "Användare";

                let fullName = email.split('@')[0];
                let initials = fullName.substring(0, 2).toUpperCase();

                if (firstName && lastName) {
                    fullName = `${firstName} ${lastName}`;
                    initials = (firstName[0] + lastName[0]).toUpperCase();
                }

                setUserData({ firstName, lastName, fullName, initials, email, role, orgName });

            } catch (e) {
                console.error("Kunde inte läsa token", e);
            }
        }

        // 2. Hämta statistik
        const fetchStats = async () => {
            try {
                const shiftsData = await fetchMyShifts();
                const totalHours = shiftsData.reduce((sum, s) => sum + s.durationHours, 0);

                setStats({
                    totalShifts: shiftsData.length,
                    totalHours: Math.round(totalHours * 10) / 10
                });
            } catch (err) {
                console.error("Kunde inte hämta statistik");
            } finally {
                setLoading(false);
            }
        };

        fetchStats();
    }, []);

    // Öppna redigeringsläge
    const handleEdit = () => {
        setForm({
            firstName: userData.firstName,
            lastName: userData.lastName,
            email: userData.email
        });
        setIsEditing(true);
        setIsChangingPassword(false);
        setTimeout(() => {
            profileCardRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 50);
    };

    // Öppna lösenordsbyte
    const handlePasswordClick = () => {
        setIsChangingPassword(true);
        setIsEditing(false);
        setTimeout(() => {
            profileCardRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 50);
    };

    // Spara profiländringar via API
    const handleSave = async () => {
        setSaving(true);
        try {
            await updateProfile({
                firstName: form.firstName,
                lastName: form.lastName,
                email: form.email
            });

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
            const data = err.response?.data;
            const message = data?.message || (typeof data === 'string' ? data : "Kunde inte uppdatera profilen.");
            toast.error(message);
        } finally {
            setSaving(false);
        }
    };

    // Hantera lösenordsbyte
    const handleChangePassword = async () => {
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
            const data = err.response?.data;
            const message = data?.message || (typeof data === 'string' ? data : "Kunde inte byta lösenord.");
            toast.error(message);
        } finally {
            setSavingPassword(false);
        }
    };

    const cancelEdit = () => {
        setIsEditing(false);
        setIsChangingPassword(false);
        setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
    };

    // Rollbadge — färg baserat på roll
    const roleBadge = () => {
        const label = roleLabels[userData.role] || userData.role;
        const styles = {
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

    // Gemensam input-klass
    const inputClass = "w-full px-5 py-4 bg-slate-950/50 border border-slate-800 rounded-xl text-white placeholder-slate-600 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all font-medium";

    if (loading) return <LoadingSpinner message="Laddar profil..." />;

    // Avgör om vi visar ett formulär (redigera eller lösenord)
    const showForm = isEditing || isChangingPassword;

    return (
        <div className="space-y-6">

            {/* ── PROFILKORT ── */}
            <div ref={profileCardRef} className="animate-fade-up bg-slate-900/60 backdrop-blur-xl rounded-3xl border border-slate-800 relative overflow-hidden">
                {/* Gradient mesh bakgrund */}
                <div className="absolute inset-0 opacity-40">
                    <div className="absolute -top-24 -right-24 w-64 h-64 rounded-full bg-blue-600/20 blur-3xl" />
                    <div className="absolute -bottom-16 -left-16 w-48 h-48 rounded-full bg-purple-600/15 blur-3xl" />
                    <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-32 h-32 rounded-full bg-pink-600/10 blur-3xl" />
                </div>

                <div className="relative p-8 flex flex-col items-center text-center">
                    {/* Avatar med glödande ring */}
                    <div className="relative mb-5">
                        <div className="absolute -inset-1.5 rounded-full bg-gradient-to-tr from-blue-500 via-purple-500 to-pink-500 opacity-60 blur-sm animate-pulse" />
                        <div className="relative w-24 h-24 rounded-full bg-gradient-to-tr from-blue-600 via-purple-600 to-pink-600 flex items-center justify-center border-4 border-slate-900 shadow-2xl">
                            <span className="text-3xl font-black text-white">{userData.initials}</span>
                        </div>
                    </div>

                    {/* Namn, e-post, org, roll — alltid synligt */}
                    <h2 className="text-2xl font-black text-white tracking-tight capitalize">
                        {userData.fullName}
                    </h2>
                    <p className="text-sm font-medium text-slate-500 mt-1">
                        {userData.email}
                    </p>
                    {userData.orgName && (
                        <p className="text-xs font-medium text-slate-600 mt-1">{userData.orgName}</p>
                    )}
                    <div className="flex items-center gap-3 mt-3">
                        {userData.role && roleBadge()}
                    </div>

                    {/* Kompakt statistik-rad */}
                    {!showForm && (
                        <div className="flex items-center gap-5 mt-5 pt-5 border-t border-slate-800/60">
                            <div className="text-center">
                                <p className="text-xl font-black text-white">{stats.totalShifts}</p>
                                <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Pass</p>
                            </div>
                            <div className="w-px h-8 bg-slate-800" />
                            <div className="text-center">
                                <p className="text-xl font-black text-white">{stats.totalHours}</p>
                                <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Timmar</p>
                            </div>
                        </div>
                    )}

                    {/* ── Formulär (redigera profil ELLER byt lösenord) ── */}
                    {isEditing && (
                        <div className="w-full max-w-sm space-y-4 mt-6 pt-6 border-t border-slate-800/60">
                            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-widest">Redigera profil</h3>
                            <div className="flex space-x-4">
                                <div className="space-y-2 w-1/2">
                                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Förnamn</label>
                                    <input type="text" required className={inputClass}
                                        value={form.firstName}
                                        onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                                    />
                                </div>
                                <div className="space-y-2 w-1/2">
                                    <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Efternamn</label>
                                    <input type="text" required className={inputClass}
                                        value={form.lastName}
                                        onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                                    />
                                </div>
                            </div>
                            <div className="space-y-2">
                                <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">E-post</label>
                                <input type="email" required className={inputClass}
                                    value={form.email}
                                    onChange={(e) => setForm({ ...form, email: e.target.value })}
                                />
                            </div>
                            <div className="flex space-x-3 pt-2">
                                <button onClick={handleSave} disabled={saving}
                                    className="flex-1 py-3 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-500 hover:to-purple-500 text-white font-bold rounded-xl transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 uppercase tracking-widest text-xs">
                                    {saving ? 'Sparar...' : 'Spara'}
                                </button>
                                <button onClick={cancelEdit} disabled={saving}
                                    className="flex-1 py-3 bg-slate-800 hover:bg-slate-700 text-slate-300 font-bold rounded-xl transition-all uppercase tracking-widest text-xs">
                                    Avbryt
                                </button>
                            </div>
                        </div>
                    )}

                    {isChangingPassword && (
                        <div className="w-full max-w-sm space-y-4 mt-6 pt-6 border-t border-slate-800/60">
                            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-widest">Byt lösenord</h3>
                            <div className="space-y-2">
                                <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Nuvarande lösenord</label>
                                <input type="password" className={inputClass} placeholder="Ange nuvarande lösenord"
                                    value={passwordForm.currentPassword}
                                    onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Nytt lösenord</label>
                                <input type="password" className={inputClass} placeholder="Minst 8 tecken"
                                    value={passwordForm.newPassword}
                                    onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-[10px] font-bold text-slate-500 uppercase tracking-widest ml-1">Bekräfta nytt lösenord</label>
                                <input type="password" className={inputClass} placeholder="Upprepa nytt lösenord"
                                    value={passwordForm.confirmPassword}
                                    onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                                />
                            </div>
                            <div className="flex space-x-3 pt-2">
                                <button onClick={handleChangePassword} disabled={savingPassword}
                                    className="flex-1 py-3 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-500 hover:to-purple-500 text-white font-bold rounded-xl transition-all hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 uppercase tracking-widest text-xs">
                                    {savingPassword ? 'Sparar...' : 'Spara lösenord'}
                                </button>
                                <button onClick={cancelEdit} disabled={savingPassword}
                                    className="flex-1 py-3 bg-slate-800 hover:bg-slate-700 text-slate-300 font-bold rounded-xl transition-all uppercase tracking-widest text-xs">
                                    Avbryt
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* ── INSTÄLLNINGAR — Settings-lista ── */}
            {!showForm && (
                <div className="animate-fade-up" style={{ animationDelay: '0.1s' }}>
                    <h3 className="text-sm font-bold text-slate-400 uppercase tracking-widest mb-3">Inställningar</h3>
                    <div className="bg-slate-900/60 backdrop-blur-xl rounded-2xl border border-slate-800 overflow-hidden divide-y divide-slate-800/80">
                        {/* Redigera profil */}
                        <button onClick={handleEdit}
                            className="w-full flex items-center justify-between px-5 py-4 hover:bg-slate-800/60 transition-colors group">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 rounded-lg bg-purple-500/10 flex items-center justify-center">
                                    <svg className="w-4 h-4 text-purple-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                                    </svg>
                                </div>
                                <span className="text-sm font-semibold text-slate-300 group-hover:text-white transition-colors">Redigera profil</span>
                            </div>
                            <svg className="w-4 h-4 text-slate-600 group-hover:text-slate-400 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
                            </svg>
                        </button>

                        {/* Byt lösenord */}
                        <button onClick={handlePasswordClick}
                            className="w-full flex items-center justify-between px-5 py-4 hover:bg-slate-800/60 transition-colors group">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 rounded-lg bg-blue-500/10 flex items-center justify-center">
                                    <svg className="w-4 h-4 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z" />
                                    </svg>
                                </div>
                                <span className="text-sm font-semibold text-slate-300 group-hover:text-white transition-colors">Byt lösenord</span>
                            </div>
                            <svg className="w-4 h-4 text-slate-600 group-hover:text-slate-400 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
                            </svg>
                        </button>

                        {/* Logga ut */}
                        <button onClick={onLogout}
                            className="w-full flex items-center justify-between px-5 py-4 hover:bg-red-500/5 transition-colors group">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 rounded-lg bg-red-500/10 flex items-center justify-center">
                                    <svg className="w-4 h-4 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
                                    </svg>
                                </div>
                                <span className="text-sm font-semibold text-red-400/80 group-hover:text-red-400 transition-colors">Logga ut</span>
                            </div>
                        </button>
                    </div>
                </div>
            )}

            <p className="text-center text-[10px] text-slate-700 font-medium tracking-wide">
                ShiftMate v1.0
            </p>
        </div>
    );
};

export default Profile;
