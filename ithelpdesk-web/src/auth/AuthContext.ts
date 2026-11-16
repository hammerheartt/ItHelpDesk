import { createContext, useContext } from "react";
import type { CurrentUser } from "./jwt";

export type AuthState = {
    user: CurrentUser | null
    loading: boolean
    login: (email: string, password: string) => Promise<void>
    logout: () => Promise<void>
}

export const AuthContext = createContext<AuthState | null>(null)

export function useAuth(): AuthState {
    const auth = useContext(AuthContext)
    if (!auth) throw new Error('useAuth must be used inside <AuthProvider>')
    return auth
}