import { Navigate, Outlet } from "react-router";
import { useAuth } from "../auth/AuthContext";

export default function RequireAuth() {
    const { user, loading } = useAuth()

    if (loading) return null
    if (!user) return <Navigate to="/login" replace />
    return <Outlet />
}