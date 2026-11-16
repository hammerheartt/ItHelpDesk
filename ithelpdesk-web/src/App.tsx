import { Navigate, Route, Routes } from 'react-router'
import { useAuth } from './auth/AuthContext'
import RequireAuth from './components/RequireAuth'
import RequireRole from './components/RequireRole'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import TicketsPage from './pages/TicketsPage'
import TicketPage from './pages/TicketPage'
import NewTicketPage from './pages/NewTicketPage'
import DashboardPage from './pages/DashboardPage'
import UsersPage from './pages/UsersPage'
import CategoriesPage from './pages/CategoriesPage'
import NotFoundPage from './pages/NotFoundPage'


function Home() {
    const { user } = useAuth()
    return <Navigate to={user!.role === 'Admin' ? '/dashboard' : '/tickets'} replace />
}
export default function App() {
  return (
    <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<RequireAuth />}>
            <Route element={<Layout />}>
                <Route index element={<Home />} />
                <Route path="tickets" element={<TicketsPage />} />
                <Route path="tickets/:id" element={<TicketPage />} />

                <Route element={<RequireRole roles={['Employee']} />}>
                    <Route path="tickets/new" element={<NewTicketPage />} />
                </Route>

                 <Route element={<RequireRole roles={['Admin']} />}>
                    <Route path="dashboard" element={<DashboardPage />} />
                    <Route path="users" element={<UsersPage />} />
                    <Route path="categories" element={<CategoriesPage />} />
                </Route>

                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Route>
    </Routes>
  )
}