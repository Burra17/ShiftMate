// shiftmate-frontend/src/api.js
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL;

let logoutCallback = () => {}; // Standardfunktion som inte gör något

export const setLogoutCallback = (callback) => {
  logoutCallback = callback;
};

const axiosInstance = axios.create({
  baseURL: API_URL,
});

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

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    // Om vi får en 401 Unauthorized, logga ut användaren
    if (error.response?.status === 401) {
      console.log("Token expired or invalid, logging out...");
      logoutCallback(); // Anropa den registrerade utloggningsfunktionen
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
