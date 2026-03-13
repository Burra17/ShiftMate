import { useState } from 'react';
import { deleteUser, updateUserRole, getCurrentUserId } from '../../api';
import { useToast, useConfirm } from '../../contexts/ToastContext';

const UserManagement = ({ users, onUsersChange }) => {
    const toast = useToast();
    const confirm = useConfirm();
    const currentUserId = getCurrentUserId();
    const [updatingUserId, setUpdatingUserId] = useState(null);

    const handleRoleChange = async (targetUserId, newRole) => {
        setUpdatingUserId(targetUserId);
        try {
            await updateUserRole(targetUserId, newRole);
            onUsersChange(prev =>
                prev.map(u => u.id === targetUserId ? { ...u, role: newRole } : u)
            );
            toast.success("Rollen har uppdaterats.");
        } catch (err) {
            const msg = err.response?.data?.message || "Kunde inte uppdatera rollen.";
            toast.error(msg);
        } finally {
            setUpdatingUserId(null);
        }
    };

    const handleDeleteUser = async (targetUserId, fullName) => {
        const confirmed = await confirm(`Är du säker på att du vill inaktivera ${fullName}? Användarens pass frigörs och väntande bytesförfrågningar avbryts.`);
        if (!confirmed) return;
        try {
            await deleteUser(targetUserId);
            onUsersChange(prev => prev.filter(u => u.id !== targetUserId));
            toast.success(`${fullName} har inaktiverats.`);
        } catch (err) {
            const msg = err.response?.data?.message || "Kunde inte inaktivera användaren.";
            toast.error(msg);
        }
    };

    return (
        <div className="space-y-3">
            {users.length === 0 ? (
                <p className="text-slate-500 text-sm text-center py-8">Inga användare hittades.</p>
            ) : (
                users.map(user => {
                    const isSelf = user.id === currentUserId;
                    const initials = `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase();
                    return (
                        <div
                            key={user.id}
                            className="flex items-center gap-4 bg-slate-800/50 border border-slate-700 rounded-xl px-4 py-3"
                        >
                            {/* Initialer-avatar */}
                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-600 to-indigo-600 flex items-center justify-center text-white text-sm font-black flex-shrink-0">
                                {initials}
                            </div>

                            {/* Namn + e-post */}
                            <div className="flex-1 min-w-0">
                                <p className="text-white text-sm font-semibold truncate">
                                    {user.firstName} {user.lastName}
                                    {isSelf && <span className="ml-2 text-xs text-slate-500">(du)</span>}
                                </p>
                                <p className="text-slate-400 text-xs truncate">{user.email}</p>
                            </div>

                            {/* Roll-badge */}
                            <span className={`text-[10px] font-black uppercase tracking-widest px-2 py-1 rounded-full border flex-shrink-0
                                ${user.role === 'Manager'
                                    ? 'bg-amber-500/10 border-amber-500/30 text-amber-400'
                                    : 'bg-blue-500/10 border-blue-500/30 text-blue-400'
                                }`}
                            >
                                {user.role}
                            </span>

                            {/* Roll-dropdown */}
                            <select
                                value={user.role}
                                disabled={isSelf || updatingUserId === user.id}
                                onChange={(e) => handleRoleChange(user.id, e.target.value)}
                                className="bg-slate-700 border border-slate-600 text-white text-xs rounded-lg px-2 py-1.5 focus:outline-none focus:border-blue-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0"
                            >
                                <option value="Employee">Employee</option>
                                <option value="Manager">Manager</option>
                            </select>

                            {/* Inaktivera-knapp */}
                            <button
                                disabled={isSelf}
                                onClick={() => handleDeleteUser(user.id, `${user.firstName} ${user.lastName}`)}
                                className="text-red-400 hover:text-red-300 transition-colors disabled:opacity-30 disabled:cursor-not-allowed flex-shrink-0 p-1"
                                title={isSelf ? 'Du kan inte inaktivera ditt eget konto' : `Inaktivera ${user.firstName}`}
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                </svg>
                            </button>
                        </div>
                    );
                })
            )}
        </div>
    );
};

export default UserManagement;
