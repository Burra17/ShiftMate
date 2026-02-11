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

export default axiosInstance;
