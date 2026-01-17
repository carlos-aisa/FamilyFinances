window.authHelper = {
    login: async function (email, password) {
        try {
            const response = await fetch('/auth/session', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email, password }),
                credentials: 'include' // IMPORTANTE: incluir cookies
            });

            if (!response.ok) {
                const error = await response.text();
                return { success: false, error: error || 'Login failed' };
            }

            const data = await response.json();
            return { success: true, accessToken: data.accessToken };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }
};