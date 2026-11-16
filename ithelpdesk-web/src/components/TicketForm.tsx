import { useEffect, useState, type SubmitEvent } from 'react'
import { Link } from 'react-router'
import { api, ApiError } from '../api/client'
import type { Category } from '../api/types'

export type TicketFormValues = {
  title: string
  description: string
  categoryId: number
}

type Props = {
  initial?: TicketFormValues
  currentCategory?: { id: number; name: string }
  submitLabel: string
  cancelTo: string
  onSubmit: (values: TicketFormValues) => Promise<void>
}

export default function TicketForm({ initial, currentCategory, submitLabel, cancelTo, onSubmit }: Props) {
  const [categories, setCategories] = useState<Category[] | null>(null)
  const [title, setTitle] = useState(initial?.title ?? '')
  const [categoryId, setCategoryId] = useState(initial ? String(initial.categoryId) : '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({})
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api<Category[]>('/categories')
      .then(setCategories)
      .catch(() => setError('Could not load categories.'))
  }, [])

  const options =
    categories && currentCategory && !categories.some(c => c.id === currentCategory.id)
      ? [...categories, { id: currentCategory.id, name: `${currentCategory.name} (inactive)`, isActive: false }]
      : categories ?? []

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setFieldErrors({})
    setError(null)
    setBusy(true)
    try {
      await onSubmit({ title, description, categoryId: Number(categoryId) })
    } catch (err) {
      if (err instanceof ApiError && err.errors) setFieldErrors(err.errors)
      else setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
      setBusy(false)
    }
  }

  return (
    <form className="panel" onSubmit={handleSubmit}>
      <label>
        Title
        <input value={title} onChange={e => setTitle(e.target.value)} required minLength={5} maxLength={200} />
        {fieldErrors.Title && <span className="field-error">{fieldErrors.Title[0]}</span>}
      </label>

      <label>
        Category
        <select value={categoryId} onChange={e => setCategoryId(e.target.value)} required>
          <option value="" disabled>Select a category</option>
          {options.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
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
        <button className="primary" type="submit" disabled={busy}>{busy ? 'Saving...' : submitLabel}</button>
        <Link to={cancelTo} className="button">Cancel</Link>
      </div>
    </form>
  )
}