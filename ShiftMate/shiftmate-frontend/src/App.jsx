import { useState } from 'react'
import Login from './Login'

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));

    if (!isLoggedIn) {
        return <Login onLoginSuccess={() => setIsLoggedIn(true)} />;
    }

    return (
        <div className="min-h-screen bg-gray-50 p-6 flex flex-col items-center justify-center">
            <div className="bg-white p-10 rounded-3xl shadow-sm border border-gray-100 text-center max-w-sm w-full">
                <h1 className="text-2xl font-bold mb-4 text-gray-900">Inloggad! 🎉</h1>
                <p className="text-gray-600 mb-8">Vi har nu din JWT-token sparad i webbläsaren.</p>
                <button
                    onClick={() => { localStorage.removeItem('token'); setIsLoggedIn(false); }}
                    className="w-full py-3 bg-gray-100 text-gray-600 font-bold rounded-2xl hover:bg-gray-200 transition-all"
                >
                    Logga ut
                </button>
            </div>
        </div>
    );
}

export default App;