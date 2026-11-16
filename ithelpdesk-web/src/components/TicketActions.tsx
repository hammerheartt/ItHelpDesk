import { useState, type SubmitEvent } from 'react'
import { Link } from 'react-router'
import { api, ApiError } from '../api/client'
import type { Ticket, TicketStatus } from '../api/types'
import type { Role } from '../auth/jwt'
import { allowedTransitions } from '../utils/workflow'

const ACTION_LABELS: Partial<Record<TicketStatus, string>> = {
  InProgress: 'Start work',
  Resolved: 'Resolve',
  Closed: 'Close ticket',
  Reopened: 'Reopen',
  Cancelled: 'Cancel ticket',
}

const NOTE_LABELS: Partial<Record<TicketStatus, string>> = {
  Resolved: 'Resolution note (visible to the employee)',
  Reopened: 'Why is the problem not solved?',
}

type Props = { ticket: Ticket; role: Role; onChanged: () => void }

export default function TicketActions({ ticket, role, onChanged }: Props) {
  const [noteFor, setNoteFor] = useState<TicketStatus | null>(null)
  const [note, setNote] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const actions = allowedTransitions(ticket, role)
  const canEdit = role === 'Employee' && ticket.status === 'New'
  if (actions.length === 0 && !canEdit) return null

  async function changeStatus(to: TicketStatus, comment?: string) {
    setError(null)
    setBusy(true)
    try {
      await api<Ticket>(`/tickets/${ticket.id}/status`, {
        method: 'PATCH',
        body: { status: to, comment },
      })
      setNoteFor(null)
      setNote('')
      onChanged()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
    } finally {
      setBusy(false)
    }
  }

  function handleClick(to: TicketStatus) {
    if (NOTE_LABELS[to]) {
      setNoteFor(to)
      setNote('')
      return
    }
    if (to === 'Cancelled' && !window.confirm('Cancel this ticket? This cannot be undone.')) return
    changeStatus(to)
  }

  function handleNoteSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    if (noteFor) changeStatus(noteFor, note)
  }

  return (
    <section className="panel">
      <h2>Actions</h2>

      {noteFor ? (
        <form onSubmit={handleNoteSubmit}>
          <label>
            {NOTE_LABELS[noteFor]}
            <textarea rows={4} value={note} onChange={e => setNote(e.target.value)} required maxLength={2000} autoFocus />
          </label>
          <div className="form-actions">
            <button className="primary" type="submit" disabled={busy}>{ACTION_LABELS[noteFor]}</button>
            <button type="button" onClick={() => setNoteFor(null)}>Back</button>
          </div>
        </form>
      ) : (
        <div className="actions">
          {canEdit && <Link to={`/tickets/${ticket.id}/edit`} className="button">Edit</Link>}
          {actions.map((to, i) => (
            <button key={to} className={i === 0 && to !== 'Cancelled' ? 'primary' : ''} disabled={busy} onClick={() => handleClick(to)}>
              {ACTION_LABELS[to]}
            </button>
        ))}
        </div>
      )}

      {error && <p className="error">{error}</p>}
    </section>
  )
}