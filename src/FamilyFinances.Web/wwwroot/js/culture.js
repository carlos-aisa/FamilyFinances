(function () {
    const STORAGE_KEY = "ff_culture";
    const COOKIE_KEY = ".AspNetCore.Culture";
    const DEFAULT_CULTURE = "es-ES";
    const SUPPORTED = ["es-ES", "en-US"];

    function normalize(culture) {
        if (!culture || typeof culture !== "string") {
            return DEFAULT_CULTURE;
        }

        return SUPPORTED.includes(culture) ? culture : DEFAULT_CULTURE;
    }

    function readCookie() {
        const cookies = document.cookie ? document.cookie.split(";") : [];
        for (const cookie of cookies) {
            const trimmed = cookie.trim();
            if (!trimmed.startsWith(COOKIE_KEY + "=")) {
                continue;
            }

            const value = decodeURIComponent(trimmed.substring(COOKIE_KEY.length + 1));
            const match = /^c=([^|]+)\|uic=([^|]+)$/.exec(value);
            if (match && match[1] === match[2]) {
                return match[1];
            }
        }

        return null;
    }

    function writeCookie(culture) {
        const value = `c=${culture}|uic=${culture}`;
        document.cookie = `${COOKIE_KEY}=${encodeURIComponent(value)}; path=/; max-age=31536000; samesite=lax`;
    }

    function setCulture(culture) {
        const normalized = normalize(culture);
        localStorage.setItem(STORAGE_KEY, normalized);
        writeCookie(normalized);
        return normalized;
    }

    function getCulture() {
        const rawStorageCulture = localStorage.getItem(STORAGE_KEY);
        const rawCookieCulture = readCookie();

        const storageCulture = SUPPORTED.includes(rawStorageCulture) ? rawStorageCulture : null;
        const cookieCulture = SUPPORTED.includes(rawCookieCulture) ? rawCookieCulture : null;
        const resolved = storageCulture || cookieCulture || DEFAULT_CULTURE;

        if (storageCulture !== resolved || cookieCulture !== resolved) {
            setCulture(resolved);
        }

        return resolved;
    }

    window.cultureHelper = {
        getCulture,
        setCulture
    };
})();
