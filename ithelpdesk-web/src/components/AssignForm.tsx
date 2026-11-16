import { useEffect, useState, type SubmitEvent } from 'react'
import { api, ApiError } from '../api/client'
import { TICKET_PRIORITIES, type Ticket, type User } from '../api/types'
import { DEADLINE_LABELS } from '../utils/workflow'

type Props = { ticket: Ticket; onChanged: () => void }

export default function AssignForm({ ticket, onChanged }: Props) {
  const firstAssignment = ticket.status === 'New'
  const [technicians, setTechnicians] = useState<User[]>([])
  const [technicianId, setTechnicianId] = useState('')
  const [priority, setPriority] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api<User[]>('/users?role=Technician&isActive=true')
      .then(setTechnicians)
      .catch(() => setError('Could not load technicians.'))
  }, [])

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    setBusy(true)
    try {
      await api<Ticket>(`/tickets/${ticket.id}/assign`, {
        method: 'POST',
        body: firstAssignment ? { technicianId, priority } : { technicianId },
      })
      setTechnicianId('')
      setPriority('')
      onChanged()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
    } finally {
      setBusy(false)
    }
  }

  const options = technicians.filter(t => t.id !== ticket.assignedToId)

  return (
    <section className="panel">
      <h2>{firstAssignment ? 'Assign' : 'Reassign'}</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Technician
          <select value={technicianId} onChange={e => setTechnicianId(e.target.value)} required>
            <option value="" disabled>Select a technician</option>
            {options.map(t => <option key={t.id} value={t.id}>{t.fullName}</option>)}
          </select>
        </label>

        {firstAssignment && (
          <label>
            Priority
            <select value={priority} onChange={e => setPriority(e.target.value)} required>
              <option value="" disabled>Select a priority</option>
              {TICKET_PRIORITIES.map(p => (
                <option key={p} value={p}>{p} – due in {DEADLINE_LABELS[p]}</option>
              ))}
            </select>
          </label>
        )}

        {error && <p className="error">{error}</p>}
        <button className="primary" type="submit" disabled={busy}>
          {firstAssignment ? 'Assign' : 'Reassign'}
        </button>
      </form>
    </section>
  )
}