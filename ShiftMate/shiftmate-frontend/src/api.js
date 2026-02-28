// shiftmate-frontend/src/api.js
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL;

let logoutCallback = () => { }; // Standardfunktion som inte gör något

export const setLogoutCallback = (callback) => {
    logoutCallback = callback;
};

const axiosInstance = axios.create({
    baseURL: API_URL,
});

// Interceptor för att bifoga JWT-token vid varje anrop
axiosInstance.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Interceptor för att hantera utgångna tokens (401 Unauthorized)
axiosInstance.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            console.log("Token har gått ut eller är ogiltigt, loggar ut...");
            logoutCallback();
        }
        return Promise.reject(error);
    }
);

// ---------------------------------------------------------
// GEMENSAM HJÄLPFUNKTION: Avkoda JWT-token
// ---------------------------------------------------------

/**
 * Avkodar JWT-tokenet från localStorage och returnerar payload-objektet.
 * Returnerar null om inget token finns eller om avkodningen misslyckas.
 */
export const decodeToken = () => {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error("Kunde inte avkoda token:", e);
        return null;
    }
};

// ---------------------------------------------------------
// SHIFTS - Centraliserade API-funktioner
// ---------------------------------------------------------

/**
 * Hämta alla pass, med valfritt filter för att bara inkludera pass med ägare.
 */
export const fetchShifts = async (onlyWithUsers = false) => {
    const response = await axiosInstance.get('/shifts', {
        params: { onlyWithUsers }
    });
    return response.data;
};

/**
 * Hämta den inloggade användarens egna pass.
 */
export const fetchMyShifts = async () => {
    const response = await axiosInstance.get('/shifts/mine');
    return response.data;
};

/**
 * Hämta pass som är lediga att ta (marknadsplatsen).
 */
export const fetchClaimableShifts = async () => {
    const response = await axiosInstance.get('/Shifts/claimable');
    return response.data;
};

/**
 * Ta ett ledigt pass.
 */
export const takeShift = async (shiftId) => {
    const response = await axiosInstance.put(`/Shifts/${shiftId}/take`, {});
    return response.data;
};

/**
 * Ångra att ett pass ligger ute för byte.
 */
export const cancelShiftSwap = async (shiftId) => {
    const response = await axiosInstance.put(`/shifts/${shiftId}/cancel-swap`);
    return response.data;
};

// ---------------------------------------------------------
// SWAP REQUESTS - Centraliserade API-funktioner
// ---------------------------------------------------------

/**
 * Lägg ut ett pass på marknadsplatsen för byte.
 */
export const initiateSwap = async (shiftId) => {
    const response = await axiosInstance.post('/SwapRequests/initiate', { shiftId });
    return response.data;
};

/**
 * Föreslå ett direktbyte mellan två pass.
 */
export const proposeDirectSwap = async (myShiftId, targetShiftId) => {
    const response = await axiosInstance.post('/swaprequests/propose-direct', {
        myShiftId,
        targetShiftId
    });
    return response.data;
};

/**
 * Hämta inkommande bytesförfrågningar.
 */
export const fetchReceivedSwapRequests = async () => {
    const response = await axiosInstance.get('/swaprequests/received');
    return response.data;
};

/**
 * Hämta skickade bytesförfrågningar (väntande).
 */
export const fetchSentSwapRequests = async () => {
    const response = await axiosInstance.get('/swaprequests/sent');
    return response.data;
};

/**
 * Acceptera en bytesförfrågan.
 */
export const acceptSwapRequest = async (swapRequestId) => {
    const response = await axiosInstance.post('/swaprequests/accept', { swapRequestId });
    return response.data;
};

/**
 * Neka en bytesförfrågan.
 */
export const declineSwapRequest = async (requestId) => {
    const response = await axiosInstance.post(`/swaprequests/${requestId}/decline`);
    return response.data;
};

// ---------------------------------------------------------
// TOKEN-BASERADE HJÄLPFUNKTIONER
// ---------------------------------------------------------

