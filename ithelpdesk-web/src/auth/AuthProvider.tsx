import { useEffect, useState, type ReactNode } from "react";
import { AuthContext } from "./AuthContext";
import { readUser, type CurrentUser } from "./jwt";
import { login as apiLogin, logout as apiLogout, restoreSession } from '../api/client'

export function AuthProvider({ children }: { children: ReactNode}) {
    const [user, setUser] = useState<CurrentUser | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        restoreSession()
            .then(token => { if (token) setUser(readUser(token))})
            .finally(() => setLoading(false))
    }, [])

    async function login(email: string, password: string) {
        const token = await apiLogin(email, password)
        setUser(readUser(token))
    }

    async function logout() {
        await apiLogout()
        setUser(null)
    }

    return (
        <AuthContext.Provider value={{ user, loading, login, logout }}>
            {children}
        </AuthContext.Provider>
    )
}