import { useParams } from 'react-router'

export default function TicketPage() {
  const { id } = useParams()
  return (
    <>
      <h1>Ticket #{id}</h1>
      <div className="panel">Ticket details come later.</div>
    </>
  )
}