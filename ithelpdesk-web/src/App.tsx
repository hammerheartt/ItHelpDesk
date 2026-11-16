import { useAuth } from './auth/AuthContext'
import LoginPage from './pages/LoginPage'

export default function App() {
  const { user, loading, logout } = useAuth()

  if (loading) return <p style={{ margin: 40 }}>Loading...</p>
  if (!user) return <LoginPage />

  return (
    <div className="panel" style={{ margin: 40 }}>
      <h1>ItHelpDesk</h1>
      <p>Signed in as {user.email} ({user.role})</p>
      <button onClick={logout}>Sign out</button>
    </div>
  )
}