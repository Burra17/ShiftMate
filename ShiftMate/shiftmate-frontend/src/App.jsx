import { useState } from 'react'
import Login from './Login'
import ShiftList from './ShiftList'
import MarketPlace from './MarketPlace'

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));
    const [activeTab, setActiveTab] = useState('mine'); // 'mine' eller 'market'

    if (!isLoggedIn) return <Login onLoginSuccess={() => setIsLoggedIn(true)} />;

    return (
        <div className="min-h-screen bg-gray-50 pb-20">
            <header className="bg-white px-6 py-4 shadow-sm sticky top-0 z-10 flex justify-between items-center">
                <h1 className="text-2xl font-black text-gray-900">ShiftMate</h1>
                <button onClick={() => { localStorage.removeItem('token'); setIsLoggedIn(false) }} className="text-xs font-bold text-gray-400">LOGGA UT</button>
            </header>

            <main className="max-w-md mx-auto p-6">
                {/* Enkla flikar */}
                <div className="flex gap-2 mb-8 bg-gray-100 p-1 rounded-xl">
                    <button
                        onClick={() => setActiveTab('mine')}
                        className={`flex-1 py-2 text-sm font-bold rounded-lg transition-all ${activeTab === 'mine' ? 'bg-white text-orange-600 shadow-sm' : 'text-gray-500'}`}
                    >
                        Mina Pass
                    </button>
                    <button
                        onClick={() => setActiveTab('market')}
                        className={`flex-1 py-2 text-sm font-bold rounded-lg transition-all ${activeTab === 'market' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'}`}
                    >
                        Lediga Pass
                    </button>
                </div>

                {activeTab === 'mine' ? <ShiftList /> : <MarketPlace />}
            </main>
        </div>
    );
}

export default App;