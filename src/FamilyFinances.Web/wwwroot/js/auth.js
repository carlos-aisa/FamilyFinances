(() => {
    const LAST_USERNAME_KEY = "ff_last_username";

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
                const error = await response.text();
                return { success: false, error: error || "Login failed" };
            }

            const data = await response.json();
            return { success: true, accessToken: data.accessToken };
        } catch (error) {
            return { success: false, error: error.message };
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
