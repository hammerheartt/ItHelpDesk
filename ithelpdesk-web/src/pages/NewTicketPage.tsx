import { useNavigate } from 'react-router'
import { api } from '../api/client'
import type { Ticket } from '../api/types'
import TicketForm, { type TicketFormValues } from '../components/TicketForm'

export default function NewTicketPage() {
  const navigate = useNavigate()

  async function create(values: TicketFormValues) {
    const ticket = await api<Ticket>('/tickets', { method: 'POST', body: values })
    navigate(`/tickets/${ticket.id}`)
  }

  return (
    <div className="narrow">
      <div className="page-header">
        <h1>New ticket</h1>
      </div>
      <TicketForm submitLabel="Create ticket" cancelTo="/tickets" onSubmit={create} />
    </div>
  )
}