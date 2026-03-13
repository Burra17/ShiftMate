import { useState, useEffect } from 'react';
import { fetchAllUsers } from '../api';
import ShiftForm from './manager/ShiftForm';
import ShiftListTab from './manager/ShiftListTab';
import UserManagement from './manager/UserManagement';
import InviteCode from './manager/InviteCode';

const tabs = [
    { id: 'shifts', label: 'Nytt Pass', color: 'purple' },
    { id: 'allShifts', label: 'Alla Pass', color: 'emerald' },
    { id: 'users', label: 'Användare', color: 'blue' },
    { id: 'invite', label: 'Inbjudningskod', color: 'amber' },
];

const tabColors = {
    purple: 'bg-purple-500/10 border-purple-500/30 text-purple-300',
    emerald: 'bg-emerald-500/10 border-emerald-500/30 text-emerald-300',
    blue: 'bg-blue-500/10 border-blue-500/30 text-blue-300',
    amber: 'bg-amber-500/10 border-amber-500/30 text-amber-300',
};

const ManagerPanel = () => {
    const [activeTab, setActiveTab] = useState('shifts');
    const [users, setUsers] = useState([]);

    useEffect(() => {
        const loadUsers = async () => {
            try {
                const data = await fetchAllUsers();
                setUsers(data);
            } catch (err) {
                console.error("Kunde inte hämta användare:", err);
            }
        };
        loadUsers();
    }, []);

    return (
        <div className="space-y-6">

            {/* Fliknavigation */}
            <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-none">
                {tabs.map(tab => (
                    <button
                        key={tab.id}
                        onClick={() => setActiveTab(tab.id)}
                        className={`px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-wider transition-all whitespace-nowrap border
                            ${activeTab === tab.id
                                ? tabColors[tab.color]
                                : 'border-transparent text-slate-500 hover:text-slate-300 hover:bg-slate-800/40'}`}
                    >
                        {tab.label}
                    </button>
                ))}
            </div>

            {/* Aktiv flik */}
            {activeTab === 'shifts' && <ShiftForm users={users} />}
            {activeTab === 'allShifts' && <ShiftListTab users={users} />}
            {activeTab === 'users' && <UserManagement users={users} onUsersChange={setUsers} />}
            {activeTab === 'invite' && <InviteCode />}
        </div>
    );
};

export default ManagerPanel;