/**
 * Hämta användarens roll från JWT-tokenet.
 */
export const getUserRole = () => {
    const payload = decodeToken();
    if (!payload) return null;
    return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role;
};

/**
 * Hämta den inloggade användarens ID (Guid) från JWT-tokenet.
 */
export const getCurrentUserId = () => {
    const payload = decodeToken();
    if (!payload) return null;
    return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || payload.sub;
};

/**
 * Hämta den inloggade användarens organisations-ID från JWT-tokenet.
 */
export const getOrganizationId = () => {
    const payload = decodeToken();
    if (!payload) return null;
    return payload["OrganizationId"] || null;
};

/**
 * Hämta den inloggade användarens organisationsnamn från JWT-tokenet.
 */
export const getOrganizationName = () => {
    const payload = decodeToken();
    if (!payload) return null;
    return payload["OrganizationName"] || null;
};

/**
 * Hämta alla organisationer (för registreringssidan).
 */
export const fetchOrganizations = async () => {
    const res = await axiosInstance.get('/Organizations');
    return res.data;
};

/**
 * Hämta alla organisationer med detaljer (SuperAdmin).
 */
export const fetchOrganizationsDetail = async () => {
    const res = await axiosInstance.get('/Organizations/admin');
    return res.data;
};

/**
 * Skapa en ny organisation (SuperAdmin).
 */
export const createOrganization = async (name) => {
    const res = await axiosInstance.post('/Organizations', { name });
    return res.data;
};

/**
 * Uppdatera en organisation (SuperAdmin).
 */
export const updateOrganization = async (id, name) => {
    const res = await axiosInstance.put(`/Organizations/${id}`, { name });
    return res.data;
};

/**
 * Radera en organisation (SuperAdmin).
 */
export const deleteOrganization = async (id) => {
    const res = await axiosInstance.delete(`/Organizations/${id}`);
    return res.data;
};

/**
 * Uppdatera den inloggade användarens profil (namn och e-post).
 */
export const updateProfile = async (data) => {
    const res = await axiosInstance.put('/Users/profile', data);
    return res.data;
};

/**
 * Byt lösenord för den inloggade användaren.
 */
export const changePassword = async (data) => {
    const res = await axiosInstance.put('/Users/change-password', data);
    return res.data;
};

/**
 * Skapa ett pass via manager-endpoint.
 */
export const createManagerShift = async (payload) => {
    const res = await axiosInstance.post('/Shifts/admin', payload);
    return res.data;
};

/**
 * Uppdatera ett pass (manager only).
 */
export const updateShift = async (shiftId, payload) => {
    const res = await axiosInstance.put(`/Shifts/${shiftId}`, payload);
    return res.data;
};

/**
 * Radera ett pass (manager only).
 */
export const deleteShift = async (shiftId) => {
    const res = await axiosInstance.delete(`/Shifts/${shiftId}`);
    return res.data;
};

/**
 * Hämta alla användare.
 */
export const fetchAllUsers = async () => {
    const res = await axiosInstance.get('/Users');
    return res.data;
};

/**
 * Radera en användare.
 */
export const deleteUser = async (userId) => {
    const res = await axiosInstance.delete(`/Users/${userId}`);
    return res.data;
};

/**
 * Uppdatera en användares roll.
 */
export const updateUserRole = async (userId, role) => {
    const res = await axiosInstance.put(`/Users/${userId}/role`, { newRole: role });
    return res.data;
};

/**
 * Begär lösenordsåterställning via e-post.
 */
export const forgotPassword = async (email) => {
    const res = await axiosInstance.post('/Users/forgot-password', { email });
    return res.data;
};

/**
 * Återställ lösenord med token från e-post.
 */
export const resetPassword = async (token, email, newPassword) => {
    const res = await axiosInstance.post('/Users/reset-password', { token, email, newPassword });
    return res.data;
};

export default axiosInstance;
