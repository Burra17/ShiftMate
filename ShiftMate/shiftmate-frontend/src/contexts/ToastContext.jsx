import { createContext, useContext, useState, useCallback, useRef } from 'react';
import { createPortal } from 'react-dom';
import ToastContainer from '../components/ui/ToastContainer';
import ConfirmModal from '../components/ui/ConfirmModal';

// Kontext för toast-notifikationer och bekräftelsedialoger
const ToastContext = createContext(null);

let toastIdCounter = 0;

export const ToastProvider = ({ children }) => {
  const [toasts, setToasts] = useState([]);
  const [confirmState, setConfirmState] = useState(null);
  const confirmResolveRef = useRef(null);

  // --- Toast-hantering ---

  const addToast = useCallback((type, message) => {
    const id = ++toastIdCounter;

    setToasts(prev => {
      // Max 5 toasts synliga samtidigt
      const updated = [...prev, { id, type, message, exiting: false }];
      return updated.length > 5 ? updated.slice(updated.length - 5) : updated;
    });

    // Auto-dismiss efter 4 sekunder
    setTimeout(() => dismissToast(id), 4000);

    return id;
  }, []);

  const dismissToast = useCallback((id) => {
    // Markera som exiting för att trigga ut-animation
    setToasts(prev => prev.map(t => t.id === id ? { ...t, exiting: true } : t));

    // Ta bort helt efter animationen (300ms)
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 300);
  }, []);

  const toast = {
    success: (message) => addToast('success', message),
    error: (message) => addToast('error', message),
    info: (message) => addToast('info', message),
    warning: (message) => addToast('warning', message),
  };

  // --- Confirm-hantering (Promise-baserad) ---

  const confirm = useCallback((options) => {
    return new Promise((resolve) => {
      confirmResolveRef.current = resolve;
      setConfirmState({
        title: options.title || 'Bekräfta',
        message: options.message || 'Är du säker?',
        confirmLabel: options.confirmLabel || 'Bekräfta',
        cancelLabel: options.cancelLabel || 'Avbryt',
        variant: options.variant || 'default',
      });
    });
  }, []);

  const handleConfirmResolve = useCallback((value) => {
    if (confirmResolveRef.current) {
      confirmResolveRef.current(value);
      confirmResolveRef.current = null;
    }
    setConfirmState(null);
  }, []);

  return (
    <ToastContext.Provider value={{ toast, confirm }}>
      {children}
      {createPortal(
        <>
          <ToastContainer toasts={toasts} onDismiss={dismissToast} />
          {confirmState && (
            <ConfirmModal
              {...confirmState}
              onConfirm={() => handleConfirmResolve(true)}
              onCancel={() => handleConfirmResolve(false)}
            />
          )}
        </>,
        document.body
      )}
    </ToastContext.Provider>
  );
};

// Hook: Returnerar { success, error, info, warning } metoder
export const useToast = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast måste användas inom <ToastProvider>');
  return ctx.toast;
};

// Hook: Returnerar confirm(options) som returnerar Promise<boolean>
export const useConfirm = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useConfirm måste användas inom <ToastProvider>');
  return ctx.confirm;
};
