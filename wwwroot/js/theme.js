// Theme management for dark/light mode
(function() {
    const STORAGE_KEY = 'ff_theme';
    
    // Get stored theme or default to 'dark'
    function getStoredTheme() {
        return localStorage.getItem(STORAGE_KEY) || 'dark';
    }
    
    // Set theme
    function setTheme(theme) {
        localStorage.setItem(STORAGE_KEY, theme);
        document.documentElement.setAttribute('data-bs-theme', theme);
        console.log('Theme set to:', theme);
    }
    
    // Initialize theme immediately (before content renders)
    const initialTheme = getStoredTheme();
    setTheme(initialTheme);
    
    // Expose functions to Blazor
    window.themeHelper = {
        getTheme: function() {
            return getStoredTheme();
        },
        setTheme: function(theme) {
            setTheme(theme);
            return theme;
        },
        toggleTheme: function() {
            const current = getStoredTheme();
            const newTheme = current === 'dark' ? 'light' : 'dark';
            setTheme(newTheme);
            return newTheme;
        }
    };
    
    console.log('Theme helper initialized with theme:', initialTheme);
})();