import React from 'react';

// AuthLayout-komponenten tillhandahåller en gemensam layout för autentiseringsrelaterade sidor (t.ex. inloggning, registrering).
// Den tar emot 'title', 'subtitle' och 'children' som props.
const AuthLayout = ({ title, subtitle, children }) => {
    return (
        // Huvudcontainer som centrerar innehållet och sätter bakgrundsstil.
        <div className="min-h-screen flex items-center justify-center bg-slate-950 relative overflow-hidden font-sans selection:bg-purple-500 selection:text-white">

            {/* Bakgrundseffekter: Cirklar med oskärpa och animerad puls för en "neon-effekt". */}
            <div className="absolute top-[-20%] left-[-10%] w-[500px] h-[500px] bg-purple-600/20 rounded-full blur-[120px] animate-pulse duration-[4000ms]"></div>
            <div className="absolute bottom-[-20%] right-[-10%] w-[500px] h-[500px] bg-blue-600/20 rounded-full blur-[120px] animate-pulse duration-[5000ms]"></div>

            {/* "Glas-kort" (glassmorphism) stiliserad container för autentiseringsformulär. */}
            <div className="w-full max-w-md bg-slate-900/60 backdrop-blur-2xl border border-slate-800 p-8 md:p-12 rounded-3xl shadow-2xl relative z-10 animate-in fade-in zoom-in duration-500 mx-4">
                <div className="text-center mb-10">
                    {/* Applikationens logotyp och varumärke. */}
                    <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-2xl mb-6 shadow-lg shadow-indigo-500/30">
                        <span className="text-3xl filter drop-shadow-md">⛽</span>
                    </div>
                    {/* Titel för sidan, dynamiskt satt via props. */}
                    <h1 className="text-4xl font-black text-white tracking-tight mb-2">{title}</h1>
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