import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { api, ApiError } from '../api/client'
import type { Ticket } from '../api/types'
import TicketForm, { type TicketFormValues } from '../components/TicketForm'
import NotFoundPage from './NotFoundPage'

type LoadError = { ticketId: number; status: number; message: string }

export default function EditTicketPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const ticketId = Number(id)
  const validId = Number.isInteger(ticketId) && ticketId > 0

  const [ticket, setTicket] = useState<Ticket | null>(null)
  const [loadError, setLoadError] = useState<LoadError | null>(null)

  useEffect(() => {
    if (!validId) return
    let ignore = false

    api<Ticket>(`/tickets/${ticketId}`)
      .then(t => { if (!ignore) setTicket(t) })
      .catch(err => {
        if (!ignore) {
          setLoadError({
            ticketId,
            status: err instanceof ApiError ? err.status : 0,
            message: err instanceof ApiError ? err.message : 'Cannot reach the server.',
          })
        }
      })

    return () => { ignore = true }
  }, [ticketId, validId])

  if (!validId) return <NotFoundPage />
  if (loadError?.ticketId === ticketId) {
    return loadError.status === 404 ? <NotFoundPage /> : <p className="error">{loadError.message}</p>
  }
  if (ticket?.id !== ticketId) return null

  if (ticket.status !== 'New') {
    return (
      <div className="narrow">
        <div className="page-header">
          <h1>Edit ticket #{ticket.id}</h1>
        </div>
        <div className="panel">
          <p>Only new tickets can be edited. This ticket is already being handled.</p>
          <Link to={`/tickets/${ticket.id}`} className="button">Back to ticket</Link>
        </div>
      </div>
    )
  }

  async function save(values: TicketFormValues) {
    await api<Ticket>(`/tickets/${ticketId}`, { method: 'PUT', body: values })
    navigate(`/tickets/${ticketId}`)
  }

  return (
    <div className="narrow">
      <div className="page-header">
        <h1>Edit ticket #{ticket.id}</h1>
      </div>
      <TicketForm
        key={ticket.id}
        initial={{ title: ticket.title, description: ticket.description, categoryId: ticket.categoryId }}
        currentCategory={{ id: ticket.categoryId, name: ticket.categoryName }}
        submitLabel="Save changes"
        cancelTo={`/tickets/${ticket.id}`}
        onSubmit={save}
      />
    </div>
  )
}