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
            console.log("Token expired or invalid, logging out...");
            logoutCallback();
        }
        return Promise.reject(error);
    }
);

/**
 * HÄMTA PASS (Shifts)
 * @param {boolean} onlyWithUsers - Om true, hämtas endast pass som har en ägare (för direktbyten).
 * Om false, hämtas alla pass (för schemavyn).
 */
export const fetchShifts = async (onlyWithUsers = false) => {
    try {
        const response = await axiosInstance.get('/shifts', {
            params: { onlyWithUsers } // Detta skickar ?onlyWithUsers=true i URL-en
        });
        return response.data;
    } catch (error) {
        console.error("Kunde inte hämta pass:", error);
        throw error;
    }
};

/**
 * FÖRESLÅ DIREKTBYTE (Swap)
 * Skickar förfrågan till backend för att initiera ett byte mellan två specifika pass.
 */
export const proposeDirectSwap = async (myShiftId, targetShiftId) => {
    try {
        const response = await axiosInstance.post('/swaprequests/propose-direct', {
            myShiftId,
            targetShiftId
        });
        return response.data;
    } catch (error) {
        console.error("Kunde inte föreslå byte:", error);
        throw error;
    }
};

// --- NY FUNKTION: Hämta roll från token ---
export const getUserRole = () => {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
        // 1. Dela upp token (Header.Payload.Signature)
        const base64Url = token.split('.')[1];

        // 2. Fixa Base64-formatet
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

        // 3. Avkoda till läsbar text
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payload = JSON.parse(jsonPayload);

        // 4. Returnera rollen (hanterar både Microsoft-format och standard)
        return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role;
    } catch (e) {
        console.error("Kunde inte läsa roll från token:", e);
        return null;
    }
};

export default axiosInstance;