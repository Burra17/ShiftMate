import React from 'react';

const AuthLayout = ({ title, subtitle, children }) => {
    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-950 relative overflow-hidden noise-bg">

            {/* Bakgrundseffekter — subtila, organiska ljusfält */}
            <div className="absolute top-[-30%] left-[-15%] w-[700px] h-[700px] bg-blue-600/20 rounded-full blur-[180px] animate-pulse duration-[4000ms]"></div>
            <div className="absolute bottom-[-25%] right-[-10%] w-[600px] h-[600px] bg-cyan-600/15 rounded-full blur-[160px] animate-pulse duration-[5000ms]"></div>
            <div className="absolute top-[40%] left-[60%] w-[300px] h-[300px] bg-indigo-500/8 rounded-full blur-[120px]"></div>

            {/* Geometriskt rutnät — ger djup och struktur */}
            <div className="absolute inset-0 opacity-[0.04]"
                style={{
                    backgroundImage: `
                        linear-gradient(rgba(148,163,184,0.3) 1px, transparent 1px),
                        linear-gradient(90deg, rgba(148,163,184,0.3) 1px, transparent 1px)
                    `,
                    backgroundSize: '60px 60px'
                }}
            ></div>

            {/* Glas-kort */}
            <div className="w-full max-w-md bg-slate-900/60 backdrop-blur-2xl border border-slate-700/40 p-8 md:p-12 rounded-3xl shadow-2xl shadow-blue-950/40 relative z-10 animate-fade-up mx-4">

                {/* Subtil toppkant-glow */}
                <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-blue-500/40 to-transparent"></div>

                <div className="text-center mb-10">
                    {/* Logotyp med ring och glow */}
                    <div className="inline-flex items-center justify-center w-18 h-18 rounded-2xl mb-6 ring-2 ring-blue-500/25 shadow-lg shadow-blue-500/30 overflow-hidden bg-slate-950/80">
                        <img src="/favicon.svg" alt="ShiftMate" className="w-full h-full" />
                    </div>
                    {/* Titel med gradient */}
                    <h1 className="text-4xl font-extrabold tracking-tight mb-2 bg-gradient-to-r from-blue-400 via-cyan-400 to-blue-400 bg-clip-text text-transparent">
                        {title}
                    </h1>
                    <p className="text-slate-400 text-sm font-medium tracking-wide">
                        {subtitle}
                    </p>
                </div>

                {children}
            </div>
        </div>
    );
};

export default AuthLayout;
