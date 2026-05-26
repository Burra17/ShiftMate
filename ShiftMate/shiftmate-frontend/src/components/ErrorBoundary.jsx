import { Component } from 'react';

/**
 * ErrorBoundary — fångar renderingsfel i komponentträdet och visar en
 * fallback-vy istället för en vit skärm.
 *
 * OBS: Detta är den enda klasskomponenten i projektet. React saknar en
 * hook-motsvarighet till componentDidCatch/getDerivedStateFromError, så en
 * error boundary MÅSTE vara en klasskomponent. Resten av appen följer
 * regeln "endast funktionskomponenter".
 */
class ErrorBoundary extends Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false };
    }

    // Uppdaterar state så att nästa render visar fallback-vyn.
    static getDerivedStateFromError() {
        return { hasError: true };
    }

    // Loggar felet för felsökning (i prod kan detta kopplas till en logg-tjänst).
    componentDidCatch(error, errorInfo) {
        console.error('Ouppfångat renderingsfel:', error, errorInfo);
    }

    handleReload = () => {
        window.location.reload();
    };

    render() {
        if (this.state.hasError) {
            return (
                <div className="min-h-screen bg-slate-950 flex items-center justify-center p-6">
                    <div className="bg-slate-900/60 backdrop-blur-md p-10 rounded-2xl border border-slate-800 text-center max-w-md w-full">
                        <p className="text-4xl mb-4">⚠️</p>
                        <h1 className="text-xl font-black text-white tracking-tight mb-2 uppercase">
                            Något gick fel
                        </h1>
                        <p className="text-slate-400 font-medium text-sm mb-6">
                            Ett oväntat fel uppstod. Försök ladda om sidan — kvarstår problemet, kontakta din administratör.
                        </p>
                        <button
                            onClick={this.handleReload}
                            className="px-6 py-3 bg-blue-500/10 border border-blue-500/30 text-blue-400 hover:bg-blue-500 hover:text-white text-xs font-black rounded-xl transition-all uppercase tracking-widest"
                        >
                            🔄 Ladda om sidan
                        </button>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}

export default ErrorBoundary;
