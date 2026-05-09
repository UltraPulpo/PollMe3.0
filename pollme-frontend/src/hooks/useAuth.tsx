import { createContext, useContext, useEffect, useState } from 'react';
import type { Creator } from '../types';
import * as authApi from '../api/authApi';

interface AuthContextValue {
    creator: Creator | null;
    isLoading: boolean;
    login: (username: string, password: string) => Promise<void>;
    register: (username: string, password: string) => Promise<void>;
    logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [creator, setCreator] = useState<Creator | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        authApi.me()
            .then(setCreator)
            .catch(() => setCreator(null))
            .finally(() => setIsLoading(false));
    }, []);

    const login = async (username: string, password: string) => {
        const c = await authApi.login(username, password);
        setCreator(c);
    };

    const register = async (username: string, password: string) => {
        const c = await authApi.register(username, password);
        setCreator(c);
    };

    const logout = async () => {
        await authApi.logout();
        setCreator(null);
    };

    return (
        <AuthContext.Provider value={{ creator, isLoading, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth(): AuthContextValue {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}
