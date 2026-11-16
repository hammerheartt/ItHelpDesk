import { useState, type SubmitEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { ApiError } from "../api/client";
import { Navigate } from "react-router";

export default function LoginPage() {
    const { user, loading, login } = useAuth()
    const [ email, setEmail ] = useState('')
    const [password, setPassword] = useState('')
    const [error, setError] = useState<string | null>(null)
    const [busy, setBusy] = useState(false)

    if (loading) return <p className="page-message">Loading...</p>
    if (user) return <Navigate to="/" replace />

    async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
        e.preventDefault()
        setError(null)
        setBusy(true)
        try {
            await login(email, password)
        } catch (err) {
            setError(err instanceof ApiError ? err.message : 'Cannot reach the server')
        } finally {
            setBusy(false)
        }
    }
    return (
        <div className="login">
            <form className="panel" onSubmit={handleSubmit}>
            <h1>IT Helpdesk</h1>
            <label>
                Email
                <input type="email" value={email} onChange={e => setEmail(e.target.value)} required autoFocus />
            </label>
            <label>
                Password
                <input type="password" value={password} onChange={e => setPassword(e.target.value)} required />
            </label>
            {error && <p className="error">{error}</p>}
            <button className="primary" type="submit" disabled={busy}>
                {busy ? 'Signing in...' : 'Sign in'}
            </button>
            </form>
        </div>
    )
}