import { useState, useEffect } from 'react';
import {
    fetchOrganizationsDetail,
    createOrganization,
    updateOrganization,
    deleteOrganization
} from './api';
import { useToast, useConfirm } from './contexts/ToastContext';

const AdminPanel = () => {
    const [organizations, setOrganizations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState(null);

    // Skapa-formulär
    const [newName, setNewName] = useState('');
    const [creating, setCreating] = useState(false);

    // Redigering
    const [editingId, setEditingId] = useState(null);
    const [editName, setEditName] = useState('');

    const toast = useToast();
    const confirm = useConfirm();

    const loadOrganizations = async () => {
        try {
            const data = await fetchOrganizationsDetail();
            setOrganizations(data);
        } catch {
            toast.error('Kunde inte hämta organisationer.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadOrganizations();
    }, []);

    const handleCreate = async (e) => {
        e.preventDefault();
        if (!newName.trim()) return;

        setCreating(true);
        try {
            await createOrganization(newName.trim());
            toast.success('Organisationen har skapats!');
            setNewName('');
            await loadOrganizations();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Kunde inte skapa organisationen.');
        } finally {
            setCreating(false);
        }
    };

    const handleUpdate = async (id) => {
        if (!editName.trim()) return;

        setActionLoading(id);
        try {
            await updateOrganization(id, editName.trim());
            toast.success('Organisationen har uppdaterats!');
            setEditingId(null);
            await loadOrganizations();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Kunde inte uppdatera organisationen.');
        } finally {
            setActionLoading(null);
        }
    };

    const handleDelete = async (org) => {
        const message = org.userCount > 0
            ? `Du håller på att permanent radera organisationen "${org.name}" tillsammans med alla ${org.userCount} användare, deras pass och bytesförfrågningar. Detta går inte att ångra.`
            : `Du håller på att permanent radera organisationen "${org.name}". Detta går inte att ångra.`;
        const confirmed = await confirm({
            title: 'Radera organisation',
            message,
            confirmLabel: 'Radera permanent',
            cancelLabel: 'Avbryt',
            variant: 'danger',
        });
        if (!confirmed) return;

        setActionLoading(org.id);
        try {
            await deleteOrganization(org.id);
            toast.success('Organisationen har raderats!');
            await loadOrganizations();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Kunde inte ta bort organisationen.');
        } finally {
            setActionLoading(null);
        }
    };

    const startEditing = (org) => {
        setEditingId(org.id);
        setEditName(org.name);
    };

    const cancelEditing = () => {
        setEditingId(null);
        setEditName('');
    };

    const totalUsers = organizations.reduce((sum, o) => sum + o.userCount, 0);

    if (loading) {
        return (
            <div className="flex items-center justify-center py-20">
                <div className="w-8 h-8 border-2 border-blue-400 border-t-transparent rounded-full animate-spin"></div>
                <span className="ml-3 text-slate-400">Laddar organisationer...</span>
            </div>
        );
    }

    return (
        <div className="space-y-8">
            {/* Statistikkort */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="bg-slate-900/50 border border-slate-800 rounded-xl p-5">
                    <p className="text-slate-400 text-xs font-bold uppercase tracking-wider">Organisationer</p>
                    <p className="text-3xl font-black text-white mt-1">{organizations.length}</p>
                </div>
                <div className="bg-slate-900/50 border border-slate-800 rounded-xl p-5">
                    <p className="text-slate-400 text-xs font-bold uppercase tracking-wider">Totalt antal användare</p>
                    <p className="text-3xl font-black text-white mt-1">{totalUsers}</p>
                </div>
            </div>

            {/* Skapa ny organisation */}
            <div className="bg-slate-900/50 border border-slate-800 rounded-xl p-6">
                <h3 className="text-lg font-bold text-white mb-4">Skapa ny organisation</h3>
                <form onSubmit={handleCreate} className="flex gap-3">
                    <input
                        type="text"
                        value={newName}
                        onChange={(e) => setNewName(e.target.value)}
                        placeholder="Organisationsnamn..."
                        className="flex-1 bg-slate-800 border border-slate-700 rounded-lg px-4 py-2.5 text-white placeholder-slate-500 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                        maxLength={100}
                    />
                    <button
                        type="submit"
                        disabled={creating || !newName.trim()}
                        className="px-6 py-2.5 bg-blue-600 hover:bg-blue-500 disabled:bg-slate-700 disabled:text-slate-500 text-white font-bold rounded-lg transition-colors"
                    >
                        {creating ? 'Skapar...' : 'Skapa'}
                    </button>
                </form>
            </div>

            {/* Organisationslista */}
            <div className="bg-slate-900/50 border border-slate-800 rounded-xl overflow-hidden">
                <div className="px-6 py-4 border-b border-slate-800">
                    <h3 className="text-lg font-bold text-white">Alla organisationer</h3>
                </div>
                <div className="divide-y divide-slate-800">
                    {organizations.map((org) => (
                        <div key={org.id} className="px-6 py-4 flex items-center gap-4">
                            {editingId === org.id ? (
                                <>
                                    <input
                                        type="text"
                                        value={editName}
                                        onChange={(e) => setEditName(e.target.value)}
                                        className="flex-1 bg-slate-800 border border-blue-500 rounded-lg px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
                                        maxLength={100}
                                        autoFocus
                                        onKeyDown={(e) => {
                                            if (e.key === 'Enter') handleUpdate(org.id);
                                            if (e.key === 'Escape') cancelEditing();
                                        }}
                                    />
                                    <button
                                        onClick={() => handleUpdate(org.id)}
                                        disabled={actionLoading === org.id}
                                        className="px-4 py-2 bg-green-600 hover:bg-green-500 disabled:bg-slate-700 text-white text-sm font-bold rounded-lg transition-colors"
                                    >
                                        {actionLoading === org.id ? 'Sparar...' : 'Spara'}
                                    </button>
                                    <button
                                        onClick={cancelEditing}
                                        className="px-4 py-2 bg-slate-700 hover:bg-slate-600 text-slate-300 text-sm font-bold rounded-lg transition-colors"
                                    >
                                        Avbryt
                                    </button>
                                </>
                            ) : (
                                <>
                                    <div className="flex-1 min-w-0">
                                        <p className="text-white font-bold truncate">{org.name}</p>
                                        <p className="text-slate-400 text-sm">
                                            {org.userCount} {org.userCount === 1 ? 'användare' : 'användare'}
                                            <span className="mx-2 text-slate-600">·</span>
                                            Skapad {new Date(org.createdAt).toLocaleDateString('sv-SE')}
                                        </p>
                                    </div>
                                    <button
                                        onClick={() => startEditing(org)}
                                        className="px-3 py-1.5 text-slate-400 hover:text-blue-400 hover:bg-slate-800 rounded-lg transition-colors text-sm font-bold"
                                    >
                                        Redigera
                                    </button>
                                    <button
                                        onClick={() => handleDelete(org)}
                                        disabled={actionLoading === org.id}
                                        className="px-3 py-1.5 text-slate-400 hover:text-red-400 hover:bg-slate-800 disabled:text-slate-600 disabled:hover:bg-transparent rounded-lg transition-colors text-sm font-bold"
                                    >
                                        Ta bort
                                    </button>
                                </>
                            )}
                        </div>
                    ))}
                    {organizations.length === 0 && (
                        <div className="px-6 py-12 text-center text-slate-500">
                            Inga organisationer hittades.
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default AdminPanel;
