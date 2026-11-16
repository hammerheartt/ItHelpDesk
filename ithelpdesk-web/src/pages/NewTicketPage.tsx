import { useEffect, useState, type SubmitEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { api, ApiError } from '../api/client'
import type { Category, Ticket } from '../api/types'

export default function NewTicketPage() {
  const navigate = useNavigate()
  const [categories, setCategories] = useState<Category[]>([])
  const [title, setTitle] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [description, setDescription] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({})
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api<Category[]>('/categories')
      .then(setCategories)
      .catch(() => setError('Could not load categories.'))
  }, [])

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setFieldErrors({})
    setError(null)
    setBusy(true)
    try {
      const ticket = await api<Ticket>('/tickets', {
        method: 'POST',
        body: { title, description, categoryId: Number(categoryId) },
      })
      navigate(`/tickets/${ticket.id}`)
    } catch (err) {
      if (err instanceof ApiError && err.errors) setFieldErrors(err.errors)
      else setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
      setBusy(false)
    }
  }

  return (
    <div className="narrow">
      <div className="page-header">
        <h1>New ticket</h1>
      </div>

      <form className="panel" onSubmit={handleSubmit}>
        <label>
          Title
          <input
            value={title}
            onChange={e => setTitle(e.target.value)}
            required
            minLength={5}
            maxLength={200}
          />
          {fieldErrors.Title && <span className="field-error">{fieldErrors.Title[0]}</span>}
        </label>

        <label>
          Category
          <select value={categoryId} onChange={e => setCategoryId(e.target.value)} required>
            <option value="" disabled>Select a category</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          {fieldErrors.CategoryId && <span className="field-error">{fieldErrors.CategoryId[0]}</span>}
        </label>

        <label>
          Description
          <textarea
            rows={8}
            value={description}
            onChange={e => setDescription(e.target.value)}
            placeholder="What happened, what did you expect, and any error message you saw."
            required
            minLength={10}
            maxLength={4000}
          />
          <span className="hint">{description.length} / 4000</span>
          {fieldErrors.Description && <span className="field-error">{fieldErrors.Description[0]}</span>}
        </label>

        {error && <p className="error">{error}</p>}

        <div className="form-actions">
          <button className="primary" type="submit" disabled={busy}>
            {busy ? 'Creating...' : 'Create ticket'}
          </button>
          <Link to="/tickets" className="button">Cancel</Link>
        </div>
      </form>
    </div>
  )
}