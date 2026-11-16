import { Navigate, Outlet } from "react-router";
import { useAuth } from "../auth/AuthContext";
import type { Role } from "../auth/jwt";

export function RequireRole({ roles }: { roles: Role[] }) {
    const { user } = useAuth()

    if (!user || !roles.includes(user.role)) return <Navigate to="/" replace />
    return <Outlet />
}