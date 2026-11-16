import { useEffect, useState, type SubmitEvent } from 'react'
import { api, ApiError } from '../api/client'
import type { User } from '../api/types'
import type { Role } from '../auth/jwt'
import { useAuth } from '../auth/AuthContext'

const ROLES: Role[] = ['Employee', 'Technician', 'Admin']
const EMPTY_FORM = { fullName: '', email: '', password: '', role: 'Employee' as Role}

const sortByName = (list: User[]) => [...list].sort((a, b) => a.fullName.localeCompare(b.fullName))

function messagesOf(err: unknown): string[] {
  if (err instanceof ApiError) return err.errors ? Object.values(err.errors).flat() : [err.message]
  return ['Cannot reach the server.']
}

export default function UsersPage() {
    const { user: me } = useAuth()
    const [users, setUsers] = useState<User[] | null>(null)
    const [form, setForm] = useState(EMPTY_FORM)
    const [roleFilter, setRoleFilter] = useState<Role | ''>('')
    const [errors, setErrors] = useState<string[]>([])
    const [busy, setBusy] = useState(false)

    useEffect(() => {
        api<User[]>('/users')
            .then(list => setUsers(sortByName(list)))
            .catch(err => setErrors(messagesOf(err))) 
    }, [])

    async function run(action: () => Promise<void>) {
        setErrors([])
        setBusy(true)
        try {
            await action()
        } catch(err) {
            setErrors(messagesOf(err))
        } finally {
            setBusy(false)
        }
    }

    function replaceUser(updated: User) {
        setUsers(list => (list ?? []).map(u => (u.id === updated.id ? updated : u)))
    }

    function create(e: SubmitEvent<HTMLFormElement>) {
        e.preventDefault()
        run(async () => {
            const created = await api<User>('/users', {
                method: 'POST',
                body: form
            })
            setUsers(list => sortByName([...(list ?? []), created]))
            setForm(EMPTY_FORM)
        })
    }

    function changeRole(user: User, role: Role) {
        if (!window.confirm(`Change ${user.fullName}'s role to ${role}? They will have to sign in again.`)) return
        run(async () => {
        replaceUser(await api<User>(`/users/${user.id}/role`, { method: 'PATCH', body: { role } }))
    })
  }

  function toggleActive(user: User) {
        if (user.isActive && !window.confirm(`Deactivate ${user.fullName}? They will be signed out within 10 minutes.`)) return
        run(async () => {
        replaceUser(await api<User>(`/users/${user.id}/active`, { method: 'PATCH', body: { isActive: !user.isActive } }))
    })
  }
const visible = (users ?? []).filter(u => roleFilter === '' || u.role === roleFilter)

  return (
    <>
      <div className="page-header">
        <h1>Users</h1>
      </div>

      <form className="panel add-user" onSubmit={create}>
        <h2>Add user</h2>
        <div className="form-grid">
          <label>
            Full name
            <input
              value={form.fullName}
              onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))}
              required
              maxLength={100}
            />
          </label>
          <label>
            Email
            <input
              type="email"
              value={form.email}
              onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
              autoComplete="off"
              required
              maxLength={100}
            />
          </label>
          <label>
            Initial password
            <input
              type="password"
              value={form.password}
              onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
              autoComplete="new-password"
              required
            />
          </label>
          <label>
            Role
            <select value={form.role} onChange={e => setForm(f => ({ ...f, role: e.target.value as Role }))}>
              {ROLES.map(r => <option key={r} value={r}>{r}</option>)}
            </select>
          </label>
        </div>
        <button className="primary" type="submit" disabled={busy}>Add user</button>
      </form>

      {errors.length > 0 && (
        <ul className="error-list">
          {errors.map((message, i) => <li key={i}>{message}</li>)}
        </ul>
      )}

      <div className="filters">
        <select value={roleFilter} onChange={e => setRoleFilter(e.target.value as Role | '')}>
          <option value="">All roles</option>
          {ROLES.map(r => <option key={r} value={r}>{r}</option>)}
        </select>
      </div>

      {users && (
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {visible.map(u => {
              const isMe = u.id === me!.id
              return (
                <tr key={u.id}>
                  <td>{u.fullName}{isMe && <span className="muted"> (you)</span>}</td>
                  <td>{u.email}</td>
                  <td>
                    <select
                      className="compact"
                      value={u.role}
                      disabled={busy || isMe}
                      onChange={e => changeRole(u, e.target.value as Role)}
                    >
                      {ROLES.map(r => <option key={r} value={r}>{r}</option>)}
                    </select>
                  </td>
                  <td>
                    <span className={u.isActive ? 'badge active' : 'badge inactive'}>
                      {u.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="row-actions">
                    {!isMe && (
                      <button onClick={() => toggleActive(u)} disabled={busy}>
                        {u.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                    )}
                  </td>
                </tr>
              )
            })}
            {visible.length === 0 && (
              <tr><td colSpan={5} className="empty">No users found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </>
  )
}