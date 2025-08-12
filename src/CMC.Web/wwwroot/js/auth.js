window.blazorCulture = {
    setAuthCookie: (cookieValue) => {
        document.cookie = `AuthCookie=${cookieValue}; path=/; max-age=2592000; secure; samesite=strict`;
    },
    clearAuthCookie: () => {
        document.cookie = "AuthCookie=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
};
