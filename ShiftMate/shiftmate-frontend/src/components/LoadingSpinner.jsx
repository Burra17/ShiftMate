/**
 * LoadingSpinner — Delad laddningsindikator för alla sidor.
 * Visar en animerad spinner med valfritt meddelande.
 */
const LoadingSpinner = ({ message = 'Laddar...' }) => {
    return (
        <div className="flex flex-col items-center justify-center py-16 gap-4">
            <div className="w-8 h-8 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
            <p className="text-sm font-bold text-slate-500 uppercase tracking-widest">{message}</p>
        </div>
    );
};

export default LoadingSpinner;
