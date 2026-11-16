import { useEffect, useState, type SubmitEvent } from 'react'
import { api, ApiError } from '../api/client'
import type { Category } from '../api/types'

const sortByName = (list: Category[]) => [...list].sort((a, b) => a.name.localeCompare(b.name))

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[] | null>(null)
  const [newName, setNewName] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [editName, setEditName] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api<Category[]>('/categories')
      .then(setCategories)
      .catch(err => setError(err instanceof ApiError ? err.message : 'Cannot reach the server.'))
  }, [])

  async function run(action: () => Promise<void>) {
    setError(null)
    setBusy(true)
    try {
      await action()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
    } finally {
      setBusy(false)
    }
  }

  function add(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    run(async () => {
      const created = await api<Category>('/categories', { method: 'POST', body: { name: newName.trim() } })
      setCategories(list => sortByName([...(list ?? []), created]))
      setNewName('')
    })
  }

  function update(category: Category, name: string, isActive: boolean) {
    run(async () => {
      const updated = await api<Category>(`/categories/${category.id}`, {
        method: 'PUT',
        body: { name, isActive },
      })
      setCategories(list => sortByName((list ?? []).map(c => (c.id === updated.id ? updated : c))))
      setEditingId(null)
    })
  }

  function remove(category: Category) {
    if (!window.confirm(`Delete category "${category.name}"?`)) return
    run(async () => {
      await api<void>(`/categories/${category.id}`, { method: 'DELETE' })
      setCategories(list => (list ?? []).filter(c => c.id !== category.id))
    })
  }

  function startRename(category: Category) {
    setEditingId(category.id)
    setEditName(category.name)
    setError(null)
  }

  return (
    <div className="narrow">
      <div className="page-header">
        <h1>Categories</h1>
      </div>

      <form className="panel inline-form" onSubmit={add}>
        <input
          value={newName}
          onChange={e => setNewName(e.target.value)}
          placeholder="New category name"
          required
          minLength={2}
          maxLength={100}
        />
        <button className="primary" type="submit" disabled={busy}>Add</button>
      </form>

      {error && <p className="error">{error}</p>}

      {categories && (
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {categories.map(c => (
              <tr key={c.id}>
                {editingId === c.id ? (
                  <td colSpan={3}>
                    <form
                      className="inline-form"
                      onSubmit={e => {
                        e.preventDefault()
                        update(c, editName.trim(), c.isActive)
                      }}
                    >
                      <input
                        value={editName}
                        onChange={e => setEditName(e.target.value)}
                        required
                        minLength={2}
                        maxLength={100}
                        autoFocus
                      />
                      <button className="primary" type="submit" disabled={busy}>Save</button>
                      <button type="button" onClick={() => setEditingId(null)}>Cancel</button>
                    </form>
                  </td>
                ) : (
                  <>
                    <td>{c.name}</td>
                    <td>
                      <span className={c.isActive ? 'badge active' : 'badge inactive'}>
                        {c.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="row-actions">
                      <button onClick={() => startRename(c)} disabled={busy}>Rename</button>
                      <button onClick={() => update(c, c.name, !c.isActive)} disabled={busy}>
                        {c.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                      <button onClick={() => remove(c)} disabled={busy}>Delete</button>
                    </td>
                  </>
                )}
              </tr>
            ))}
            {categories.length === 0 && (
              <tr><td colSpan={3} className="empty">No categories yet.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  )
}