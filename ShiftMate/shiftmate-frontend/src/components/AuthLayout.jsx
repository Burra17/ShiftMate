import React from 'react';

// AuthLayout-komponenten tillhandahåller en gemensam layout för autentiseringsrelaterade sidor (t.ex. inloggning, registrering).
// Den tar emot 'title', 'subtitle' och 'children' som props.
const AuthLayout = ({ title, subtitle, children }) => {
    return (
        // Huvudcontainer som centrerar innehållet och sätter bakgrundsstil.
        <div className="min-h-screen flex items-center justify-center bg-slate-950 relative overflow-hidden font-sans selection:bg-blue-500 selection:text-white">

            {/* Bakgrundseffekter: Cirklar med oskärpa och animerad puls för en "neon-effekt". */}
            <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-blue-600/25 rounded-full blur-[150px] animate-pulse duration-[4000ms]"></div>
            <div className="absolute bottom-[-20%] right-[-10%] w-[600px] h-[600px] bg-cyan-600/20 rounded-full blur-[150px] animate-pulse duration-[5000ms]"></div>
            <div className="absolute top-[50%] left-[50%] -translate-x-1/2 -translate-y-1/2 w-[400px] h-[400px] bg-indigo-600/10 rounded-full blur-[120px]"></div>

            {/* Subtil punktmönster för djup. */}
            <div className="absolute inset-0 opacity-[0.03]" style={{ backgroundImage: 'radial-gradient(circle, #94a3b8 1px, transparent 1px)', backgroundSize: '24px 24px' }}></div>

            {/* "Glas-kort" (glassmorphism) stiliserad container för autentiseringsformulär. */}
            <div className="w-full max-w-md bg-slate-900/70 backdrop-blur-2xl border border-slate-700/50 p-8 md:p-12 rounded-3xl shadow-2xl shadow-blue-950/50 relative z-10 animate-in fade-in zoom-in duration-500 mx-4">
                <div className="text-center mb-10">
                    {/* Applikationens logotyp med glödeffekt. */}
                    <div className="inline-flex items-center justify-center w-18 h-18 rounded-2xl mb-6 ring-2 ring-blue-500/30 shadow-lg shadow-blue-500/40 overflow-hidden bg-slate-950">
                        <img src="/favicon.svg" alt="ShiftMate" className="w-full h-full" />
                    </div>
                    {/* Titel med gradient-text som matchar logotypens färger. */}
                    <h1 className="text-4xl font-black tracking-tight mb-2 bg-gradient-to-r from-blue-400 via-cyan-400 to-blue-400 bg-clip-text text-transparent">{title}</h1>
                    {/* Undertitel för sidan, dynamiskt satt via props. */}
                    <p className="text-slate-400 text-sm font-medium tracking-wide">
                        {subtitle}
                    </p>
                </div>

                {/* 'children' prop renderar innehållet som skickas till AuthLayout,
                    vilket typiskt är formulär och navigeringselement. */}
                {children}
            </div>
        </div>
    );
};

export default AuthLayout;