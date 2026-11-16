import { Navigate, Outlet } from "react-router";
import { useAuth } from "../auth/AuthContext";

export default function RequireAuth() {
    const { user, loading } = useAuth()

    if (loading) return <p className="page-message">Loading...</p>
    if (!user) return <Navigate to="/login" replace />
    return <Outlet />
}