import { useState, useEffect } from 'react';
import { getMyInviteCode, regenerateInviteCode, getOrganizationId } from '../../api';
import { useToast, useConfirm } from '../../contexts/ToastContext';

const InviteCode = () => {
    const toast = useToast();
    const confirm = useConfirm();

    const [inviteCode, setInviteCode] = useState('');
    const [orgName, setOrgName] = useState('');
    const [generatedAt, setGeneratedAt] = useState(null);
    const [loading, setLoading] = useState(true);
    const [regenerating, setRegenerating] = useState(false);
    const [copied, setCopied] = useState(false);

    useEffect(() => {
        const loadCode = async () => {
            try {
                const data = await getMyInviteCode();
                setInviteCode(data.inviteCode);
                setOrgName(data.organizationName);
                setGeneratedAt(data.generatedAt);
            } catch (err) {
                console.error("Kunde inte hämta inbjudningskod:", err);
                toast.error("Kunde inte hämta inbjudningskoden.");
            } finally {
                setLoading(false);
            }
        };
        loadCode();
    }, []);

    const handleRegenerate = async () => {
        const confirmed = await confirm({
            title: 'Generera ny inbjudningskod',
            message: 'Den gamla koden slutar fungera omedelbart. Alla som inte har registrerat sig ännu behöver den nya koden.',
            confirmLabel: 'Generera ny kod',
            cancelLabel: 'Avbryt',
            variant: 'danger',
        });
        if (!confirmed) return;

        setRegenerating(true);
        try {
            const orgId = getOrganizationId();
            const data = await regenerateInviteCode(orgId);
            setInviteCode(data.inviteCode);
            setGeneratedAt(new Date().toISOString());
            toast.success("Ny inbjudningskod har genererats!");
        } catch (err) {
            toast.error("Kunde inte generera ny kod.");
        } finally {
            setRegenerating(false);
        }
    };

    const handleCopy = async () => {
        try {
            await navigator.clipboard.writeText(inviteCode);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        } catch {
            toast.error("Kunde inte kopiera koden.");
        }
    };

    if (loading) {
        return (
            <div className="text-center py-12">
                <div className="w-8 h-8 border-2 border-amber-500 border-t-transparent rounded-full animate-spin mx-auto mb-3"></div>
                <p className="text-slate-400 text-sm">Laddar inbjudningskod...</p>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="text-center space-y-2">
                <p className="text-slate-400 text-xs font-bold uppercase tracking-widest">Inbjudningskod för {orgName}</p>
                <p className="text-slate-500 text-xs">Dela denna kod med nya anställda så att de kan registrera sig</p>
            </div>

            {/* Kodvisning */}
            <div className="bg-slate-800/50 border border-amber-500/30 rounded-2xl p-6 text-center space-y-4">
                <p className="text-3xl sm:text-4xl font-black text-amber-300 tracking-[0.3em] font-mono select-all">
                    {inviteCode}
                </p>

                <button
                    onClick={handleCopy}
                    className={`px-6 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest transition-all border
                        ${copied
                            ? 'bg-green-600/20 border-green-500 text-green-300'
                            : 'bg-slate-800 border-slate-700 text-slate-300 hover:border-amber-500 hover:text-amber-300'
                        }`}
                >
                    {copied ? (
                        <span className="flex items-center gap-2">
                            <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                            </svg>
                            Kopierad!
                        </span>
                    ) : (
                        <span className="flex items-center gap-2">
                            <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                            </svg>
                            Kopiera kod
                        </span>
                    )}
                </button>

                {generatedAt && (
                    <p className="text-slate-500 text-xs">
                        Genererad {new Date(generatedAt).toLocaleDateString('sv-SE', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}
                    </p>
                )}
            </div>

            {/* Generera ny kod */}
            <div className="pt-2">
                <button
                    onClick={handleRegenerate}
                    disabled={regenerating}
                    className={`w-full py-3 rounded-xl font-black uppercase tracking-widest text-xs transition-all border
                        ${regenerating
                            ? 'bg-slate-700 text-slate-500 border-slate-700 cursor-not-allowed'
                            : 'bg-red-500/10 border-red-500/30 text-red-400 hover:bg-red-500/20 hover:border-red-500/50'
                        }`}
                >
                    {regenerating ? 'Genererar...' : 'Generera ny kod'}
                </button>
                <p className="text-[10px] text-slate-600 text-center mt-2">Den gamla koden slutar fungera omedelbart</p>
            </div>
        </div>
    );
};

export default InviteCode;
