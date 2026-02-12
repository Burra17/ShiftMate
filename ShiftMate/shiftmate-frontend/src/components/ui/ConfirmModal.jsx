import { useEffect, useRef } from 'react';

const ConfirmModal = ({ title, message, confirmLabel, cancelLabel, variant, onConfirm, onCancel }) => {
  const confirmBtnRef = useRef(null);

  // Fokusera bekräfta-knappen vid öppning och hantera Escape-tangent
  useEffect(() => {
    confirmBtnRef.current?.focus();

    const handleKeyDown = (e) => {
      if (e.key === 'Escape') onCancel();
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onCancel]);

  // Bekräfta-knappens färg baserat på variant
  const confirmBtnClass = variant === 'danger'
    ? 'bg-red-600 hover:bg-red-500 text-white shadow-lg shadow-red-900/30'
    : 'bg-blue-600 hover:bg-blue-500 text-white shadow-lg shadow-blue-900/30';

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      {/* Overlay - klick stänger dialogen */}
      <div className="absolute inset-0 bg-black/80 backdrop-blur-sm" onClick={onCancel} />

      {/* Dialog */}
      <div className="relative bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-sm shadow-2xl">
        <div className="p-6 space-y-4">
          <h3 className="text-lg font-black text-white tracking-tight">{title}</h3>
          <p className="text-sm text-slate-400 leading-relaxed">{message}</p>
        </div>

        <div className="flex gap-3 p-6 pt-2">
          <button
            onClick={onCancel}
            className="flex-1 py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-black rounded-xl transition-all uppercase tracking-widest"
          >
            {cancelLabel}
          </button>
          <button
            ref={confirmBtnRef}
            onClick={onConfirm}
            className={`flex-1 py-2.5 text-xs font-black rounded-xl transition-all uppercase tracking-widest ${confirmBtnClass}`}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmModal;
