import { useState, useEffect } from 'react';
import Login from './Login';
import ShiftList from './ShiftList';
import MarketPlace from './MarketPlace';
import Schedule from './Schedule';

// Enkla ikoner (så vi slipper installera bibliotek just nu)
const Icons = {
    Home: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" /><polyline points="9 22 9 12 15 12 15 22" /></svg>,
    Swap: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M8 3 4 7l4 4" /><path d="M4 7h16" /><path d="m16 21 4-4-4-4" /><path d="M20 17H4" /></svg>,
    Calendar: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="18" height="18" x="3" y="4" rx="2" ry="2" /><line x1="16" x2="16" y1="2" y2="6" /><line x1="8" x2="8" y1="2" y2="6" /><line x1="3" x2="21" y1="10" y2="10" /></svg>,
    User: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" /></svg>,
    LogOut: () => <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" /><polyline points="16 17 21 12 16 7" /><line x1="21" x2="9" y1="12" y2="12" /></svg>
};

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));
    const [activeTab, setActiveTab] = useState('mine'); // 'mine', 'market', 'schedule', 'profile'

    // Hantera utloggning med full reload för att rensa cache
    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsLoggedIn(false);
        window.location.reload();
    };

    if (!isLoggedIn) return <Login onLoginSuccess={() => setIsLoggedIn(true)} />;

    // Menyvalen
    const navItems = [
        { id: 'mine', label: 'Mina Pass', icon: Icons.Home },
        { id: 'market', label: 'Lediga Pass', icon: Icons.Swap },
        { id: 'schedule', label: 'Schema', icon: Icons.Calendar }, // Placeholder för framtiden
        { id: 'profile', label: 'Profil', icon: Icons.User },      // Placeholder för framtiden
    ];

    return (
        <div className="min-h-screen bg-slate-950 text-gray-100 font-sans flex overflow-hidden">

            {/* --- DESKTOP SIDEBAR (Gömd på mobil) --- */}
            <aside className="hidden md:flex flex-col w-72 bg-slate-900/50 backdrop-blur-xl border-r border-slate-800 p-6">
                <div className="flex items-center gap-3 mb-10 px-2">
                    <div className="w-8 h-8 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg flex items-center justify-center shadow-lg shadow-indigo-500/20">
                        <span className="text-sm">⛽</span>
                    </div>
                    <h1 className="text-xl font-black tracking-tight text-white">ShiftMate</h1>
                </div>

                <nav className="flex-1 space-y-2">
                    {navItems.map((item) => (
                        <button
                            key={item.id}
                            onClick={() => setActiveTab(item.id)}
                            className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-200 group text-sm font-bold ${activeTab === item.id
                                    ? 'bg-blue-600/10 text-blue-400 border border-blue-500/20 shadow-[0_0_15px_rgba(59,130,246,0.1)]'
                                    : 'text-slate-400 hover:bg-slate-800 hover:text-white'
                                }`}
                        >
                            <item.icon />
                            <span>{item.label}</span>
                            {activeTab === item.id && <div className="ml-auto w-1.5 h-1.5 rounded-full bg-blue-400 shadow-[0_0_8px_currentColor]"></div>}
                        </button>
                    ))}
                </nav>

                <button onClick={handleLogout} className="flex items-center gap-3 px-4 py-3 text-slate-500 hover:text-red-400 transition-colors mt-auto text-sm font-bold">
                    <Icons.LogOut />
                    <span>Logga ut</span>
                </button>
            </aside>

            {/* --- MAIN CONTENT AREA --- */}
            <main className="flex-1 overflow-y-auto relative h-screen">

                {/* Mobil Header */}
                <div className="md:hidden sticky top-0 z-20 bg-slate-900/80 backdrop-blur-md border-b border-slate-800 px-6 py-4 flex justify-between items-center">
                    <div className="flex items-center gap-2">
                        <span className="text-xl">⛽</span>
                        <h1 className="text-lg font-black tracking-tight text-white">ShiftMate</h1>
                    </div>
                    <button onClick={handleLogout} className="text-slate-400 hover:text-white">
                        <Icons.LogOut />
                    </button>
                </div>

                <div className="p-6 md:p-12 max-w-5xl mx-auto pb-32 md:pb-12">
                    {/* Dynamisk Rubrik */}
                    <header className="mb-8">
                        <h2 className="text-3xl md:text-4xl font-black text-white tracking-tight animate-in fade-in slide-in-from-bottom-2 duration-500">
                            {navItems.find(n => n.id === activeTab)?.label}
                        </h2>
                        <p className="text-slate-400 mt-2 text-sm">Hantera din tid på macken smidigt.</p>
                    </header>

                    {/* Innehåll */}
                    <div className="animate-in fade-in zoom-in-95 duration-300">
                        {activeTab === 'mine' && <ShiftList />}
                        {activeTab === 'market' && <MarketPlace />}

                        {/* HÄR ÄR DEN NYA VYN: */}
                        {activeTab === 'schedule' && <Schedule />}

                        {/* Profil är fortfarande placeholder */}
                        {activeTab === 'profile' && (
                            <div className="p-12 border border-dashed border-slate-800 rounded-3xl text-center bg-slate-900/30">
                                <p className="text-4xl mb-4">🚧</p>
                                <h3 className="text-xl font-bold text-white mb-2">Profil kommer snart</h3>
                            </div>
                        )}
                    </div>
                </div>
            </main>

            {/* --- MOBIL BOTTOM NAV --- */}
            <div className="md:hidden fixed bottom-0 left-0 right-0 bg-slate-950/90 backdrop-blur-xl border-t border-slate-800 px-6 py-2 z-50 pb-6">
                <div className="flex justify-between items-center">
                    {navItems.map((item) => (
                        <button
                            key={item.id}
                            onClick={() => setActiveTab(item.id)}
                            className={`flex flex-col items-center gap-1 p-2 rounded-2xl transition-all duration-300 ${activeTab === item.id ? 'text-blue-400 scale-105' : 'text-slate-500'
                                }`}
                        >
                            <div className={`p-1.5 rounded-xl transition-all ${activeTab === item.id ? 'bg-blue-500/10' : ''}`}>
                                <item.icon />
                            </div>
                            <span className="text-[10px] font-bold uppercase tracking-widest">{item.label}</span>
                        </button>
                    ))}
                </div>
            </div>

        </div>
    );
}

export default App;