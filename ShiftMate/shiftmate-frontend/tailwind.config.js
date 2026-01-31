/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                okorange: '#FF6600', // Snygg OKQ8-orange för din pitch!
            }
        },
    },
    plugins: [],
}