import { useState } from 'react'
import Login from './Login'
import ShiftList from './ShiftList' // <--- Importera den nya filen

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));

    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsLoggedIn(false);
    };

    if (!isLoggedIn) {
        return <Login onLoginSuccess={() => setIsLoggedIn(true)} />;
    }

    return (
        <div className="min-h-screen bg-gray-50 pb-10">
            {/* Header / App Bar */}
            <header className="bg-white px-6 py-4 shadow-sm sticky top-0 z-10 flex justify-between items-center">
                <h1 className="text-2xl font-black text-gray-900 tracking-tight">ShiftMate</h1>
                <button
                    onClick={handleLogout}
                    className="text-sm font-bold text-gray-500 hover:text-red-600 transition-colors"
                >
                    Logga ut
                </button>
            </header>

            {/* Huvudinnehåll */}
            <main className="max-w-md mx-auto p-6">
                <div className="mb-6">
                    <p className="text-gray-500">Välkommen tillbaka, André! 👋</p>
                </div>

                {/* Här laddar vi in listan! */}
                <ShiftList />
            </main>
        </div>
    );
}

export default App;