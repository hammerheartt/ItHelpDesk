import { useParams } from 'react-router'
import NotFoundPage from './NotFoundPage'

export default function TicketPage() {
  const { id } = useParams()
  const ticketId = Number(id)

  if (!Number.isInteger(ticketId) || ticketId <= 0) return <NotFoundPage />

  return (
    <>
      <h1>Ticket #{ticketId}</h1>
      <div className="panel">Ticket details come later.</div>
    </>
  )
}