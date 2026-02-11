// Gemensamma hjälpfunktioner för datum- och tidsformatering (sv-SE)

/**
 * Formaterar ett datum till kort svenskt format, t.ex. "MÅN 3 FEB"
 */
export const formatDate = (dateStr) => {
    if (!dateStr) return "OKÄNT DATUM";
    return new Date(dateStr).toLocaleDateString('sv-SE', {
        weekday: 'short',
        day: 'numeric',
        month: 'short'
    }).toUpperCase();
};

/**
 * Formaterar en tidpunkt till HH:MM, t.ex. "08:30"
 */
export const formatTime = (dateStr) => {
    if (!dateStr) return "";
    return new Date(dateStr).toLocaleTimeString('sv-SE', {
        hour: '2-digit',
        minute: '2-digit'
    });
};

/**
 * Formaterar ett tidsintervall, t.ex. "08:30 - 17:00"
 */
export const formatTimeRange = (startStr, endStr) => {
    if (!startStr || !endStr) return "--:--";
    return `${formatTime(startStr)} - ${formatTime(endStr)}`;
};
