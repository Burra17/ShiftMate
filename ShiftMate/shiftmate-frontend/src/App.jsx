// importera nödvändiga funktioner och komponenter från react och react-router-dom
import { useState, useEffect } from 'react';
import { Routes, Route, Navigate, useNavigate, useLocation, Link } from 'react-router-dom';
import { setLogoutCallback, getUserRole } from './api'; // <--- NYTT: Vi importerar getUserRole

// importera sid-komponenter
import Login from './Login';
import Register from './Register';
import ShiftList from './ShiftList';
import MarketPlace from './MarketPlace';
import Schedule from './Schedule';
import Profile from './Profile';
import AdminPanel from './components/AdminPanel'; // <--- NYTT: Vi importerar AdminPanel

// Huvudkomponent för applikationen när en användare är inloggad.
const MainApp = ({ onLogout }) => {
    const location = useLocation();

    // NYTT: Kolla om användaren är Admin
    const role = getUserRole();
    const isAdmin = role === 'Admin';

    // Bestämmer den aktiva fliken baserat på URL
    const [activeTab, setActiveTab] = useState(location.pathname.substring(1) || 'mine');

    // Definition av bas-elementen i menyn (för alla)
    const baseNavItems = [
        { id: 'mine', label: 'Mina Pass', path: '/mine', component: <ShiftList />, icon: Icons.Home },
        { id: 'market', label: 'Lediga Pass', path: '/market', component: <MarketPlace />, icon: Icons.Swap },
        { id: 'schedule', label: 'Schema', path: '/schedule', component: <Schedule />, icon: Icons.Calendar },
        { id: 'profile', label: 'Profil', path: '/profile', component: <Profile onLogout={onLogout} />, icon: Icons.User },
    ];

    // NYTT: Om man är admin, lägg till admin-panelen i listan!
    const navItems = isAdmin
        ? [...baseNavItems, { id: 'admin', label: 'Admin', path: '/admin', component: <AdminPanel />, icon: Icons.Shield }]
        : baseNavItems;

    // Hittar den komponent som motsvarar den aktiva fliken.
    const ActiveComponent = navItems.find(item => item.id === activeTab)?.component || <ShiftList />;

    return (
        <div className="min-h-screen bg-slate-950 text-gray-100 font-sans flex overflow-hidden">
            {/* Sidomeny */}
            <aside className="hidden md:flex flex-col w-72 bg-slate-900/50 backdrop-blur-xl border-r border-slate-800 p-6">
                <div className="flex items-center gap-3 mb-10 px-2">
                    <div className="w-8 h-8 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg flex items-center justify-center shadow-lg shadow-indigo-500/20">
                        <span className="text-sm">⛽</span>
                    </div>
                    <h1 className="text-xl font-black tracking-tight text-white">ShiftMate</h1>
                </div>

                {/* Navigationslänkar */}
                <nav className="flex-1 space-y-2">
                    {navItems.map((item) => (
                        <Link to={item.path} key={item.id} onClick={() => setActiveTab(item.id)}
                            className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-200 group text-sm font-bold ${activeTab === item.id
                                ? 'bg-blue-600/10 text-blue-400 border border-blue-500/20 shadow-[0_0_15px_rgba(59,130,246,0.1)]'
                                : 'text-slate-400 hover:bg-slate-800 hover:text-white'
                                }`}
                        >
                            <item.icon />
                            <span>{item.label}</span>
                            {activeTab === item.id && <div className="ml-auto w-1.5 h-1.5 rounded-full bg-blue-400 shadow-[0_0_8px_currentColor]"></div>}
                        </Link>
                    ))}
                </nav>

                <button onClick={onLogout} className="flex items-center gap-3 px-4 py-3 text-slate-500 hover:text-red-400 transition-colors mt-auto text-sm font-bold">
                    <Icons.LogOut />
                    <span>Logga ut</span>
                </button>
            </aside>

            {/* Huvudinnehåll */}
            <main className="flex-1 overflow-y-auto relative h-screen">
                <div className="p-6 md:p-12 max-w-5xl mx-auto pb-28 md:pb-12">
                    <header className="mb-8">
                        <h2 className="text-3xl md:text-4xl font-black text-white tracking-tight animate-in fade-in slide-in-from-bottom-2 duration-500">
                            {navItems.find(n => n.id === activeTab)?.label || "Välkommen"}
                        </h2>
                        <p className="text-slate-400 mt-2 text-sm">Hantera din tid på macken smidigt.</p>
                    </header>

                    <div className="animate-in fade-in zoom-in-95 duration-300">
                        {ActiveComponent}
                    </div>
                </div>
            </main>

            {/* Mobil bottenmeny — visas bara på små skärmar */}
            <nav className="md:hidden fixed bottom-0 left-0 right-0 bg-slate-900/95 backdrop-blur-xl border-t border-slate-800 z-50">
                <div className="flex justify-around items-center h-16 px-1">
                    {navItems.map((item) => (
                        <Link
                            to={item.path}
                            key={item.id}
                            onClick={() => setActiveTab(item.id)}
                            className={`flex flex-col items-center justify-center flex-1 py-1 transition-all duration-200
                                ${activeTab === item.id
                                    ? 'text-blue-400'
                                    : 'text-slate-500'
                                }`}
                        >
                            <item.icon />
                            <span className="text-[10px] font-bold mt-1 tracking-tight">{item.label}</span>
                            {activeTab === item.id && (
                                <div className="w-1 h-1 rounded-full bg-blue-400 shadow-[0_0_6px_#3b82f6] mt-0.5"></div>
                            )}
                        </Link>
                    ))}
                </div>
            </nav>
        </div>
    );
};


// SVG-ikoner
const Icons = {
    Home: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" /><polyline points="9 22 9 12 15 12 15 22" /></svg>,
    Swap: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M8 3 4 7l4 4" /><path d="M4 7h16" /><path d="m16 21 4-4-4-4" /><path d="M20 17H4" /></svg>,
    Calendar: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="18" height="18" x="3" y="4" rx="2" ry="2" /><line x1="16" x2="16" y1="2" y2="6" /><line x1="8" x2="8" y1="2" y2="6" /><line x1="3" x2="21" y1="10" y2="10" /></svg>,
    User: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" /></svg>,
    LogOut: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" /><polyline points="16 17 21 12 16 7" /><line x1="21" x2="9" y1="12" y2="12" /></svg>,
    // NY IKON: En sköld för Admin (måste finnas för att inte krascha)
    Shield: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /></svg>
};


function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));
    const navigate = useNavigate();

    const handleLoginSuccess = () => {
        setIsLoggedIn(true);
        navigate('/');
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsLoggedIn(false);
        navigate('/login');
    };

    useEffect(() => {
        setLogoutCallback(handleLogout);
    }, [handleLogout]);

    return (
        <Routes>
            {isLoggedIn ? (
                <Route path="/*" element={<MainApp onLogout={handleLogout} />} />
            ) : (
                <>
                    <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="*" element={<Navigate to="/login" />} />
                </>
            )}
        </Routes>
    );
}

export default App;