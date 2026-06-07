import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authApi } from '../api/authApi';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    // On app load — restore session from localStorage
    useEffect(() => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            try {
                const decoded = jwtDecode(token);
                // Check if token is still valid
                if (decoded.exp * 1000 > Date.now()) {
                    const storedUser = localStorage.getItem('user');
                    if (storedUser) setUser(JSON.parse(storedUser));
                } else {
                    // Token expired — clear storage
                    localStorage.clear();
                }
            } catch {
                localStorage.clear();
            }
        }
        setLoading(false);
    }, []);

    const login = useCallback(async (email, password) => {
        const response = await authApi.login({ email, password });
        const { data } = response.data; // ApiResponseDto wrapper

        // Store tokens
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('user', JSON.stringify(data.user));

        setUser(data.user);
        return data.user; // Return for redirect logic
    }, []);

    const logout = useCallback(async () => {
        try {
            await authApi.logout();
        } catch {
            // Even if API fails, clear local storage
        } finally {
            localStorage.clear();
            setUser(null);
        }
    }, []);

    const isAdmin = user?.role === 'SuperAdmin' || user?.role === 'Admin';
    const isSuperAdmin = user?.role === 'SuperAdmin';

    return (
        <AuthContext.Provider value={{
            user,
            loading,
            login,
            logout,
            isAdmin,
            isSuperAdmin,
            isAuthenticated: !!user
        }}>
            {children}
        </AuthContext.Provider>
    );
};

// Custom hook for easy access
export const useAuth = () => {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
    return ctx;
};