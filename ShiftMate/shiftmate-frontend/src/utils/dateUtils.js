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

// ---------------------------------------------------------
// Kalenderhjälpfunktioner för Schema-vyn
// ---------------------------------------------------------

/**
 * Returnerar måndagen i samma vecka som det givna datumet (ISO 8601).
 */
export const getMonday = (date) => {
    const d = new Date(date);
    const day = d.getDay(); // 0=söndag, 1=måndag...
    const diff = day === 0 ? -6 : 1 - day;
    d.setDate(d.getDate() + diff);
    d.setHours(0, 0, 0, 0);
    return d;
};

/**
 * Returnerar ISO-veckonummer (1-53) för ett datum.
 */
export const getWeekNumber = (date) => {
    const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7));
    const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    return Math.ceil(((d - yearStart) / 86400000 + 1) / 7);
};

/**
 * Returnerar 42 datum (6 rader × 7 kolumner) för en månadsvy.
 * Börjar från måndagen innan/på månadens första dag.
 */
export const getCalendarDays = (year, month) => {
    const firstDay = new Date(year, month, 1);
    const startDate = getMonday(firstDay);
    const days = [];
    for (let i = 0; i < 42; i++) {
        const d = new Date(startDate);
        d.setDate(startDate.getDate() + i);
        days.push(d);
    }
    return days;
};

/**
 * Jämför om två datum är samma dag (ignorerar tid).
 */
export const isSameDay = (d1, d2) => {
    return d1.getFullYear() === d2.getFullYear() &&
        d1.getMonth() === d2.getMonth() &&
        d1.getDate() === d2.getDate();
};

/**
 * Kontrollerar om ett datum är idag.
 */
export const isToday = (date) => {
    return isSameDay(date, new Date());
};

/**
 * Formaterar "Februari 2026" (för månadsvy-rubriken).
 */
export const formatMonthYear = (date) => {
    const str = date.toLocaleDateString('sv-SE', { month: 'long', year: 'numeric' });
    return str.charAt(0).toUpperCase() + str.slice(1);
};

/**
 * Formaterar "Vecka 7, 2026" (för veckovy-rubriken).
 */
export const formatWeekLabel = (date) => {
    const weekNum = getWeekNumber(date);
    return `Vecka ${weekNum}, ${date.getFullYear()}`;
};

/**
 * Formaterar "Tisdag 11 februari 2026" (för dagsvy-rubriken).
 */
export const formatDayLabel = (date) => {
    const str = date.toLocaleDateString('sv-SE', {
        weekday: 'long',
        day: 'numeric',
        month: 'long',
        year: 'numeric'
    });
    return str.charAt(0).toUpperCase() + str.slice(1);
};

/**
 * Returnerar kort veckodagsnamn, t.ex. "Mån", "Tis", etc.
 */
export const getShortWeekday = (date) => {
    const str = date.toLocaleDateString('sv-SE', { weekday: 'short' });
    return str.charAt(0).toUpperCase() + str.slice(1);
};

/**
 * Lägger till/tar bort dagar från ett datum.
 */
export const addDays = (date, days) => {
    const d = new Date(date);
    d.setDate(d.getDate() + days);
    return d;
};

/**
 * Lägger till/tar bort månader från ett datum.
 */
export const addMonths = (date, months) => {
    const d = new Date(date);
    d.setMonth(d.getMonth() + months);
    return d;
};
