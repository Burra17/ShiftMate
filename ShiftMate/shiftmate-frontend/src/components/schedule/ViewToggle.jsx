// Segmenterad kontroll för att växla mellan Dag/Vecka/Månad

const views = [
    { id: 'day', label: 'Dag' },
    { id: 'week', label: 'Vecka' },
    { id: 'month', label: 'Månad' },
];

const ViewToggle = ({ viewMode, onViewChange }) => {
    return (
        <div className="inline-flex bg-slate-900/60 rounded-xl border border-slate-800 p-1">
            {views.map((v) => (
                <button
                    key={v.id}
                    onClick={() => onViewChange(v.id)}
                    className={`px-4 py-1.5 rounded-lg text-xs font-bold transition-all duration-200
                        ${viewMode === v.id
                            ? 'bg-blue-600/20 text-blue-400 border border-blue-500/30 shadow-[0_0_10px_rgba(59,130,246,0.15)]'
                            : 'text-slate-400 hover:text-white border border-transparent'
                        }`}
                >
                    {v.label}
                </button>
            ))}
        </div>
    );
};

export default ViewToggle;
