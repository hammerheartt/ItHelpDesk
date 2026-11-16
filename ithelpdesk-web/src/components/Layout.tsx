import { NavLink, Outlet } from "react-router";
import { useAuth } from "../auth/AuthContext";
import type { Role } from "../auth/jwt";

const menu: { to: string; label: string; roles: Role[] }[] = [
    { to: '/dashboard', label: 'Dashboard', roles: ['Admin'] },
    { to: '/tickets', label: 'Tickets', roles: ['Employee', 'Technician', 'Admin'] },
    { to: '/tickets/new', label: 'New ticket', roles: ['Employee'] },
    { to: '/users', label: 'Users', roles: ['Admin'] },
    { to: '/categories', label: 'Categories', roles: ['Admin'] },
]

export default function Layout() {
    const { user, logout } = useAuth()
    const items = menu.filter(item => item.roles.includes(user!.role))

    return (
        <>
        <header className="topbar">
            <span className="brand">IT helpdesk</span>
            <nav>
                {items.map(item => (
                    <NavLink key={item.to} to={item.to} end>
                        {item.label}
                    </NavLink>
                ))}
            </nav>
            <span className="user">{user!.email} | {user!.role}</span>
            <button onClick={logout}>Sign out</button>
        </header>
        <main className="content">
            <Outlet />
        </main>
        </>
    )
}