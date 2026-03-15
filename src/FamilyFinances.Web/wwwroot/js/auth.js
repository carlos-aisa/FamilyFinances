(() => {
    const LAST_USERNAME_KEY = "ff_last_username";

    function normalizeLoginError(message, statusCode) {
        const normalized = typeof message === "string" ? message.trim() : "";
        if (!normalized) {
            return "Login failed";
        }

        if (statusCode === 400 || statusCode === 401 || statusCode === 403) {
            const lowered = normalized.toLowerCase();
            if (
                lowered.includes("authentication failed") ||
                lowered.includes("email and password are required") ||
                lowered.includes("invalid credentials") ||
                lowered.includes("unauthorized")
            ) {
                return "Login failed";
            }
        }

        return normalized;
    }

    async function readErrorMessage(response) {
        let body = "";
        try {
            body = await response.text();
        } catch {
            return "Login failed";
        }

        const trimmed = typeof body === "string" ? body.trim() : "";
        if (!trimmed) {
            return "Login failed";
        }

        const contentType = response.headers.get("content-type") || "";
        if (contentType.includes("application/json") || trimmed.startsWith("{")) {
            try {
                const payload = JSON.parse(trimmed);
                if (typeof payload === "string" && payload.trim().length > 0) {
                    return payload.trim();
                }

                if (payload && typeof payload === "object") {
                    const candidates = [payload.error, payload.detail, payload.message, payload.title];
                    for (const candidate of candidates) {
                        if (typeof candidate === "string" && candidate.trim().length > 0) {
                            return candidate.trim();
                        }
                    }
                }
            } catch {
                // If parsing fails, fall back to plain response text.
            }
        }

        return trimmed;
    }

    async function login(email, password) {
        try {
            const response = await fetch("/auth/session", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ email, password }),
                credentials: "include"
            });

            if (!response.ok) {
                const parsedError = await readErrorMessage(response);
                return { success: false, error: normalizeLoginError(parsedError, response.status) };
            }

            const data = await response.json();
            return { success: true, accessToken: data.accessToken };
        } catch (error) {
            const fallback = error && typeof error.message === "string" ? error.message : "Login failed";
            return { success: false, error: fallback };
        }
    }

    function getLastUsername() {
        try {
            const value = window.localStorage.getItem(LAST_USERNAME_KEY);
            return typeof value === "string" ? value : "";
        } catch {
            return "";
        }
    }

    function setLastUsername(value) {
        try {
            if (typeof value !== "string") {
                return;
            }

            const normalized = value.trim();
            if (normalized.length === 0) {
                window.localStorage.removeItem(LAST_USERNAME_KEY);
                return;
            }

            window.localStorage.setItem(LAST_USERNAME_KEY, normalized);
        } catch {
            // Ignore localStorage errors in private/incognito contexts.
        }
    }

    window.authHelper = {
        login,
        getLastUsername,
        setLastUsername
    };

    window.loginHelper = window.loginHelper || {};
    window.loginHelper.executeLogin = login;
    window.loginHelper.getLastUsername = getLastUsername;
    window.loginHelper.setLastUsername = setLastUsername;
})();
