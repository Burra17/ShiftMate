import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // API:ets CORS tillåter bara port 5173 – krascha tydligt om porten är upptagen
    // istället för att tyst byta port och få CORS-fel
    port: 5173,
    strictPort: true,
  },
})
